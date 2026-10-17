using ShipContent;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    public static partial class DockScreenBuilder
    {
        private const string ShopName = "Shop";
        private const string ModuleBarName = "Module Bar";
        private const string AnnouncementName = "Boss Announcement";
        private const string OutlineMaterial = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";
        private const string PlayerPrefab = "Assets/Prefabs/Player Ship.prefab";
        private const int ShopCardCount = 10;
        private const int ShopColumns = 5;

        private static readonly Color TabIdle = new Color(0.45f, 0.55f, 0.66f, 0.9f);
        private static readonly Color Shade = new Color(0f, 0.02f, 0.05f, 0.72f);
        private static readonly Color CooldownShade = new Color(0.01f, 0.03f, 0.06f, 0.82f);
        private static readonly Color WindowFill = new Color(0.04f, 0.086f, 0.15f, 1f);

        private static TabView AddTab(RectTransform parent, int index, string text, Vector2 position, Vector2 size)
        {
            RectTransform rect = Place(text + " Tab", parent, new Vector2(0.5f, 0.5f), position, size);
            Image image = AddImage(rect, Art("Slot Frame"));
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text label = AddText(Stretch("Label", rect), text, 22, TextAlignmentOptions.Center, Color.white);

            TabView tab = rect.gameObject.AddComponent<TabView>();
            SerializedObject so = new SerializedObject(tab);
            so.FindProperty("_index").intValue = index;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_background").objectReferenceValue = image;
            so.FindProperty("_label").objectReferenceValue = label;
            so.FindProperty("_selectedColor").colorValue = Cyan;
            so.FindProperty("_idleColor").colorValue = TabIdle;
            so.FindProperty("_selectedText").colorValue = Color.white;
            so.FindProperty("_idleText").colorValue = Muted;
            so.ApplyModifiedPropertiesWithoutUndo();

            return tab;
        }

        private static ShopWindow BuildShop(RectTransform root, Hangar hangar, ModuleRack modules, Economy economy)
        {
            RectTransform shade = Stretch(ShopName, root);
            Image blocker = AddImage(shade, null);
            blocker.color = Shade;
            blocker.raycastTarget = true;

            RectTransform backing = Place("Backing", shade, new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(984f, 524f));
            AddImage(backing, null).color = WindowFill;

            RectTransform window = Place("Window", shade, new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(1000f, 540f));
            AddPanel(window);
            Image windowImage = window.GetComponent<Image>();
            windowImage.raycastTarget = true;

            AddText(Place("Title", window, new Vector2(0f, 1f), new Vector2(110f, -42f), new Vector2(180f, 50f)),
                "SHOP", 38, TextAlignmentOptions.Left, Color.white);
            TabView[] tabs =
            {
                AddTab(window, ShopWindow.WeaponsTab, "WEAPONS", new Vector2(-110f, 228f), new Vector2(200f, 48f)),
                AddTab(window, ShopWindow.ModulesTab, "MODULES", new Vector2(100f, 228f), new Vector2(200f, 48f)),
            };
            Button close = AddButton(window, "Close Button", "X", new Vector2(1f, 1f), new Vector2(-56f, -42f), new Vector2(72f, 56f), 30);

            ShopCardView[] cards = new ShopCardView[ShopCardCount];

            for (int i = 0; i < ShopCardCount; i++)
            {
                int column = i % ShopColumns;
                int row = i / ShopColumns;
                cards[i] = AddShopCard(window, i, new Vector2(-380f + column * 190f, 87f - row * 218f));
            }

            ShopWindow shop = shade.gameObject.AddComponent<ShopWindow>();
            SerializedObject so = new SerializedObject(shop);
            so.FindProperty("_panel").objectReferenceValue = shade.gameObject;
            so.FindProperty("_hangar").objectReferenceValue = hangar;
            so.FindProperty("_modules").objectReferenceValue = modules;
            so.FindProperty("_economy").objectReferenceValue = economy;
            so.FindProperty("_locations").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LocationTable>(LocationsAsset);
            so.FindProperty("_systems").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SystemTable>(SystemsAsset);
            so.FindProperty("_closeButton").objectReferenceValue = close;
            SetArray(so.FindProperty("_tabs"), tabs);
            SetArray(so.FindProperty("_cards"), cards);
            so.ApplyModifiedPropertiesWithoutUndo();

            shade.gameObject.SetActive(false);

            return shop;
        }

        private static ShopCardView AddShopCard(RectTransform parent, int index, Vector2 position)
        {
            RectTransform card = Place("Card " + (index + 1), parent, new Vector2(0.5f, 0.5f), position, new Vector2(178f, 206f));
            AddImage(card, Art("Slot Frame")).type = Image.Type.Sliced;
            GameObject highlight = AddSelection("Highlight", card);

            Image icon = AddImage(Place("Icon", card, new Vector2(0.5f, 0.5f), new Vector2(0f, 54f), new Vector2(150f, 58f)), null);
            icon.preserveAspect = true;
            TMP_Text title = AddText(Place("Title", card, new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(166f, 26f)),
                "", 19, TextAlignmentOptions.Center, Color.white);
            TMP_Text details = AddText(Place("Details", card, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(166f, 22f)),
                "", 14, TextAlignmentOptions.Center, Muted);

            Button button = AddButton(card, "Action Button", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(154f, 50f), 20);
            TMP_Text action = button.transform.Find("Label").GetComponent<TMP_Text>();

            RectTransform priceGroup = Place("Price", (RectTransform)button.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 40f));
            AddImage(Place("Coin", priceGroup, new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(26f, 26f)), Art("Coin Icon"));
            CompactNumber price = AddNumber(AddText(Place("Value", priceGroup, new Vector2(0f, 0.5f), new Vector2(84f, 0f), new Vector2(100f, 36f)),
                "0", 22, TextAlignmentOptions.Left, Gold));

            ShopCardView view = card.gameObject.AddComponent<ShopCardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_details").objectReferenceValue = details;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_action").objectReferenceValue = action;
            so.FindProperty("_priceGroup").objectReferenceValue = priceGroup.gameObject;
            so.FindProperty("_price").objectReferenceValue = price;
            so.FindProperty("_highlight").objectReferenceValue = highlight;
            so.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static void BuildModuleHud(RectTransform canvas, BattleModules battle)
        {
            Transform old = canvas.Find(ModuleBarName);
            int sibling = old == null ? canvas.childCount : old.GetSiblingIndex();

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            RectTransform bar = Place(ModuleBarName, canvas, new Vector2(0.5f, 0f), new Vector2(0f, 176f), new Vector2(240f, 72f));
            bar.SetSiblingIndex(Mathf.Min(sibling, canvas.childCount - 1));

            ModuleButtonView[] buttons = new ModuleButtonView[ModuleSlotCount];

            for (int i = 0; i < ModuleSlotCount; i++)
                buttons[i] = AddModuleButton(bar, i, new Vector2(-78f + i * 78f, 0f));

            ModuleHud hud = bar.gameObject.AddComponent<ModuleHud>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("_modules").objectReferenceValue = battle;
            SetArray(so.FindProperty("_buttons"), buttons);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildBossAnnouncement(RectTransform canvas, EnemySpawner spawner)
        {
            Transform old = canvas.Find(AnnouncementName);
            int sibling = old == null ? canvas.childCount : old.GetSiblingIndex();

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            RectTransform rect = Place(AnnouncementName, canvas, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f),
                new Vector2(1200f, 200f));
            rect.SetSiblingIndex(Mathf.Min(sibling, canvas.childCount - 1));
            CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            TMP_Text label = AddText(Stretch("Label", rect), "", 48, TextAlignmentOptions.Center, Color.white);
            label.fontStyle = FontStyles.Bold;
            label.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterial);
            label.raycastTarget = false;

            BossAnnouncement announcement = rect.gameObject.AddComponent<BossAnnouncement>();
            SerializedObject so = new SerializedObject(announcement);
            so.FindProperty("_spawner").objectReferenceValue = spawner;
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ModuleButtonView AddModuleButton(RectTransform parent, int slot, Vector2 position)
        {
            RectTransform rect = Place("Module " + (slot + 1), parent, new Vector2(0.5f, 0.5f), position, new Vector2(66f, 66f));
            Image frame = AddImage(rect, Art("Slot Frame"));
            frame.type = Image.Type.Sliced;
            frame.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;

            Image icon = AddImage(Place("Icon", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f)), null);
            icon.preserveAspect = true;

            Image cooldown = AddImage(Place("Cooldown", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(58f, 58f)), Art("White Square"));
            cooldown.color = CooldownShade;
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Radial360;
            cooldown.fillOrigin = (int)Image.Origin360.Top;
            cooldown.fillClockwise = false;
            cooldown.fillAmount = 0f;

            GameObject ring = AddSelection("Active", rect);

            TMP_Text key = AddText(Place("Key", rect, new Vector2(1f, 0f), new Vector2(-10f, 10f), new Vector2(20f, 20f)),
                (slot + 1).ToString(), 16, TextAlignmentOptions.Center, Color.white);
            key.fontStyle = FontStyles.Bold;

            ModuleButtonView view = rect.gameObject.AddComponent<ModuleButtonView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_slot").intValue = slot;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.FindProperty("_cooldown").objectReferenceValue = cooldown;
            so.FindProperty("_activeRing").objectReferenceValue = ring;
            so.FindProperty("_key").objectReferenceValue = key;
            so.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static EnergyShield BuildShield(PlayerShip sceneShip)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(PlayerPrefab);

            try
            {
                PlayerShip ship = prefab.GetComponent<PlayerShip>();
                SpriteRenderer body = (SpriteRenderer)new SerializedObject(ship).FindProperty("_shipSprite").objectReferenceValue;
                EnergyShield shield = ShieldRig.Attach(prefab, body, body.GetComponent<AmmoHitHandler>(),
                    out AmmoHitHandler shieldHits);

                SerializedObject shipSo = new SerializedObject(ship);
                shipSo.FindProperty("_shield").objectReferenceValue = shield;
                shipSo.FindProperty("_shieldHitHandler").objectReferenceValue = shieldHits;
                shipSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefab, PlayerPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            return sceneShip.transform.Find(ShieldRig.ShieldName).GetComponent<EnergyShield>();
        }
    }
}
