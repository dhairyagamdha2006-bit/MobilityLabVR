using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MobilityLabVR.Editor
{
    public static class BuildAutomation
    {
        [MenuItem("Tools/MobilityLab VR/Build Development Player", priority = 20)]
        public static void PerformDevelopmentBuild()
        {
            ProjectSetup.CreateOrRebuildDemo();
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled scenes are available to build.");

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string output = OutputPath(target);
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? "Builds");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = target,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Development build failed with {report.summary.totalErrors} error(s). See the Editor log.");
            }
            Debug.Log($"MobilityLab VR development build succeeded: {output} ({report.summary.totalSize} bytes)");
        }

        private static string OutputPath(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Builds/Windows/MobilityLabVR.exe";
                case BuildTarget.StandaloneOSX:
                    return "Builds/macOS/MobilityLabVR.app";
                case BuildTarget.StandaloneLinux64:
                    return "Builds/Linux/MobilityLabVR";
                default:
                    throw new NotSupportedException($"The automated desktop build does not support target {target}.");
            }
        }
    }
}
