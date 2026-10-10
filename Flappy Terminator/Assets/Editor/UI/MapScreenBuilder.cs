using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    internal static class GraphicExtensions
    {
        public static T AddGraphic<T>(this GameObject owner) where T : Graphic
        {
            if (owner.GetComponent<CanvasRenderer>() == null)
                owner.AddComponent<CanvasRenderer>();

            return owner.AddComponent<T>();
        }
    }

    public static class MapScreenBuilder
    {
        private const string ArtFolder = "Assets/Sprites/UI/Map/";
        private const string GeometryFolder = "Assets/Editor/UI/MapGeometry/";
        private const string LocationsAsset = "Assets/Data/Generated/Locations.asset";
        private const string SystemsAsset = "Assets/Data/Generated/Systems.asset";
        private const string PanelName = "Map";
        private const string OutlineMaterial = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";
        private const float Aspect = 16f / 9f;
        private const int SystemCount = 5;
        private const int LocationNodeCount = 10;
        private const float SystemNodeSize = 84f;
        private const float LocationNodeSize = 56f;
        private const float BorderWidth = 2.5f;
        private const float SelectionScale = 1.6f;
        private const string WavesAsset = "Assets/Data/Generated/Waves.asset";

        private static readonly Color Cyan = new Color(0.21f, 0.85f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.74f, 0.86f);
        private static readonly Color LockedLabel = new Color(0.55f, 0.6f, 0.68f);
        private static readonly Color BorderColor = new Color(0.21f, 0.85f, 1f, 0.55f);
        private static readonly Color SelectionColor = new Color(1f, 1f, 1f, 0.9f);

        [Serializable]
        private class Point
        {
            public float x;
            public float y;

            public Vector2 ToVector()
            {
                return new Vector2(x, y);
            }
        }

        [Serializable]
        private class Arm
        {
            public string id;
            public string title;
            public bool available;
            public Point[] polygon;
            public Point[] border;
            public Point label;
        }

        [Serializable]
        private class Galaxy
        {
            public Arm[] arms;
        }

        [Serializable]
        private class StarSystem
        {
            public string id;
            public string color;
            public Point position;
            public Point[] locations;
        }

        [Serializable]
        private class Systems
        {
            public StarSystem[] systems;
        }

        [MenuItem("Tools/UI/Build Map Screen")]
        public static void Build()
        {
            PrepareSprites();
            DockScreenBuilder.UseDefaultFont();

            Galaxy galaxy = JsonUtility.FromJson<Galaxy>(Read("Galaxy Map.json"));
            Systems systems = JsonUtility.FromJson<Systems>(Read("Systems Map.json"));

            DockScreen dock = UnityEngine.Object.FindAnyObjectByType<DockScreen>(FindObjectsInactive.Include);
            RectTransform canvas = (RectTransform)dock.transform;
            Transform old = canvas.Find(PanelName);

            if (old != null)
                UnityEngine.Object.DestroyImmediate(old.gameObject);

            RectTransform panel = DockScreenBuilder.Stretch(PanelName, canvas);
            panel.SetAsLastSibling();
            DockScreenBuilder.AddImage(panel, null).color = Color.black;
            panel.GetComponent<Image>().raycastTarget = true;

            RectTransform viewport = DockScreenBuilder.Stretch("Viewport", panel);
            AspectRatioFitter fitter = viewport.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = Aspect;

            RectTransform galaxyView = BuildGalaxy(viewport, galaxy, out MapRegion[] regions);
            RectTransform armView = BuildArm(viewport, systems, out MapNode[] systemNodes, out UiPolyline armDone,
                out UiPolyline armRest);
            RectTransform systemView = BuildSystemView(viewport, out Image systemBackground, out MapNode[] locationNodes,
                out UiPolyline routeDone, out UiPolyline routeRest);

            Button back = DockScreenBuilder.AddButton(panel, "Back Button", "BACK", new Vector2(0f, 1f), new Vector2(110f, -54f),
                new Vector2(170f, 64f), 28);
            TMP_Text title = DockScreenBuilder.AddText(DockScreenBuilder.Place("Title", panel, new Vector2(0f, 1f),
                new Vector2(440f, -54f), new Vector2(460f, 60f)), "MILKY WAY", 40, TextAlignmentOptions.Left, Color.white);
            TMP_Text info = DockScreenBuilder.AddText(DockScreenBuilder.Place("Info", panel, new Vector2(0.5f, 1f),
                new Vector2(0f, -120f), new Vector2(900f, 40f)), "", 26, TextAlignmentOptions.Center, Cyan);
            MapToast toast = BuildToast(panel);

            Button menu = DockScreenBuilder.AddButton(panel, "Menu Button", "MENU", new Vector2(0f, 0f), new Vector2(150f, 62f),
                new Vector2(220f, 76f), 32);
            Button dockButton = DockScreenBuilder.AddButton(panel, "Dock Button", "DOCK", new Vector2(0.5f, 0f), new Vector2(0f, 62f),
                new Vector2(240f, 84f), 36);
            Button play = DockScreenBuilder.AddButton(panel, "Play Button", "PLAY", new Vector2(1f, 0f), new Vector2(-190f, 62f),
                new Vector2(300f, 96f), 40);

            MapScreen screen = canvas.GetComponent<MapScreen>();

            if (screen == null)
                screen = canvas.gameObject.AddComponent<MapScreen>();

            Economy economy = UnityEngine.Object.FindAnyObjectByType<Economy>(FindObjectsInactive.Include);
            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            so.FindProperty("_economy").objectReferenceValue = economy;
            so.FindProperty("_locations").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LocationTable>(LocationsAsset);
            so.FindProperty("_waves").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WaveTable>(WavesAsset);
            so.FindProperty("_systems").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SystemTable>(SystemsAsset);
            so.FindProperty("_galaxyView").objectReferenceValue = galaxyView.gameObject;
            so.FindProperty("_armView").objectReferenceValue = armView.gameObject;
            so.FindProperty("_systemView").objectReferenceValue = systemView.gameObject;
            SetArray(so.FindProperty("_regions"), regions);
            SetArray(so.FindProperty("_systemNodes"), systemNodes);
            so.FindProperty("_armDone").objectReferenceValue = armDone;
            so.FindProperty("_armRest").objectReferenceValue = armRest;
            so.FindProperty("_systemBackground").objectReferenceValue = systemBackground;
            SetArray(so.FindProperty("_locationNodes"), locationNodes);
            so.FindProperty("_routeDone").objectReferenceValue = routeDone;
            so.FindProperty("_routeRest").objectReferenceValue = routeRest;
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_info").objectReferenceValue = info;
            so.FindProperty("_backButton").objectReferenceValue = back;
            so.FindProperty("_toast").objectReferenceValue = toast;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.FindProperty("_dockButton").objectReferenceValue = dockButton;
            so.FindProperty("_playButton").objectReferenceValue = play;
            so.FindProperty("_playLabel").objectReferenceValue = play.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            screen.Configure(GetSystemPositions(systems), GetSystemSprites(), GetStarColors(systems),
                GetLocationPositions(systems));
            EditorUtility.SetDirty(screen);

            WireNavigation(dock, screen);
            panel.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Карта: экран собран.");
        }

        private static RectTransform BuildGalaxy(RectTransform viewport, Galaxy galaxy, out MapRegion[] regions)
        {
            RectTransform view = DockScreenBuilder.Stretch("Galaxy", viewport);
            Image background = DockScreenBuilder.AddImage(view, Art("Galaxy Map"));
            background.raycastTarget = true;

            regions = new MapRegion[galaxy.arms.Length];

            for (int i = 0; i < galaxy.arms.Length; i++)
            {
                Arm arm = galaxy.arms[i];
                RectTransform regionRect = DockScreenBuilder.Stretch(arm.title, view);
                MapRegion region = regionRect.gameObject.AddGraphic<MapRegion>();
                SerializedObject so = new SerializedObject(region);
                so.FindProperty("_id").stringValue = arm.id;
                so.FindProperty("_isAvailable").boolValue = arm.available;
                SetPoints(so.FindProperty("_polygon"), arm.polygon);
                so.ApplyModifiedPropertiesWithoutUndo();
                regions[i] = region;

                UiPolyline border = DockScreenBuilder.Stretch(arm.title + " Border", view).gameObject.AddGraphic<UiPolyline>();
                border.color = BorderColor;
                border.SetStyle(BorderWidth, 0f, 0f, 0f);
                border.SetPoints(ToVectors(arm.border));

                RectTransform label = Anchor(arm.title + " Label", view, arm.label.ToVector(), new Vector2(380f, 34f));
                TMP_Text text = DockScreenBuilder.AddText(label, arm.title, arm.available ? 26 : 20, TextAlignmentOptions.Center,
                    arm.available ? Cyan : LockedLabel);
                text.fontStyle = FontStyles.Bold;
                text.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterial);
            }

            return view;
        }

        private static RectTransform BuildArm(RectTransform viewport, Systems systems, out MapNode[] nodes,
            out UiPolyline done, out UiPolyline rest)
        {
            RectTransform view = DockScreenBuilder.Stretch("Arm", viewport);
            DockScreenBuilder.AddImage(view, Art("Orion Arm Map")).raycastTarget = true;
            rest = DockScreenBuilder.Stretch("Route Rest", view).gameObject.AddGraphic<UiPolyline>();
            done = DockScreenBuilder.Stretch("Route Done", view).gameObject.AddGraphic<UiPolyline>();

            nodes = new MapNode[SystemCount];

            for (int i = 0; i < SystemCount; i++)
            {
                nodes[i] = AddNode(view, "System " + (i + 1), SystemNodeSize, 26);
                Anchor((RectTransform)nodes[i].transform, systems.systems[i].position.ToVector());
            }

            return view;
        }

        private static RectTransform BuildSystemView(RectTransform viewport, out Image background, out MapNode[] nodes,
            out UiPolyline done, out UiPolyline rest)
        {
            RectTransform view = DockScreenBuilder.Stretch("System", viewport);
            background = DockScreenBuilder.AddImage(view, Art("System 1 Map"));
            background.raycastTarget = true;
            rest = DockScreenBuilder.Stretch("Route Rest", view).gameObject.AddGraphic<UiPolyline>();
            done = DockScreenBuilder.Stretch("Route Done", view).gameObject.AddGraphic<UiPolyline>();

            nodes = new MapNode[LocationNodeCount];

            for (int i = 0; i < LocationNodeCount; i++)
                nodes[i] = AddNode(view, "Location " + (i + 1), LocationNodeSize, 22);

            return view;
        }

        private static MapNode AddNode(RectTransform parent, string name, float size, int fontSize)
        {
            RectTransform root = DockScreenBuilder.Place(name, parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
            RectTransform selection = DockScreenBuilder.Place("Selection", root, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(size * SelectionScale, size * SelectionScale));
            DockScreenBuilder.AddImage(selection, Art("Map Node Ring")).color = SelectionColor;
            selection.gameObject.SetActive(false);
            Image core = DockScreenBuilder.AddImage(DockScreenBuilder.Stretch("Core", root), Art("Map Node Core"));
            core.raycastTarget = true;
            Image ring = DockScreenBuilder.AddImage(DockScreenBuilder.Stretch("Ring", root), Art("Map Node Ring"));
            RectTransform lockRect = DockScreenBuilder.Place("Lock", root, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(size * 0.5f, size * 0.5f));
            DockScreenBuilder.AddImage(lockRect, DockScreenBuilder.Art("Lock Icon"));
            TMP_Text label = DockScreenBuilder.AddText(DockScreenBuilder.Place("Label", root, new Vector2(0.5f, 0f),
                new Vector2(0f, -fontSize * 0.8f), new Vector2(220f, fontSize + 8f)), "", fontSize, TextAlignmentOptions.Center,
                Color.white);
            label.fontStyle = FontStyles.Bold;

            MapNode node = root.gameObject.AddComponent<MapNode>();
            SerializedObject so = new SerializedObject(node);
            so.FindProperty("_ring").objectReferenceValue = ring;
            so.FindProperty("_selection").objectReferenceValue = selection.gameObject;
            so.FindProperty("_core").objectReferenceValue = core;
            so.FindProperty("_lock").objectReferenceValue = lockRect.gameObject;
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            return node;
        }

        private static MapToast BuildToast(RectTransform panel)
        {
            RectTransform rect = DockScreenBuilder.Place("Toast", panel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 84f));
            Image image = DockScreenBuilder.AddImage(rect, DockScreenBuilder.Art("Dock Panel"));
            image.type = Image.Type.Sliced;
            TMP_Text label = DockScreenBuilder.AddText(DockScreenBuilder.Stretch("Label", rect), "", 30, TextAlignmentOptions.Center,
                Color.white);
            rect.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;

            MapToast toast = rect.gameObject.AddComponent<MapToast>();
            SerializedObject so = new SerializedObject(toast);
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            return toast;
        }

        private static void WireNavigation(DockScreen dock, MapScreen screen)
        {
            SerializedObject dockSo = new SerializedObject(dock);
            Transform dockPanel = ((GameObject)dockSo.FindProperty("_panel").objectReferenceValue).transform;
            dockSo.FindProperty("_mapButton").objectReferenceValue = dockPanel.Find("Map Button").GetComponent<Button>();
            dockSo.ApplyModifiedPropertiesWithoutUndo();

            ScreenNavigator navigator = UnityEngine.Object.FindAnyObjectByType<ScreenNavigator>(FindObjectsInactive.Include);
            SerializedObject navigatorSo = new SerializedObject(navigator);
            navigatorSo.FindProperty("_mapScreen").objectReferenceValue = screen;
            navigatorSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PrepareSprites()
        {
            foreach (string name in new[] { "Galaxy Map", "Orion Arm Map", "System 1 Map", "System 2 Map", "System 3 Map",
                         "System 4 Map", "System 5 Map" })
                Prepare(name, TextureImporterCompression.Compressed);

            Prepare("Map Node Ring", TextureImporterCompression.Uncompressed);
            Prepare("Map Node Core", TextureImporterCompression.Uncompressed);
        }

        private static void Prepare(string name, TextureImporterCompression compression)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(ArtFolder + name + ".png");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = compression;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static Vector2[] GetSystemPositions(Systems systems)
        {
            Vector2[] positions = new Vector2[systems.systems.Length];

            for (int i = 0; i < positions.Length; i++)
                positions[i] = systems.systems[i].position.ToVector();

            return positions;
        }

        private static Sprite[] GetSystemSprites()
        {
            Sprite[] sprites = new Sprite[SystemCount];

            for (int i = 0; i < SystemCount; i++)
                sprites[i] = Art("System " + (i + 1) + " Map");

            return sprites;
        }

        private static Color[] GetStarColors(Systems systems)
        {
            Color[] colors = new Color[systems.systems.Length];

            for (int i = 0; i < colors.Length; i++)
                ColorUtility.TryParseHtmlString(systems.systems[i].color, out colors[i]);

            return colors;
        }

        private static LocationPositions[] GetLocationPositions(Systems systems)
        {
            LocationPositions[] positions = new LocationPositions[systems.systems.Length];

            for (int i = 0; i < positions.Length; i++)
                positions[i] = new LocationPositions(ToVectors(systems.systems[i].locations).ToArray());

            return positions;
        }

        private static RectTransform Anchor(string name, RectTransform parent, Vector2 normalized, Vector2 size)
        {
            RectTransform rect = DockScreenBuilder.Place(name, parent, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Anchor(rect, normalized);

            return rect;
        }

        private static void Anchor(RectTransform rect, Vector2 normalized)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(normalized.x, 1f - normalized.y);
            rect.anchoredPosition = Vector2.zero;
        }

        private static List<Vector2> ToVectors(Point[] points)
        {
            List<Vector2> vectors = new List<Vector2>();

            foreach (Point point in points)
                vectors.Add(point.ToVector());

            return vectors;
        }

        private static void SetPoints(SerializedProperty property, Point[] points)
        {
            property.arraySize = points.Length;

            for (int i = 0; i < points.Length; i++)
                property.GetArrayElementAtIndex(i).vector2Value = points[i].ToVector();
        }

        private static void SetArray(SerializedProperty property, UnityEngine.Object[] values)
        {
            property.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static string Read(string name)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(GeometryFolder + name).text;
        }

        private static Sprite Art(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + name + ".png");
        }
    }
}
