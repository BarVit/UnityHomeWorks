using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    public static class DockScreenBuilder
    {
        private const string ArtFolder = "Assets/Sprites/UI/Dock/";
                private const string TitleSprite = "Assets/Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/buttons/button_active.png";
        private const string PlayerSheet = "Assets/Sprites/Ships/Player.png";
        private const string PlayerFrame = "tinyShip5_0";
        private const string SlotsAsset = "Assets/Data/Generated/Slots.asset";
        private const string UpgradesAsset = "Assets/Data/Generated/WeaponUpgrades.asset";
        private const string WeaponsAsset = "Assets/Data/Generated/Weapons.asset";

        private const string TooltipName = "Number Tooltip";
        private const int PanelBorder = 30;
        private const int SlotBorder = 22;
        private const int SlotCount = 4;
        private const int PipCount = 5;
        private const float ShipScale = 6f;

        private static readonly Vector2 HudPosition = new Vector2(-440f, 289f);

        private static readonly Color Cyan = new Color(0.21f, 0.85f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.74f, 0.86f);
        private static readonly Color PipOff = new Color(0.13f, 0.2f, 0.29f);
        private static readonly Color Gold = new Color(1f, 0.82f, 0.3f);

        private static TMP_FontAsset _font;
        private static NumberTooltip _tooltip;

        internal static void UseDefaultFont()
        {
            _font = TMP_Settings.defaultFontAsset;
        }

        [MenuItem("Tools/UI/Build Dock Screen")]
        public static void Build()
        {
            PrepareSprites();

            _font = TMP_Settings.defaultFontAsset;

            DockScreen dock = Object.FindAnyObjectByType<DockScreen>(FindObjectsInactive.Include);
            Economy economy = Object.FindAnyObjectByType<Economy>(FindObjectsInactive.Include);
            Game game = Object.FindAnyObjectByType<Game>(FindObjectsInactive.Include);
            LocationRunner runner = Object.FindAnyObjectByType<LocationRunner>(FindObjectsInactive.Include);

            Hangar hangar = economy.GetComponent<Hangar>();

            if (hangar == null)
                hangar = economy.gameObject.AddComponent<Hangar>();

            Set(hangar, "_economy", economy);
            Set(hangar, "_slots", AssetDatabase.LoadAssetAtPath<SlotTable>(SlotsAsset));
            Set(hangar, "_upgrades", AssetDatabase.LoadAssetAtPath<UpgradeTable>(UpgradesAsset));
            Set(hangar, "_weapons", AssetDatabase.LoadAssetAtPath<WeaponTable>(WeaponsAsset));
            Set(game, "_hangar", hangar);

            _tooltip = BuildTooltip((RectTransform)dock.transform);
            BuildDock(dock, hangar, economy);
            BuildHud(runner);

            EditorSceneManager.MarkSceneDirty(dock.gameObject.scene);
            Debug.Log("Док: экран собран.");
        }

        private static void PrepareSprites()
        {
            Prepare("Dock Background", 0);
            Prepare("Dock Panel", PanelBorder);
            Prepare("Slot Frame", SlotBorder);
            Prepare("Slot Selected", SlotBorder);

            foreach (string name in new[] { "Coin Icon", "XP Icon", "Lock Icon", "Damage Icon", "Crit Icon", "Upgrade Pip" })
                Prepare(name, 0);
        }

        private static void Prepare(string name, int border)
        {
            string path = ArtFolder + name + ".png";
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
        }

        private static void BuildDock(DockScreen dock, Hangar hangar, Economy economy)
        {
            SerializedObject so = new SerializedObject(dock);
            GameObject panel = (GameObject)so.FindProperty("_panel").objectReferenceValue;

            for (int i = panel.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(panel.transform.GetChild(i).gameObject);

            panel.name = "Dock";
            panel.transform.SetAsLastSibling();

            RectTransform root = (RectTransform)panel.transform;

            Image background = AddImage(Stretch("Background", root), Art("Dock Background"));
            background.raycastTarget = true;

            RectTransform ship = Place("Docked Ship", root, new Vector2(0.26f, 0.546f), Vector2.zero, new Vector2(34f, 38f) * ShipScale);
            AddImage(ship, PlayerShip()).preserveAspect = true;
            ship.localRotation = Quaternion.Euler(0f, 0f, -90f);

            RectTransform plate = Place("Title Plate", root, new Vector2(0f, 1f), new Vector2(190f, -60f), new Vector2(300f, 76f));
            AddImage(plate, Load<Sprite>(TitleSprite)).type = Image.Type.Sliced;
            AddText(Stretch("Title", plate), "DOCK", 44, TextAlignmentOptions.Center, Color.white);

            RectTransform earned = Place("Earned", root, new Vector2(0f, 1f), new Vector2(360f, -122f), new Vector2(640f, 40f));
            CompactNumber earnedXp = AddNumber(AddText(Place("XP", earned, new Vector2(0f, 0.5f), new Vector2(150f, 0f), new Vector2(300f, 40f)),
                "", 26, TextAlignmentOptions.Left, Gold), "+", " XP");
            CompactNumber earnedMoney = AddNumber(AddText(Place("Money", earned, new Vector2(0f, 0.5f), new Vector2(390f, 0f), new Vector2(300f, 40f)),
                "", 26, TextAlignmentOptions.Left, Gold), "+", " COINS");
            TMP_Text status = AddText(Place("Status", root, new Vector2(0f, 1f), new Vector2(360f, -154f), new Vector2(640f, 30f)),
                "LOCATION CLEARED", 22, TextAlignmentOptions.Left, Muted);

            RectTransform wallet = Place("Wallet", root, new Vector2(1f, 1f), new Vector2(-250f, -58f), new Vector2(420f, 76f));
            AddPanel(wallet);
            CompactNumber xp = AddCounter(wallet, "XP", "XP Icon", -100f);
            CompactNumber money = AddCounter(wallet, "Money", "Coin Icon", 100f);

            RectTransform loadout = Place("Loadout", root, new Vector2(1f, 0.5f), new Vector2(-350f, 6f), new Vector2(640f, 470f));
            AddPanel(loadout);
            AddText(Place("Header", loadout, new Vector2(0f, 1f), new Vector2(170f, -34f), new Vector2(300f, 36f)),
                "WEAPON SLOTS", 26, TextAlignmentOptions.Left, Cyan);

            SlotView[] slots = new SlotView[SlotCount];

            for (int i = 0; i < SlotCount; i++)
                slots[i] = AddSlot(loadout, i + 1, new Vector2(-226.5f + i * 151f, 132f));

            RectTransform divider = Place("Divider", loadout, new Vector2(0.5f, 0.5f), new Vector2(0f, 64f), new Vector2(580f, 2f));
            AddImage(divider, null).color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);

            RectTransform upgrades = Place("Upgrades", loadout, new Vector2(0.5f, 0.5f), new Vector2(0f, -86f), new Vector2(640f, 290f));
            Image weaponIcon = AddImage(Place("Weapon Icon", upgrades, new Vector2(0f, 1f), new Vector2(120f, -40f), new Vector2(180f, 71f)), null);
            weaponIcon.preserveAspect = true;
            TMP_Text weaponTitle = AddText(Place("Weapon Title", upgrades, new Vector2(0f, 1f), new Vector2(390f, -40f), new Vector2(330f, 50f)),
                "", 34, TextAlignmentOptions.Left, Color.white);

            UpgradeRowView damage = AddUpgradeRow(upgrades, UpgradeStat.Damage, "DAMAGE", "+2 PER LEVEL", "Damage Icon", -58f);
            UpgradeRowView crit = AddUpgradeRow(upgrades, UpgradeStat.CritChance, "CRIT CHANCE", "CRIT HIT DEALS x2", "Crit Icon", -148f);

            Button menu = AddButton(root, "Menu Button", "MENU", new Vector2(0f, 0f), new Vector2(150f, 62f), new Vector2(220f, 76f), 32);
            AddMapButton(root);
            Button play = AddButton(root, "Play Button", "PLAY", new Vector2(1f, 0f), new Vector2(-190f, 62f), new Vector2(300f, 96f), 40);

            so.FindProperty("_hangar").objectReferenceValue = hangar;
            so.FindProperty("_economy").objectReferenceValue = economy;
            so.FindProperty("_weapons").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WeaponTable>(WeaponsAsset);
            so.FindProperty("_xp").objectReferenceValue = xp;
            so.FindProperty("_money").objectReferenceValue = money;
            so.FindProperty("_earned").objectReferenceValue = earned.gameObject;
            so.FindProperty("_earnedXp").objectReferenceValue = earnedXp;
            so.FindProperty("_earnedMoney").objectReferenceValue = earnedMoney;
            so.FindProperty("_status").objectReferenceValue = status;
            SetArray(so.FindProperty("_slots"), slots);
            so.FindProperty("_weaponIcon").objectReferenceValue = weaponIcon;
            so.FindProperty("_weaponTitle").objectReferenceValue = weaponTitle;
            so.FindProperty("_upgradePanel").objectReferenceValue = upgrades.gameObject;
            SetArray(so.FindProperty("_upgrades"), new Object[] { damage, crit });
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.FindProperty("_playButton").objectReferenceValue = play;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildHud(LocationRunner runner)
        {
            Transform canvas = Object.FindAnyObjectByType<DockScreen>(FindObjectsInactive.Include).transform;
            RewardViewer viewer = Object.FindAnyObjectByType<RewardViewer>(FindObjectsInactive.Include);
            Vector2 position = HudPosition;
            int sibling = 0;

            if (viewer != null)
            {
                position = ((RectTransform)viewer.transform).anchoredPosition;
                sibling = viewer.transform.GetSiblingIndex();
                Object.DestroyImmediate(viewer.gameObject);
            }

            RectTransform hud = Place("Run Reward", (RectTransform)canvas, new Vector2(0.5f, 0.5f), position,
                new Vector2(320f, 40f));
            hud.SetSiblingIndex(sibling);

            CompactNumber xp = AddHudCounter(hud, "XP", "XP Icon", 0f);
            CompactNumber money = AddHudCounter(hud, "Money", "Coin Icon", 150f);

            viewer = hud.gameObject.AddComponent<RewardViewer>();
            Set(viewer, "_runner", runner);
            Set(viewer, "_xp", xp);
            Set(viewer, "_money", money);
        }

        private static CompactNumber AddHudCounter(RectTransform parent, string name, string icon, float x)
        {
            RectTransform group = Place(name, parent, new Vector2(0f, 0.5f), new Vector2(x + 70f, 0f), new Vector2(140f, 40f));
            AddImage(Place("Icon", group, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(34f, 34f)), Art(icon)).preserveAspect = true;

            return AddNumber(AddText(Place("Value", group, new Vector2(0f, 0.5f), new Vector2(88f, 0f), new Vector2(100f, 40f)),
                "0", 30, TextAlignmentOptions.Left, Color.white));
        }

        private static CompactNumber AddCounter(RectTransform parent, string name, string icon, float x)
        {
            RectTransform group = Place(name, parent, new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(190f, 60f));
            AddImage(Place("Icon", group, new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(44f, 44f)), Art(icon)).preserveAspect = true;

            return AddNumber(AddText(Place("Value", group, new Vector2(0f, 0.5f), new Vector2(118f, 0f), new Vector2(130f, 50f)),
                "0", 34, TextAlignmentOptions.Left, Color.white), hasTooltip: true);
        }

        private static SlotView AddSlot(RectTransform parent, int slot, Vector2 position)
        {
            RectTransform frame = Place("Slot " + slot, parent, new Vector2(0.5f, 0.5f), position, new Vector2(140f, 104f));
            Image image = AddImage(frame, Art("Slot Frame"));
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

            Button button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CanvasGroup group = frame.gameObject.AddComponent<CanvasGroup>();

            GameObject selection = AddImage(Stretch("Selection", frame), Art("Slot Selected")).gameObject;
            selection.GetComponent<Image>().type = Image.Type.Sliced;
            selection.GetComponent<Image>().raycastTarget = false;

            Image icon = AddImage(Place("Icon", frame, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(118f, 46f)), null);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            TMP_Text caption = AddText(Place("Caption", frame, new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(130f, 24f)),
                "", 18, TextAlignmentOptions.Center, Muted);

            GameObject lockGroup = Place("Lock", frame, new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(48f, 48f)).gameObject;
            AddImage((RectTransform)lockGroup.transform, Art("Lock Icon")).raycastTarget = false;

            RectTransform priceGroup = Place("Price", frame, new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(130f, 26f));
            AddImage(Place("Coin", priceGroup, new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(22f, 22f)), Art("Coin Icon")).raycastTarget = false;
            CompactNumber price = AddNumber(AddText(Place("Value", priceGroup, new Vector2(0f, 0.5f), new Vector2(78f, 0f), new Vector2(90f, 26f)),
                "0", 22, TextAlignmentOptions.Left, Gold));

            SlotView view = frame.gameObject.AddComponent<SlotView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_slot").intValue = slot;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.FindProperty("_lock").objectReferenceValue = lockGroup;
            so.FindProperty("_priceGroup").objectReferenceValue = priceGroup.gameObject;
            so.FindProperty("_price").objectReferenceValue = price;
            so.FindProperty("_caption").objectReferenceValue = caption;
            so.FindProperty("_selection").objectReferenceValue = selection;
            so.FindProperty("_group").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static UpgradeRowView AddUpgradeRow(RectTransform parent, UpgradeStat stat, string title, string hint,
            string icon, float y)
        {
            RectTransform row = Place(title, parent, new Vector2(0.5f, 1f), new Vector2(0f, y - 70f), new Vector2(600f, 82f));
            AddImage(Place("Icon", row, new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(60f, 60f)), Art(icon)).preserveAspect = true;
            AddText(Place("Title", row, new Vector2(0f, 0.5f), new Vector2(190f, 20f), new Vector2(240f, 30f)),
                title, 24, TextAlignmentOptions.Left, Color.white);
            AddText(Place("Hint", row, new Vector2(0f, 0.5f), new Vector2(190f, -4f), new Vector2(240f, 22f)),
                hint, 16, TextAlignmentOptions.Left, Muted);
            TMP_Text value = AddText(Place("Value", row, new Vector2(0f, 0.5f), new Vector2(190f, -28f), new Vector2(240f, 26f)),
                "", 22, TextAlignmentOptions.Left, Cyan);

            Image[] pips = new Image[PipCount];

            for (int i = 0; i < PipCount; i++)
            {
                pips[i] = AddImage(Place("Pip " + (i + 1), row, new Vector2(0f, 0.5f), new Vector2(330f + i * 22f, 0f), new Vector2(18f, 28f)),
                    Art("Upgrade Pip"));
                pips[i].color = PipOff;
            }

            Button button = AddButton(row, "Buy Button", "", new Vector2(1f, 0.5f), new Vector2(-82f, 0f), new Vector2(156f, 62f), 26);
            RectTransform label = (RectTransform)button.transform.Find("Label");
            label.anchorMin = new Vector2(0f, 0f);
            label.anchorMax = new Vector2(1f, 1f);
            label.offsetMin = new Vector2(52f, 0f);
            label.offsetMax = new Vector2(-14f, 0f);
            TMP_Text priceLabel = label.GetComponent<TMP_Text>();
            priceLabel.alignment = TextAlignmentOptions.Left;
            priceLabel.color = Gold;
            CompactNumber price = AddNumber(priceLabel);
            AddImage(Place("Coin", (RectTransform)button.transform, new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(28f, 28f)),
                Art("Coin Icon")).raycastTarget = false;

            UpgradeRowView view = row.gameObject.AddComponent<UpgradeRowView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_stat").enumValueIndex = (int)stat;
            so.FindProperty("_value").objectReferenceValue = value;
            SetArray(so.FindProperty("_pips"), pips);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_price").objectReferenceValue = price;
            so.FindProperty("_priceLabel").objectReferenceValue = priceLabel;
            so.FindProperty("_pipOn").colorValue = Cyan;
            so.FindProperty("_pipOff").colorValue = PipOff;
            so.ApplyModifiedPropertiesWithoutUndo();

            return view;
        }

        private static CompactNumber AddNumber(TMP_Text text, string prefix = "", string suffix = "",
            bool hasTooltip = false)
        {
            text.raycastTarget = hasTooltip;

            CompactNumber number = text.gameObject.AddComponent<CompactNumber>();
            SerializedObject so = new SerializedObject(number);
            so.FindProperty("_tooltip").objectReferenceValue = hasTooltip ? _tooltip : null;
            so.FindProperty("_prefix").stringValue = prefix;
            so.FindProperty("_suffix").stringValue = suffix;
            so.ApplyModifiedPropertiesWithoutUndo();

            return number;
        }

        private static NumberTooltip BuildTooltip(RectTransform canvas)
        {
            Transform old = canvas.Find(TooltipName);

            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            RectTransform panel = Place(TooltipName, canvas, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 48f));
            AddImage(panel, Art("Slot Frame")).type = Image.Type.Sliced;
            TMP_Text label = AddText(Stretch("Label", panel), "0", 24, TextAlignmentOptions.Center, Color.white);

            NumberTooltip tooltip = panel.gameObject.AddComponent<NumberTooltip>();
            SerializedObject so = new SerializedObject(tooltip);
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_label").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);

            return tooltip;
        }

        private static void AddMapButton(RectTransform root)
        {
            AddButton(root, "Map Button", "MAP", new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(240f, 84f), 36);
        }

        internal static Button AddButton(RectTransform parent, string name, string text, Vector2 anchor, Vector2 position,
            Vector2 size, int fontSize)
        {
            RectTransform rect = Place(name, parent, anchor, position, size);
            Image image = AddImage(rect, Load<Sprite>(ButtonStyle.ButtonSprite));
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ButtonStyle.Apply(button);
            AddText(Stretch("Label", rect), text, fontSize, TextAlignmentOptions.Center, Color.white).raycastTarget = false;

            return button;
        }

        private static void AddPanel(RectTransform rect)
        {
            Image image = AddImage(rect, Art("Dock Panel"));
            image.type = Image.Type.Sliced;
        }

        internal static RectTransform Stretch(string name, RectTransform parent)
        {
            RectTransform rect = Create(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return rect;
        }

        internal static RectTransform Place(string name, RectTransform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform rect = Create(name, parent);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            return rect;
        }

        private static RectTransform Create(string name, RectTransform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            return (RectTransform)go.transform;
        }

        internal static Image AddImage(RectTransform rect, Sprite sprite)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;

            return image;
        }

        internal static TMP_Text AddText(RectTransform rect, string text, int size, TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = _font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            return label;
        }

        internal static Sprite Art(string name)
        {
            return Load<Sprite>(ArtFolder + name + ".png");
        }

        private static Sprite PlayerShip()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(PlayerSheet))
            {
                if (asset is Sprite sprite && sprite.name == PlayerFrame)
                    return sprite;
            }

            return null;
        }

        private static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
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
