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
        private const string GalaxyArt = "Galaxy Map v2";
        private const string FirstGalaxyArt = "Galaxy Map";
        private const string VoidKind = "void";
        private const string OutlineMaterial = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";
        private const float Aspect = 16f / 9f;
        private const int SystemCount = 5;
        private const int LocationNodeCount = 10;
        private const int BranchNodeCount = 3;
        private const float SystemNodeSize = 84f;
        private const float LocationNodeSize = 56f;
        private const int LocationStarCount = 3;
        private const float NodeStarSize = 18f;
        private const float NodeStarStep = 20f;
        private const float BorderWidth = 2.5f;
        private const float SelectionScale = 1.6f;
        private const float GateSize = 110f;
        private const float VoidGateSize = 132f;
        private const float VoidHaloPulse = 1.3f / 3f;
        private const string VoidArt = "Ashen Void Map";
        private const string WavesAsset = "Assets/Data/Generated/Waves.asset";

        private static readonly Color Cyan = new Color(0.21f, 0.85f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.74f, 0.86f);
        private static readonly Color LockedLabel = new Color(0.55f, 0.6f, 0.68f);
        private static readonly Color VoidLabel = new Color(0.7f, 0.62f, 1f);
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
            public string caption;
            public string kind;
            public bool available;
            public Point[] polygon;
            public Point[] border;
            public Point label;
        }

        [Serializable]
        private class Line
        {
            public Point[] points;
        }

        [Serializable]
        private class Galaxy
        {
            public Arm[] arms;
            public Line[] borders;
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
        private class BranchPoint
        {
            public string id;
            public Point system;
            public Point arm;
        }

        [Serializable]
        private class VoidPoint
        {
            public string id;
            public string title;
            public Point system;
            public Point arm;
            public Point[] locations;
        }

        [Serializable]
        private class GatePoint
        {
            public string system;
            public float x;
            public float y;
        }

        [Serializable]
        private class Systems
        {
            public StarSystem[] systems;
            public BranchPoint[] branches;
            public GatePoint[] gates;
            public VoidPoint[] voids;
        }

        [MenuItem("Tools/UI/Build Map Screen")]
        public static void Build()
        {
            Build(GalaxyArt);
        }

        [MenuItem("Tools/UI/Build Map Screen (Galaxy v1)")]
        public static void BuildFirstGalaxy()
        {
            Build(FirstGalaxyArt);
        }

        private static void Build(string galaxyArt)
        {
            PrepareSprites();
            DockScreenBuilder.UseDefaultFont();

            Galaxy galaxy = JsonUtility.FromJson<Galaxy>(Read(galaxyArt + ".json"));
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

            RectTransform galaxyView = BuildGalaxy(viewport, galaxy, galaxyArt, out MapRegion[] regions,
                out MapOverlay overlay);
            RectTransform armView = BuildArm(viewport, systems, out MapNode[] systemNodes, out UiPolyline armDone,
                out UiPolyline armRest, out MapNode[] armBranchNodes);
            RectTransform systemView = BuildSystemView(viewport, out Image systemBackground, out MapNode[] locationNodes,
                out UiPolyline routeDone, out UiPolyline routeRest, out MapNode[] branchNodes, out UiPolyline[] branchLinks,
                out StarGateView starGate, out UiPolyline gateLink, out StarGateView voidGate, out UiPolyline voidLink);

            Button back = DockScreenBuilder.AddButton(panel, "Back Button", "BACK", new Vector2(0f, 1f), new Vector2(110f, -54f),
                new Vector2(170f, 64f), 28);
            TMP_Text title = DockScreenBuilder.AddText(DockScreenBuilder.Place("Title", panel, new Vector2(0.5f, 1f),
                new Vector2(0f, -54f), new Vector2(700f, 60f)), "MILKY WAY", 40, TextAlignmentOptions.Center, Color.white);
            TMP_Text info = DockScreenBuilder.AddText(DockScreenBuilder.Place("Info", panel, new Vector2(0.5f, 1f),
                new Vector2(0f, -104f), new Vector2(900f, 40f)), "", 26, TextAlignmentOptions.Center, Cyan);
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
            so.FindProperty("_galaxyOverlay").objectReferenceValue = overlay;
            SetArray(so.FindProperty("_systemNodes"), systemNodes);
            so.FindProperty("_armDone").objectReferenceValue = armDone;
            so.FindProperty("_armRest").objectReferenceValue = armRest;
            so.FindProperty("_systemBackground").objectReferenceValue = systemBackground;
            SetArray(so.FindProperty("_locationNodes"), locationNodes);
            so.FindProperty("_routeDone").objectReferenceValue = routeDone;
            so.FindProperty("_routeRest").objectReferenceValue = routeRest;
            SetArray(so.FindProperty("_branchNodes"), branchNodes);
            SetArray(so.FindProperty("_branchLinks"), branchLinks);
            SetArray(so.FindProperty("_armBranchNodes"), armBranchNodes);
            so.FindProperty("_starGate").objectReferenceValue = starGate;
            so.FindProperty("_gateLink").objectReferenceValue = gateLink;
            so.FindProperty("_voidGate").objectReferenceValue = voidGate;
            so.FindProperty("_voidLink").objectReferenceValue = voidLink;
            so.FindProperty("_backLabel").objectReferenceValue = back.GetComponentInChildren<TMP_Text>();
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
                GetLocationPositions(systems), GetBranchPlacements(systems), GetGatePositions(systems),
                GetVoidPlacements(systems), Art(VoidArt));
            EditorUtility.SetDirty(screen);

            WireNavigation(dock, screen);
            panel.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Карта: экран собран.");
        }

        private static RectTransform BuildGalaxy(RectTransform viewport, Galaxy galaxy, string galaxyArt,
            out MapRegion[] regions, out MapOverlay overlay)
        {
            RectTransform view = DockScreenBuilder.Stretch("Galaxy", viewport);
            Image background = DockScreenBuilder.AddImage(view, Art(galaxyArt));
            background.raycastTarget = true;

            regions = new MapRegion[galaxy.arms.Length];
            RectTransform layer = DockScreenBuilder.Stretch("Overlay", view);
            overlay = BuildOverlay(layer);

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

                if (arm.border != null && arm.border.Length > 1)
                    AddBorder(layer, arm.title + " Border", arm.border);

                RectTransform label = Anchor(arm.title + " Label", layer, arm.label.ToVector(), new Vector2(380f, 64f));
                string caption = string.IsNullOrEmpty(arm.caption) ? arm.title : arm.caption;
                TMP_Text text = DockScreenBuilder.AddText(label, caption, arm.available ? 26 : 20, TextAlignmentOptions.Center,
                    GetLabelColor(arm));
                text.fontStyle = FontStyles.Bold;
                text.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterial);

                SerializedObject regionSo = new SerializedObject(region);
                regionSo.FindProperty("_label").objectReferenceValue = text;
                regionSo.FindProperty("_labelColor").colorValue = text.color;
                regionSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (galaxy.borders != null)
            {
                for (int i = 0; i < galaxy.borders.Length; i++)
                    AddBorder(layer, "Border " + (i + 1), galaxy.borders[i].points);
            }

            layer.SetAsLastSibling();

            return view;
        }

        private static Color GetLabelColor(Arm arm)
        {
            if (arm.available)
                return Cyan;

            return arm.kind == VoidKind ? VoidLabel : LockedLabel;
        }

        private static MapOverlay BuildOverlay(RectTransform layer)
        {
            CanvasGroup group = layer.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            MapOverlay overlay = layer.gameObject.AddComponent<MapOverlay>();
            SerializedObject so = new SerializedObject(overlay);
            so.FindProperty("_group").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();

            return overlay;
        }

        private static void AddBorder(RectTransform layer, string name, Point[] points)
        {
            UiPolyline border = DockScreenBuilder.Stretch(name, layer).gameObject.AddGraphic<UiPolyline>();
            border.color = BorderColor;
            border.SetStyle(BorderWidth, 0f, 0f, 0f);
            border.SetPoints(ToVectors(points));
        }

        private static RectTransform BuildArm(RectTransform viewport, Systems systems, out MapNode[] nodes,
            out UiPolyline done, out UiPolyline rest, out MapNode[] branches)
        {
            RectTransform view = DockScreenBuilder.Stretch("Arm", viewport);
            DockScreenBuilder.AddImage(view, Art("Orion Arm Map")).raycastTarget = true;
            rest = DockScreenBuilder.Stretch("Route Rest", view).gameObject.AddGraphic<UiPolyline>();
            done = DockScreenBuilder.Stretch("Route Done", view).gameObject.AddGraphic<UiPolyline>();

            nodes = new MapNode[SystemCount];

            for (int i = 0; i < SystemCount; i++)
            {
                nodes[i] = AddNode(view, "System " + (i + 1), SystemNodeSize, 26, 0);
                Anchor((RectTransform)nodes[i].transform, systems.systems[i].position.ToVector());
            }

            branches = new MapNode[BranchNodeCount];

            for (int i = 0; i < BranchNodeCount; i++)
                branches[i] = AddNode(view, "Arm Branch " + (i + 1), LocationNodeSize, 18, LocationStarCount);

            return view;
        }

        private static RectTransform BuildSystemView(RectTransform viewport, out Image background, out MapNode[] nodes,
            out UiPolyline done, out UiPolyline rest, out MapNode[] branches, out UiPolyline[] links, out StarGateView gate,
            out UiPolyline gateLink, out StarGateView voidGate, out UiPolyline voidLink)
        {
            RectTransform view = DockScreenBuilder.Stretch("System", viewport);
            background = DockScreenBuilder.AddImage(view, Art("System 1 Map"));
            background.raycastTarget = true;
            rest = DockScreenBuilder.Stretch("Route Rest", view).gameObject.AddGraphic<UiPolyline>();
            done = DockScreenBuilder.Stretch("Route Done", view).gameObject.AddGraphic<UiPolyline>();

            links = new UiPolyline[BranchNodeCount];

            for (int i = 0; i < BranchNodeCount; i++)
                links[i] = DockScreenBuilder.Stretch("Branch Link " + (i + 1), view).gameObject.AddGraphic<UiPolyline>();

            gateLink = DockScreenBuilder.Stretch("Gate Link", view).gameObject.AddGraphic<UiPolyline>();
            gate = AddGate(view, "Star Gate", Color.white);
            voidLink = DockScreenBuilder.Stretch("Void Link", view).gameObject.AddGraphic<UiPolyline>();
            voidGate = AddVoidGate(view);

            nodes = new MapNode[LocationNodeCount];

            for (int i = 0; i < LocationNodeCount; i++)
                nodes[i] = AddNode(view, "Location " + (i + 1), LocationNodeSize, 22, LocationStarCount);

            branches = new MapNode[BranchNodeCount];

            for (int i = 0; i < BranchNodeCount; i++)
                branches[i] = AddNode(view, "Branch " + (i + 1), LocationNodeSize, 18, LocationStarCount);

            return view;
        }

        private static StarGateView AddVoidGate(RectTransform parent)
        {
            RectTransform root = DockScreenBuilder.Place("Void Gate", parent, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(VoidGateSize, VoidGateSize));
            RectTransform halo = DockScreenBuilder.Place("Halo", root, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(VoidGateSize * 1.35f, VoidGateSize * 1.35f));
            Image haloImage = DockScreenBuilder.AddImage(halo, Art("Void Gate Halo"));
            RectTransform rift = DockScreenBuilder.Stretch("Rift", root);
            DockScreenBuilder.AddImage(rift, Art("Void Gate Rift"));
            RectTransform frame = DockScreenBuilder.Stretch("Frame", root);
            Image frameImage = DockScreenBuilder.AddImage(frame, Art("Void Gate Frame"));
            frameImage.raycastTarget = true;
            RectTransform lockRect = DockScreenBuilder.Place("Lock", root, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(GateSize * 0.32f, GateSize * 0.32f));
            DockScreenBuilder.AddImage(lockRect, DockScreenBuilder.Art("Lock Icon"));
            TMP_Text label = DockScreenBuilder.AddText(DockScreenBuilder.Place("Label", root, new Vector2(0.5f, 0f),
                new Vector2(0f, -8f), new Vector2(260f, 30f)), "", 22, TextAlignmentOptions.Center, Color.white);
            label.fontStyle = FontStyles.Bold;

            StarGateView gate = root.gameObject.AddComponent<StarGateView>();
            SerializedObject so = new SerializedObject(gate);
            so.FindProperty("_ring").objectReferenceValue = frameImage;
            so.FindProperty("_vortex").objectReferenceValue = rift;
            so.FindProperty("_lock").objectReferenceValue = lockRect.gameObject;
            so.FindProperty("_label").objectReferenceValue = label;
            so.FindProperty("_spinSpeed").floatValue = -28f;
            so.FindProperty("_frame").objectReferenceValue = frame;
            so.FindProperty("_frameSpinSpeed").floatValue = 7f;
            so.FindProperty("_halo").objectReferenceValue = haloImage;
            so.FindProperty("_haloPulseSpeed").floatValue = VoidHaloPulse;
            so.FindProperty("_lockedColor").colorValue = new Color(0.32f, 0.3f, 0.36f, 0.9f);
            so.ApplyModifiedPropertiesWithoutUndo();

            return gate;
        }

        private static StarGateView AddGate(RectTransform parent, string name, Color vortexTint)
        {
            RectTransform root = DockScreenBuilder.Place(name, parent, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(GateSize, GateSize));
            RectTransform vortex = DockScreenBuilder.Stretch("Vortex", root);
            DockScreenBuilder.AddImage(vortex, Art("Star Gate Vortex")).color = vortexTint;
            Image ring = DockScreenBuilder.AddImage(DockScreenBuilder.Stretch("Ring", root), Art("Star Gate"));
            ring.raycastTarget = true;
            RectTransform lockRect = DockScreenBuilder.Place("Lock", root, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(GateSize * 0.32f, GateSize * 0.32f));
            DockScreenBuilder.AddImage(lockRect, DockScreenBuilder.Art("Lock Icon"));
            TMP_Text label = DockScreenBuilder.AddText(DockScreenBuilder.Place("Label", root, new Vector2(0.5f, 0f),
                new Vector2(0f, -16f), new Vector2(220f, 30f)), "", 22, TextAlignmentOptions.Center, Color.white);
            label.fontStyle = FontStyles.Bold;

            StarGateView gate = root.gameObject.AddComponent<StarGateView>();
            SerializedObject so = new SerializedObject(gate);
            so.FindProperty("_ring").objectReferenceValue = ring;
            so.FindProperty("_vortex").objectReferenceValue = vortex;
            so.FindProperty("_lock").objectReferenceValue = lockRect.gameObject;
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            return gate;
        }

        private static MapNode AddNode(RectTransform parent, string name, float size, int fontSize, int starCount)
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
            Image[] stars = AddStars(root, starCount, -fontSize * 1.8f - NodeStarSize * 0.5f);

            MapNode node = root.gameObject.AddComponent<MapNode>();
            SerializedObject so = new SerializedObject(node);
            so.FindProperty("_ring").objectReferenceValue = ring;
            so.FindProperty("_selection").objectReferenceValue = selection.gameObject;
            so.FindProperty("_core").objectReferenceValue = core;
            so.FindProperty("_lock").objectReferenceValue = lockRect.gameObject;
            so.FindProperty("_label").objectReferenceValue = label;
            SerializedProperty starsProperty = so.FindProperty("_stars");
            starsProperty.arraySize = stars.Length;

            for (int i = 0; i < stars.Length; i++)
                starsProperty.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            return node;
        }

        private static Image[] AddStars(RectTransform root, int count, float y)
        {
            Image[] stars = new Image[count];

            for (int i = 0; i < count; i++)
            {
                RectTransform star = DockScreenBuilder.Place("Star " + (i + 1), root, new Vector2(0.5f, 0f),
                    new Vector2((i - (count - 1) * 0.5f) * NodeStarStep, y), new Vector2(NodeStarSize, NodeStarSize));
                stars[i] = DockScreenBuilder.AddImage(star, DockScreenBuilder.Art("Rating Star"));
                stars[i].preserveAspect = true;
                star.gameObject.SetActive(false);
            }

            return stars;
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
            foreach (string name in new[] { FirstGalaxyArt, GalaxyArt, "Orion Arm Map", "System 1 Map", "System 2 Map", "System 3 Map",
                         "System 4 Map", "System 5 Map" })
                Prepare(name, TextureImporterCompression.Compressed);

            Prepare("Map Node Ring", TextureImporterCompression.Uncompressed);
            Prepare("Map Node Core", TextureImporterCompression.Uncompressed);
            Prepare("Star Gate", TextureImporterCompression.Uncompressed);
            Prepare("Star Gate Vortex", TextureImporterCompression.Uncompressed);
            Prepare(VoidArt, TextureImporterCompression.Compressed);
            Prepare("Void Gate Rift", TextureImporterCompression.Uncompressed);
            Prepare("Void Gate Frame", TextureImporterCompression.Uncompressed);
            Prepare("Void Gate Halo", TextureImporterCompression.Uncompressed);
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

        private static Vector2[] GetGatePositions(Systems systems)
        {
            Vector2[] positions = new Vector2[systems.systems.Length];

            if (systems.gates == null)
                return positions;

            foreach (GatePoint gate in systems.gates)
            {
                int index = Array.FindIndex(systems.systems, system => system.id == gate.system);

                if (index < 0)
                    throw new InvalidOperationException("Systems Map.json: врата у неизвестной системы " + gate.system);

                positions[index] = new Vector2(gate.x, gate.y);
            }

            return positions;
        }

        private static VoidPlacement[] GetVoidPlacements(Systems systems)
        {
            if (systems.voids == null)
                return new VoidPlacement[0];

            VoidPlacement[] placements = new VoidPlacement[systems.voids.Length];

            for (int i = 0; i < placements.Length; i++)
            {
                VoidPoint point = systems.voids[i];

                placements[i] = new VoidPlacement(point.id, point.title, ToVectorOrZero(point.system),
                    ToVectorOrZero(point.arm), ToVectors(point.locations).ToArray());
            }

            return placements;
        }

        private static BranchPlacement[] GetBranchPlacements(Systems systems)
        {
            if (systems.branches == null)
                return new BranchPlacement[0];

            BranchPlacement[] placements = new BranchPlacement[systems.branches.Length];

            for (int i = 0; i < placements.Length; i++)
            {
                BranchPoint branch = systems.branches[i];
                placements[i] = new BranchPlacement(branch.id, ToVectorOrZero(branch.system), ToVectorOrZero(branch.arm));
            }

            return placements;
        }

        private static Vector2 ToVectorOrZero(Point point)
        {
            return point == null ? Vector2.zero : point.ToVector();
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
