using UnityEngine;

namespace MobilityLabVR
{
    public sealed class WorldBuildResult
    {
        public Light Sun;
    }

    public static class CampusEnvironmentBuilder
    {
        public static WorldBuildResult Build(Transform root, TrafficSignalController signals)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.68f, 0.71f);
            RenderSettings.ambientIntensity = 0.85f;
            RenderSettings.fog = false;
            RenderSettings.fogColor = new Color(0.68f, 0.76f, 0.8f);
            RenderSettings.fogDensity = 0.008f;

            GameObject ground = RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Ground", root,
                new Vector3(0f, -0.25f, 0f), new Vector3(120f, 0.5f, 120f), MobilityLabPalette.Grass, true);
            ground.layer = 0;

            CreateRoad(root, new Vector3(0f, 0.01f, 0f), new Vector3(16f, 0.08f, 110f));
            CreateRoad(root, new Vector3(0f, 0.02f, 0f), new Vector3(110f, 0.08f, 16f));
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Northbound Bike Lane", root,
                new Vector3(5.5f, 0.075f, 0f), new Vector3(2.2f, 0.035f, 110f),
                new Color32(40, 103, 95, 255));

            CreateSidewalks(root);
            CreateRoadMarkings(root);
            CreateCrosswalks(root);
            CreateBuildings(root);
            CreateLandscaping(root);
            CreateStreetlights(root);
            CreateSigns(root);
            CreateTrafficLights(root, signals);

            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Start Line", root,
                new Vector3(5.5f, 0.11f, -38f), new Vector3(2.1f, 0.03f, 0.45f), MobilityLabPalette.Sky);
            RuntimePrimitiveFactory.WorldText("Start Label", root, "START", new Vector3(5.5f, 0.13f, -40f),
                Quaternion.Euler(90f, 0f, 0f), 0.22f, Color.white);

            GameObject sunObject = new GameObject("Sun", typeof(Light));
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            Light sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.94f, 0.82f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;

            return new WorldBuildResult { Sun = sun };
        }

        private static void CreateRoad(Transform root, Vector3 position, Vector3 scale)
        {
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Road", root, position, scale,
                MobilityLabPalette.Asphalt, false);
        }

        private static void CreateSidewalks(Transform root)
        {
            Vector3[] positions =
            {
                new Vector3(-32f, 0.15f, -32f), new Vector3(32f, 0.15f, -32f),
                new Vector3(-32f, 0.15f, 32f), new Vector3(32f, 0.15f, 32f)
            };
            foreach (Vector3 position in positions)
            {
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Sidewalk Plaza", root, position,
                    new Vector3(48f, 0.3f, 48f), MobilityLabPalette.Concrete, true);
            }
        }

        private static void CreateRoadMarkings(Transform root)
        {
            Color white = new Color32(225, 229, 224, 255);
            Color yellow = new Color32(235, 188, 52, 255);
            for (int index = -5; index <= 5; index++)
            {
                float offset = index * 9f;
                if (Mathf.Abs(offset) < 10f) continue;
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "NS Center Dash", root,
                    new Vector3(0f, 0.09f, offset), new Vector3(0.18f, 0.025f, 4.5f), yellow);
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "EW Center Dash", root,
                    new Vector3(offset, 0.1f, 0f), new Vector3(4.5f, 0.025f, 0.18f), yellow);
            }
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Bike Lane Divider", root,
                new Vector3(4.3f, 0.1f, -31f), new Vector3(0.14f, 0.025f, 42f), white);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Bike Lane Divider North", root,
                new Vector3(4.3f, 0.1f, 31f), new Vector3(0.14f, 0.025f, 42f), white);

            for (int z = -32; z <= 32; z += 16)
            {
                if (Mathf.Abs(z) < 10) continue;
                RuntimePrimitiveFactory.WorldText("Bike Symbol", root, "◇", new Vector3(5.5f, 0.12f, z),
                    Quaternion.Euler(90f, 0f, 0f), 0.45f, white);
            }
        }

        private static void CreateCrosswalks(Transform root)
        {
            Color stripe = new Color32(232, 234, 226, 255);
            for (int index = -4; index <= 4; index++)
            {
                float offset = index * 1.45f;
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "South Crosswalk Stripe", root,
                    new Vector3(offset, 0.115f, -8.8f), new Vector3(0.75f, 0.025f, 3.2f), stripe);
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "North Crosswalk Stripe", root,
                    new Vector3(offset, 0.115f, 8.8f), new Vector3(0.75f, 0.025f, 3.2f), stripe);
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "West Crosswalk Stripe", root,
                    new Vector3(-8.8f, 0.12f, offset), new Vector3(3.2f, 0.025f, 0.75f), stripe);
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "East Crosswalk Stripe", root,
                    new Vector3(8.8f, 0.12f, offset), new Vector3(3.2f, 0.025f, 0.75f), stripe);
            }
        }

        private static void CreateBuildings(Transform root)
        {
            CreateBuilding(root, "Engineering Hall", new Vector3(-31f, 5.2f, 28f), new Vector3(30f, 10f, 24f),
                new Color32(185, 154, 116, 255));
            CreateBuilding(root, "Research Commons", new Vector3(31f, 4.2f, 29f), new Vector3(27f, 8f, 22f),
                new Color32(124, 151, 159, 255));
            CreateBuilding(root, "Student Center", new Vector3(-32f, 3.8f, -29f), new Vector3(28f, 7.2f, 20f),
                new Color32(194, 132, 96, 255));
            CreateBuilding(root, "Mobility Lab", new Vector3(33f, 4.8f, -29f), new Vector3(30f, 9.2f, 22f),
                new Color32(98, 135, 150, 255));
        }

        private static void CreateBuilding(Transform root, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject building = RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, name, root, position, scale, color, true);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Roof", building.transform,
                new Vector3(0f, 0.53f, 0f), new Vector3(1.04f, 0.06f, 1.04f), MobilityLabPalette.Navy);
            RuntimePrimitiveFactory.WorldText("Building Label", building.transform, name.ToUpperInvariant(),
                new Vector3(0f, 0.05f, -0.505f), Quaternion.identity, 0.04f, MobilityLabPalette.OffWhite);
            for (int floor = 0; floor < 2; floor++)
            {
                for (int window = -2; window <= 2; window++)
                {
                    RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Window", building.transform,
                        new Vector3(window * 0.16f, -0.18f + (floor * 0.3f), -0.506f),
                        new Vector3(0.09f, 0.15f, 0.01f), new Color32(91, 178, 198, 255), false, null, true);
                }
            }
        }

        private static void CreateLandscaping(Transform root)
        {
            Vector3[] treePositions =
            {
                new Vector3(-15f, 0.2f, -24f), new Vector3(17f, 0.2f, -24f),
                new Vector3(-18f, 0.2f, 22f), new Vector3(19f, 0.2f, 23f),
                new Vector3(-47f, 0.2f, 8f), new Vector3(46f, 0.2f, -10f),
                new Vector3(-44f, 0.2f, -47f), new Vector3(44f, 0.2f, 47f)
            };
            foreach (Vector3 position in treePositions)
            {
                GameObject tree = new GameObject("Low Poly Tree");
                tree.transform.SetParent(root, false);
                tree.transform.localPosition = position;
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Trunk", tree.transform,
                    new Vector3(0f, 1.8f, 0f), new Vector3(0.45f, 1.8f, 0.45f), new Color32(96, 67, 47, 255));
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Canopy", tree.transform,
                    new Vector3(0f, 4.2f, 0f), new Vector3(3.4f, 3f, 3.4f), new Color32(65, 123, 72, 255));
            }
        }

        private static void CreateStreetlights(Transform root)
        {
            Vector3[] positions =
            {
                new Vector3(-11f, 0.3f, -20f), new Vector3(11f, 0.3f, -20f),
                new Vector3(-11f, 0.3f, 20f), new Vector3(11f, 0.3f, 20f),
                new Vector3(-22f, 0.3f, -11f), new Vector3(22f, 0.3f, 11f)
            };
            foreach (Vector3 position in positions)
            {
                GameObject light = new GameObject("Streetlight");
                light.transform.SetParent(root, false);
                light.transform.localPosition = position;
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Pole", light.transform,
                    new Vector3(0f, 2.7f, 0f), new Vector3(0.13f, 2.7f, 0.13f), MobilityLabPalette.Navy);
                RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Lamp", light.transform,
                    new Vector3(0f, 5.45f, 0f), new Vector3(0.38f, 0.24f, 0.38f), MobilityLabPalette.Amber, false, null, true);
            }
        }

        private static void CreateSigns(Transform root)
        {
            CreateSign(root, new Vector3(11.5f, 0.2f, -28f), Quaternion.Euler(0f, 180f, 0f), "BIKE ROUTE ↑");
            CreateSign(root, new Vector3(-12f, 0.2f, 17f), Quaternion.Euler(0f, 0f, 0f), "MOBILITY LAB →");
            CreateSign(root, new Vector3(18f, 0.2f, 11.5f), Quaternion.Euler(0f, -90f, 0f), "CROSSWALK");
        }

        private static void CreateSign(Transform root, Vector3 position, Quaternion rotation, string text)
        {
            GameObject sign = new GameObject("Directional Sign");
            sign.transform.SetParent(root, false);
            sign.transform.localPosition = position;
            sign.transform.localRotation = rotation;
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Post", sign.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.08f, 1.25f, 0.08f), MobilityLabPalette.Navy);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Board", sign.transform,
                new Vector3(0f, 2.35f, 0f), new Vector3(2.9f, 0.65f, 0.12f), MobilityLabPalette.Navy);
            RuntimePrimitiveFactory.WorldText("Text", sign.transform, text, new Vector3(0f, 2.35f, -0.07f),
                Quaternion.identity, 0.1f, MobilityLabPalette.OffWhite);
        }

        private static void CreateTrafficLights(Transform root, TrafficSignalController signals)
        {
            CreateTrafficLight(root, signals, RoadAxis.NorthSouth, new Vector3(-7.2f, 0.2f, -7.2f), Quaternion.Euler(0f, 0f, 0f));
            CreateTrafficLight(root, signals, RoadAxis.NorthSouth, new Vector3(7.2f, 0.2f, 7.2f), Quaternion.Euler(0f, 180f, 0f));
            CreateTrafficLight(root, signals, RoadAxis.EastWest, new Vector3(7.2f, 0.2f, -7.2f), Quaternion.Euler(0f, -90f, 0f));
            CreateTrafficLight(root, signals, RoadAxis.EastWest, new Vector3(-7.2f, 0.2f, 7.2f), Quaternion.Euler(0f, 90f, 0f));
        }

        private static void CreateTrafficLight(Transform root, TrafficSignalController signals, RoadAxis axis,
            Vector3 position, Quaternion rotation)
        {
            GameObject assembly = new GameObject($"{axis} Traffic Light", typeof(TrafficLightVisual));
            assembly.transform.SetParent(root, false);
            assembly.transform.localPosition = position;
            assembly.transform.localRotation = rotation;
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Pole", assembly.transform,
                new Vector3(0f, 2.2f, 0f), new Vector3(0.12f, 2.2f, 0.12f), new Color32(40, 47, 50, 255));
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Signal Housing", assembly.transform,
                new Vector3(0f, 4.8f, -0.12f), new Vector3(0.8f, 2.05f, 0.5f), new Color32(28, 34, 37, 255));
            Renderer red = RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Red", assembly.transform,
                new Vector3(0f, 5.42f, -0.42f), new Vector3(0.4f, 0.4f, 0.18f), Color.red).GetComponent<Renderer>();
            Renderer yellow = RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Yellow", assembly.transform,
                new Vector3(0f, 4.8f, -0.42f), new Vector3(0.4f, 0.4f, 0.18f), Color.yellow).GetComponent<Renderer>();
            Renderer green = RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Green", assembly.transform,
                new Vector3(0f, 4.18f, -0.42f), new Vector3(0.4f, 0.4f, 0.18f), Color.green).GetComponent<Renderer>();
            assembly.GetComponent<TrafficLightVisual>().Configure(signals, axis, red, yellow, green);
        }
    }
}
