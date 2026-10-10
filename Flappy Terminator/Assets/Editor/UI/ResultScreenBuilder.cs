using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    public static class ResultScreenBuilder
    {
        private const string PanelName = "Location Result";
        private const int StarCount = 3;
        private const float RowHeight = 44f;

        private const string StarHint = "STAR FOR CLEARING THE LOCATION\nSTAR FOR DESTROYING ALL ENEMIES\n"
            + "STAR FOR NO DAMAGE FROM ASTEROIDS OR RAMMING";

        private static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.8f);
        private static readonly Color Muted = new Color(0.62f, 0.74f, 0.86f);
        private static readonly Color Gold = new Color(1f, 0.82f, 0.3f);
        private static readonly Color Cyan = new Color(0.21f, 0.85f, 1f);

        [MenuItem("Tools/UI/Build Result Screen")]
        public static void Build()
        {
            DockScreenBuilder.UseDefaultFont();

            Game game = Object.FindAnyObjectByType<Game>(FindObjectsInactive.Include);
            BattleStats stats = game.GetComponent<BattleStats>();

            if (stats == null)
                stats = game.gameObject.AddComponent<BattleStats>();

            Set(stats, "_enemySpawner", Object.FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include));
            Set(stats, "_playerShip", Object.FindAnyObjectByType<PlayerShip>(FindObjectsInactive.Include));
            Set(stats, "_shooter", Object.FindAnyObjectByType<PlayerShooter>(FindObjectsInactive.Include));
            Set(stats, "_economy", Object.FindAnyObjectByType<Economy>(FindObjectsInactive.Include));
            Set(game, "_stats", stats);

            DockScreen dock = Object.FindAnyObjectByType<DockScreen>(FindObjectsInactive.Include);
            RectTransform canvas = (RectTransform)dock.transform;
            Transform old = canvas.Find(PanelName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            RectTransform panel = DockScreenBuilder.Stretch(PanelName, canvas);
            Image shade = DockScreenBuilder.AddImage(panel, null);
            shade.color = Backdrop;
            shade.raycastTarget = true;

            RectTransform window = DockScreenBuilder.Place("Window", panel, new Vector2(0.5f, 0.5f), new Vector2(0f, -10f),
                new Vector2(760f, 620f));
            Image frame = DockScreenBuilder.AddImage(window, DockScreenBuilder.Art("Dock Panel"));
            frame.type = Image.Type.Sliced;

            Image[] stars = new Image[StarCount];
            RectTransform starGroup = DockScreenBuilder.Place("Stars", window, new Vector2(0.5f, 1f), new Vector2(0f, -70f),
                new Vector2(320f, 110f));
            Image hitArea = DockScreenBuilder.AddImage(starGroup, null);
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            AddHint(starGroup, StarHint);

            for (int i = 0; i < StarCount; i++)
            {
                float size = i == 1 ? 96f : 76f;
                RectTransform star = DockScreenBuilder.Place("Star " + (i + 1), starGroup, new Vector2(0.5f, 0.5f),
                    new Vector2((i - 1) * 104f, i == 1 ? 6f : -6f), new Vector2(size, size));
                stars[i] = DockScreenBuilder.AddImage(star, DockScreenBuilder.Art("Rating Star"));
                stars[i].preserveAspect = true;
            }

            DockScreenBuilder.AddText(DockScreenBuilder.Place("Title", window, new Vector2(0.5f, 1f), new Vector2(0f, -150f),
                new Vector2(600f, 50f)), "LOCATION COMPLETE", 40, TextAlignmentOptions.Center, Color.white);

            float y = -212f;
            TMP_Text enemies = AddRow(window, "ENEMIES KILLED", ref y, Color.white, out _);
            TMP_Text accuracy = AddRow(window, "ACCURACY", ref y, Color.white, out _);
            TMP_Text damage = AddRow(window, "DAMAGE TAKEN", ref y, Color.white, out _);
            TMP_Text earned = AddRow(window, "EARNED", ref y, Gold, out _);
            TMP_Text noDamage = AddRow(window, "NO DAMAGE BONUS", ref y, Cyan, out GameObject noDamageRow);
            TMP_Text noMiss = AddRow(window, "PERFECT AIM BONUS", ref y, Cyan, out GameObject noMissRow);

            Button toDock = DockScreenBuilder.AddButton(window, "Dock Button", "TO DOCK", new Vector2(0.5f, 0f),
                new Vector2(0f, 64f), new Vector2(300f, 84f), 36);

            LocationResultScreen screen = canvas.GetComponent<LocationResultScreen>();

            if (screen == null)
                screen = canvas.gameObject.AddComponent<LocationResultScreen>();

            SerializedObject so = new SerializedObject(screen);
            so.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            SetArray(so.FindProperty("_stars"), stars);
            so.FindProperty("_enemies").objectReferenceValue = enemies;
            so.FindProperty("_accuracy").objectReferenceValue = accuracy;
            so.FindProperty("_damage").objectReferenceValue = damage;
            so.FindProperty("_earned").objectReferenceValue = earned;
            so.FindProperty("_noDamageRow").objectReferenceValue = noDamageRow;
            so.FindProperty("_noDamage").objectReferenceValue = noDamage;
            so.FindProperty("_noMissRow").objectReferenceValue = noMissRow;
            so.FindProperty("_noMiss").objectReferenceValue = noMiss;
            so.FindProperty("_dockButton").objectReferenceValue = toDock;
            so.ApplyModifiedPropertiesWithoutUndo();

            Set(Object.FindAnyObjectByType<ScreenNavigator>(FindObjectsInactive.Include), "_resultScreen", screen);
            DockScreenBuilder.BuildHud(Object.FindAnyObjectByType<LocationRunner>(FindObjectsInactive.Include));

            panel.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Итоги локации: окно собрано.");
        }

        private static TMP_Text AddRow(RectTransform window, string title, ref float y, Color valueColor, out GameObject row)
        {
            RectTransform rect = DockScreenBuilder.Place(title, window, new Vector2(0.5f, 1f), new Vector2(0f, y),
                new Vector2(640f, RowHeight));
            y -= RowHeight;
            row = rect.gameObject;

            DockScreenBuilder.AddText(DockScreenBuilder.Place("Title", rect, new Vector2(0f, 0.5f), new Vector2(160f, 0f),
                new Vector2(320f, RowHeight)), title, 26, TextAlignmentOptions.Left, Muted);

            return DockScreenBuilder.AddText(DockScreenBuilder.Place("Value", rect, new Vector2(1f, 0.5f), new Vector2(-170f, 0f),
                new Vector2(340f, RowHeight)), "", 28, TextAlignmentOptions.Right, valueColor);
        }

        private static void AddHint(RectTransform target, string text)
        {
            HoverHint hint = target.gameObject.AddComponent<HoverHint>();
            SerializedObject so = new SerializedObject(hint);
            so.FindProperty("_tooltip").objectReferenceValue = Object.FindAnyObjectByType<NumberTooltip>(FindObjectsInactive.Include);
            so.FindProperty("_text").stringValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
