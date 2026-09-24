using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MobilityLabVR.Editor
{
    public static class ProjectSetup
    {
        private const string Root = "Assets/MobilityLabVR";
        private const string SceneDirectory = Root + "/Scenes";
        private const string ResourceDirectory = Root + "/Resources/Scenarios";
        private const string RenderDirectory = Root + "/Config/Rendering";

        [MenuItem("Tools/MobilityLab VR/Create or Rebuild Demo", priority = 1)]
        public static void CreateOrRebuildDemo()
        {
            EnsureFolder(SceneDirectory);
            EnsureFolder(ResourceDirectory);
            EnsureFolder(RenderDirectory);
            CreateScenarioAssets();
            TryCreateUniversalRenderPipeline();
            CreateBootstrapScene<MainMenuBootstrap>(SceneDirectory + "/MainMenu.unity", "MainMenu Bootstrap");
            CreateBootstrapScene<SimulationSceneBootstrap>(SceneDirectory + "/Simulation.unity", "Simulation Bootstrap");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneDirectory + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneDirectory + "/Simulation.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MobilityLab VR demo rebuilt successfully. Open MainMenu.unity and press Play.");
        }

        [MenuItem("Tools/MobilityLab VR/Validate Project", priority = 2)]
        public static void ValidateProject()
        {
            string[] requiredFiles =
            {
                SceneDirectory + "/MainMenu.unity",
                SceneDirectory + "/Simulation.unity",
                "Packages/manifest.json",
                "Analysis/analyze_sessions.py",
                "Analysis/train_risk_model.py",
                "Documentation/TelemetrySchema.md"
            };
            string[] missing = requiredFiles.Where(path => !File.Exists(path)).ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException("MobilityLab VR validation failed. Missing: " + string.Join(", ", missing));
            }

            if (EditorBuildSettings.scenes.Length < 2 || EditorBuildSettings.scenes.Any(scene => !scene.enabled))
            {
                throw new InvalidOperationException("MobilityLab VR validation failed: both scenes must be enabled in Build Settings.");
            }

            Debug.Log("MobilityLab VR project structure and Build Settings validation passed.");
        }

        private static void CreateScenarioAssets()
        {
            var defaults = DefaultScenarioFactory.CreateBuiltInDefinitions();
            foreach (ScenarioDefinition definition in defaults)
            {
                string path = $"{ResourceDirectory}/{definition.Kind}.asset";
                ScenarioDefinition existing = AssetDatabase.LoadAssetAtPath<ScenarioDefinition>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(definition), path);
                }
                else
                {
                    existing.Initialize(definition.Kind, definition.DisplayName, definition.Description,
                        definition.SeedOffset, definition.Difficulty, definition.TrialTimeoutSeconds,
                        definition.TriggerDistanceFromIntersection, definition.FogDensity,
                        definition.AmbientIntensity, definition.HazardSpeedMetersPerSecond);
                    EditorUtility.SetDirty(existing);
                }
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        private static void CreateBootstrapScene<T>(string path, string objectName) where T : Component
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject(objectName, typeof(T));
            bootstrap.transform.position = Vector3.zero;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new IOException("Could not save generated scene at " + path);
            }
        }

        private static void TryCreateUniversalRenderPipeline()
        {
            const string pipelinePath = RenderDirectory + "/MobilityLabUniversalRenderPipeline.asset";
            if (AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath) != null)
            {
                GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
                QualitySettings.renderPipeline = GraphicsSettings.defaultRenderPipeline;
                return;
            }

            Type rendererType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
            Type pipelineType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            if (rendererType == null || pipelineType == null)
            {
                Debug.LogWarning("URP package types were not available yet. Re-run Create or Rebuild Demo after package import.");
                return;
            }

            string rendererPath = RenderDirectory + "/MobilityLabForwardRenderer.asset";
            ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererType);
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }
            MethodInfo createMethod = pipelineType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "Create" && method.GetParameters().Length == 1 &&
                                          method.GetParameters()[0].ParameterType.IsAssignableFrom(rendererType));
            if (createMethod == null)
            {
                Debug.LogWarning("URP asset factory API was not found. Rendering will use the current project pipeline.");
                return;
            }

            RenderPipelineAsset pipeline = createMethod.Invoke(null, new object[] { rendererData }) as RenderPipelineAsset;
            if (pipeline == null)
            {
                Debug.LogWarning("URP asset factory returned no pipeline. Rendering will use the current project pipeline.");
                return;
            }

            AssetDatabase.CreateAsset(pipeline, pipelinePath);
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segments[index]);
                current = next;
            }
        }
    }
}
