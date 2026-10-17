using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    public static partial class DockScreenBuilder
    {
        private const string ArtFolder = "Assets/Sprites/UI/Dock/";
                private const string TitleSprite = "Assets/Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/buttons/button_active.png";
        private const string PlayerSheet = "Assets/Sprites/Ships/Player.png";
        private const string PlayerFrame = "tinyShip5_0";
        private const string SlotsAsset = "Assets/Data/Generated/Slots.asset";
        private const string UpgradesAsset = "Assets/Data/Generated/WeaponUpgrades.asset";
        private const string ModulesAsset = "Assets/Data/Generated/Modules.asset";
        private const string ModuleUpgradesAsset = "Assets/Data/Generated/ModuleUpgrades.asset";
        private const string ModuleSlotsAsset = "Assets/Data/Generated/ModuleSlots.asset";
        private const string SkillsAsset = "Assets/Data/Generated/Skills.asset";
        private const string WeaponsAsset = "Assets/Data/Generated/Weapons.asset";
        private const string LocationsAsset = "Assets/Data/Generated/Locations.asset";
        private const string SystemsAsset = "Assets/Data/Generated/Systems.asset";

        private const string TooltipName = "Number Tooltip";
        private const int PanelBorder = 30;
        private const int SlotBorder = 22;
        private const int GlowPadding = 16;
        private const int SlotCount = 4;
        private const int ModuleSlotCount = 3;
        private const int UpgradeRowCount = 3;
        private const int SkillRowCount = 4;
        private const float RowStep = 74f;
        private const float SkillListY = -15f;
        private const float SkillListWidth = 624f;
        private const float ScrollbarWidth = 6f;
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

            ModuleRack modules = GetOrAdd<ModuleRack>(economy.gameObject);
            Set(modules, "_economy", economy);
            Set(modules, "_modules", AssetDatabase.LoadAssetAtPath<ModuleTable>(ModulesAsset));
            Set(modules, "_upgrades", AssetDatabase.LoadAssetAtPath<ModuleUpgradeTable>(ModuleUpgradesAsset));
            Set(modules, "_slots", AssetDatabase.LoadAssetAtPath<SlotTable>(ModuleSlotsAsset));

            PlayerShip playerShip = Object.FindAnyObjectByType<PlayerShip>(FindObjectsInactive.Include);
            SkillBook skills = GetOrAdd<SkillBook>(economy.gameObject);
            Set(skills, "_economy", economy);
            Set(skills, "_weapons", AssetDatabase.LoadAssetAtPath<WeaponTable>(WeaponsAsset));
            Set(skills, "_skills", AssetDatabase.LoadAssetAtPath<SkillTable>(SkillsAsset));
            Set(skills, "_ship", playerShip);

            EnergyShield shield = BuildShield(playerShip);
            BattleModules battle = GetOrAdd<BattleModules>(game.gameObject);
            Set(battle, "_input", Object.FindAnyObjectByType<InputReader>(FindObjectsInactive.Include));
            Set(battle, "_ship", playerShip);
            Set(battle, "_shield", shield);

            Set(game, "_moduleRack", modules);
            Set(game, "_skills", skills);
            Set(game, "_battleModules", battle);

            _tooltip = BuildTooltip((RectTransform)dock.transform);
            BuildDock(dock, hangar, modules, skills, economy);
            BuildHud(runner);
            BuildModuleHud((RectTransform)dock.transform, battle);
            BuildBossAnnouncement((RectTransform)dock.transform,
                Object.FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include));

            EditorSceneManager.MarkSceneDirty(dock.gameObject.scene);
            Debug.Log("Док: экран собран.");
        }

        private static void PrepareSprites()
        {
            Prepare("Dock Background", 0);
            Prepare("Dock Panel", PanelBorder);
            Prepare("Slot Frame", SlotBorder);
            Prepare("Slot Selected", SlotBorder + GlowPadding);
            Prepare("Rating Star", 0);

            foreach (string name in new[] { "Coin Icon", "XP Icon", "Lock Icon", "Damage Icon", "Crit Icon", "Upgrade Pip",
                         "Energy Shield Icon", "Health Icon", "Cooldown Icon", "Double Shot Icon",
                         "Speed Icon", "Napalm Icon", "Turn Icon", "Reload Icon", "White Square" })
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

        private static void BuildDock(DockScreen dock, Hangar hangar, ModuleRack modules, SkillBook skills, Economy economy)
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
            Button reset = AddButton(root, "Reset Button", "RESET", new Vector2(0f, 1f), new Vector2(120f, -202f),
                new Vector2(160f, 48f), 22);
            TMP_Text spent = AddText(Place("Spent", root, new Vector2(0f, 1f), new Vector2(360f, -272f), new Vector2(640f, 84f)),
                "", 18, TextAlignmentOptions.TopLeft, Muted);

            RectTransform wallet = Place("Wallet", root, new Vector2(1f, 1f), new Vector2(-250f, -58f), new Vector2(420f, 76f));
            AddPanel(wallet);
            CompactNumber xp = AddCounter(wallet, "XP", "XP Icon", -100f);
            CompactNumber money = AddCounter(wallet, "Money", "Coin Icon", 100f);

            Button shopButton = AddButton(root, "Shop Button", "SHOP", new Vector2(0.5f, 1f), new Vector2(40f, -58f), new Vector2(220f, 76f), 34);

            RectTransform loadout = Place("Loadout", root, new Vector2(1f, 0.5f), new Vector2(-350f, -2f), new Vector2(640f, 486f));
            AddPanel(loadout);

            TabView[] tabs =
            {
                AddTab(loadout, 0, "WEAPON", new Vector2(-200f, 207f), new Vector2(190f, 48f)),
                AddTab(loadout, 1, "MODULES", new Vector2(0f, 207f), new Vector2(190f, 48f)),
                AddTab(loadout, 2, "SKILLS", new Vector2(200f, 207f), new Vector2(190f, 48f)),
            };

            RectTransform weaponPage = Stretch("Weapon Page", loadout);
            RectTransform modulePage = Stretch("Module Page", loadout);
            RectTransform skillPage = Stretch("Skill Page", loadout);

            SlotView[] slots = new SlotView[SlotCount];

            for (int i = 0; i < SlotCount; i++)
                slots[i] = AddSlot(weaponPage, i + 1, new Vector2(-226.5f + i * 151f, 128f));

            AddDivider(weaponPage);

            RectTransform upgrades = Stretch("Upgrades", weaponPage);
            Image weaponIcon = AddImage(Place("Weapon Icon", upgrades, new Vector2(0.5f, 0.5f), new Vector2(-200f, 34f), new Vector2(150f, 52f)), null);
            weaponIcon.preserveAspect = true;
            TMP_Text weaponTitle = AddText(Place("Weapon Title", upgrades, new Vector2(0.5f, 0.5f), new Vector2(70f, 34f), new Vector2(330f, 44f)),
                "", 30, TextAlignmentOptions.Left, Color.white);
            UpgradeRowView[] weaponRows = AddUpgradeRows(upgrades, UpgradeRowCount, -22f);

            SlotView[] moduleSlots = new SlotView[ModuleSlotCount];

            for (int i = 0; i < ModuleSlotCount; i++)
                moduleSlots[i] = AddSlot(modulePage, i + 1, new Vector2(-151f + i * 151f, 128f));

            AddDivider(modulePage);

            RectTransform modulePanel = Stretch("Module", modulePage);
            TMP_Text moduleTitle = AddText(Place("Module Title", modulePanel, new Vector2(0.5f, 0.5f), new Vector2(-65f, 34f), new Vector2(330f, 44f)),
                "", 30, TextAlignmentOptions.Left, Color.white);
            Button remove = AddButton(modulePanel, "Remove Button", "REMOVE", new Vector2(0.5f, 0.5f), new Vector2(222f, 34f), new Vector2(140f, 46f), 20);
            UpgradeRowView[] moduleRows = AddUpgradeRows(modulePanel, UpgradeRowCount, -22f);
            TMP_Text moduleHint = AddText(Place("Hint", modulePage, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(560f, 80f)),
                "EMPTY SLOT\nBUY A MODULE IN THE SHOP", 24, TextAlignmentOptions.Center, Muted);

            AddText(Place("Header", skillPage, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(580f, 34f)),
                "PILOT SKILLS ARE PAID WITH XP", 22, TextAlignmentOptions.Center, Muted);
            UpgradeRowView[] skillRows = AddSkillList(skillPage, out RectTransform skillContent);

            Button menu = AddButton(root, "Menu Button", "MENU", new Vector2(0f, 0f), new Vector2(150f, 62f), new Vector2(220f, 76f), 32);
            AddMapButton(root);
            Button play = AddButton(root, "Play Button", "PLAY", new Vector2(1f, 0f), new Vector2(-190f, 62f), new Vector2(300f, 96f), 40);

            ShopWindow shop = BuildShop(root, hangar, modules, economy);

            so.FindProperty("_hangar").objectReferenceValue = hangar;
            so.FindProperty("_modules").objectReferenceValue = modules;
            so.FindProperty("_skills").objectReferenceValue = skills;
            so.FindProperty("_economy").objectReferenceValue = economy;
            so.FindProperty("_xp").objectReferenceValue = xp;
            so.FindProperty("_money").objectReferenceValue = money;
            so.FindProperty("_earned").objectReferenceValue = earned.gameObject;
            so.FindProperty("_earnedXp").objectReferenceValue = earnedXp;
            so.FindProperty("_earnedMoney").objectReferenceValue = earnedMoney;
            so.FindProperty("_status").objectReferenceValue = status;
            so.FindProperty("_resetButton").objectReferenceValue = reset;
            so.FindProperty("_spent").objectReferenceValue = spent;
            SetArray(so.FindProperty("_tabs"), tabs);
            SetArray(so.FindProperty("_pages"), new Object[] { weaponPage.gameObject, modulePage.gameObject, skillPage.gameObject });
            SetArray(so.FindProperty("_slots"), slots);
            so.FindProperty("_weaponIcon").objectReferenceValue = weaponIcon;
            so.FindProperty("_weaponTitle").objectReferenceValue = weaponTitle;
            so.FindProperty("_upgradePanel").objectReferenceValue = upgrades.gameObject;
            SetArray(so.FindProperty("_upgrades"), weaponRows);
            SetArray(so.FindProperty("_moduleSlots"), moduleSlots);
            so.FindProperty("_modulePanel").objectReferenceValue = modulePanel.gameObject;
            so.FindProperty("_moduleTitle").objectReferenceValue = moduleTitle;
            so.FindProperty("_removeModuleButton").objectReferenceValue = remove;
            SetArray(so.FindProperty("_moduleUpgrades"), moduleRows);
            so.FindProperty("_moduleHint").objectReferenceValue = moduleHint;
            SetArray(so.FindProperty("_skillRows"), skillRows);
            so.FindProperty("_skillContent").objectReferenceValue = skillContent;
            so.FindProperty("_skillRowStep").floatValue = RowStep;
            so.FindProperty("_coinIcon").objectReferenceValue = Art("Coin Icon");
            so.FindProperty("_xpIcon").objectReferenceValue = Art("XP Icon");
            so.FindProperty("_damageIcon").objectReferenceValue = Art("Damage Icon");
            so.FindProperty("_critIcon").objectReferenceValue = Art("Crit Icon");
            so.FindProperty("_doubleShotIcon").objectReferenceValue = Art("Double Shot Icon");
            so.FindProperty("_speedIcon").objectReferenceValue = Art("Speed Icon");
            so.FindProperty("_napalmIcon").objectReferenceValue = Art("Napalm Icon");
            so.FindProperty("_turnIcon").objectReferenceValue = Art("Turn Icon");
            so.FindProperty("_reloadIcon").objectReferenceValue = Art("Reload Icon");
            so.FindProperty("_cooldownIcon").objectReferenceValue = Art("Cooldown Icon");
            so.FindProperty("_healthIcon").objectReferenceValue = Art("Health Icon");
            so.FindProperty("_shop").objectReferenceValue = shop;
            so.FindProperty("_shopButton").objectReferenceValue = shopButton;
            so.FindProperty("_menuButton").objectReferenceValue = menu;
            so.FindProperty("_playButton").objectReferenceValue = play;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddDivider(RectTransform parent)
        {
            RectTransform divider = Place("Divider", parent, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(580f, 2f));
            AddImage(divider, null).color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);
        }

        private static UpgradeRowView[] AddUpgradeRows(RectTransform parent, int count, float top)
        {
            UpgradeRowView[] rows = new UpgradeRowView[count];

            for (int i = 0; i < count; i++)
                rows[i] = AddUpgradeRow(parent, "Row " + (i + 1), top - i * RowStep);

            return rows;
        }

        private static UpgradeRowView[] AddSkillList(RectTransform page, out RectTransform content)
        {
            RectTransform viewport = Place("Skill List", page, new Vector2(0.5f, 0.5f), new Vector2(0f, SkillListY),
                new Vector2(SkillListWidth, SkillRowCount * RowStep));
            viewport.gameObject.AddComponent<RectMask2D>();
            AddImage(viewport, null).color = Color.clear;

            content = Create("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, SkillRowCount * RowStep);

            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.verticalScrollbar = AddScrollbar(viewport);
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            UpgradeRowView[] rows = new UpgradeRowView[SkillRowCount];

            for (int i = 0; i < SkillRowCount; i++)
            {
                rows[i] = AddUpgradeRow(content, "Row " + (i + 1), 0f);
                RectTransform row = (RectTransform)rows[i].transform;
                row.anchorMin = new Vector2(0.5f, 1f);
                row.anchorMax = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(-ScrollbarWidth, -RowStep * (i + 0.5f));
            }

            return rows;
        }

        private static Scrollbar AddScrollbar(RectTransform viewport)
        {
            RectTransform track = Create("Scrollbar", viewport);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = Vector2.one;
            track.pivot = new Vector2(1f, 0.5f);
            track.offsetMin = new Vector2(-ScrollbarWidth, 0f);
            track.offsetMax = Vector2.zero;
            AddImage(track, null).color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.15f);

            RectTransform area = Stretch("Sliding Area", track);
            RectTransform handle = Stretch("Handle", area);
            Image handleImage = AddImage(handle, null);
            handleImage.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.8f);

            Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            return scrollbar;
        }

        internal static void BuildHud(LocationRunner runner)
        {
            RectTransform canvas = (RectTransform)Object.FindAnyObjectByType<DockScreen>(FindObjectsInactive.Include).transform;
            Vector2 position = HudPosition;
            int sibling = 0;

            foreach (string name in new[] { "Run Reward", "Kills" })
            {
                Transform old = canvas.Find(name);

                if (old == null)
                    continue;

                position = ((RectTransform)old).anchoredPosition;
                sibling = old.GetSiblingIndex();
                Object.DestroyImmediate(old.gameObject);
            }

            RectTransform hud = Place("Kills", canvas, new Vector2(0.5f, 0.5f), position, new Vector2(320f, 40f));
            hud.SetSiblingIndex(sibling);

            AddImage(Place("Icon", hud, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(34f, 34f)), Art("Crit Icon"))
                .preserveAspect = true;
            TMP_Text label = AddText(Place("Value", hud, new Vector2(0f, 0.5f), new Vector2(128f, 0f), new Vector2(180f, 40f)),
                "0 / 0", 30, TextAlignmentOptions.Left, Color.white);

            KillsViewer viewer = hud.gameObject.AddComponent<KillsViewer>();
            Set(viewer, "_stats", Object.FindAnyObjectByType<BattleStats>(FindObjectsInactive.Include));
            Set(viewer, "_label", label);
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

            GameObject selection = AddSelection("Selection", frame);

            Image icon = AddImage(Place("Icon", frame, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(118f, 46f)), null);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            TMP_Text caption = AddText(Place("Caption", frame, new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(126f, 24f)),
                "", 18, TextAlignmentOptions.Center, Muted);
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 11f;
            caption.fontSizeMax = 18f;

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

        private static UpgradeRowView AddUpgradeRow(RectTransform parent, string name, float y)
        {
            RectTransform row = Place(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(600f, 70f));
            Image icon = AddImage(Place("Icon", row, new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(56f, 56f)), null);
            icon.preserveAspect = true;
            TMP_Text title = AddText(Place("Title", row, new Vector2(0f, 0.5f), new Vector2(190f, 18f), new Vector2(240f, 28f)),
                "", 22, TextAlignmentOptions.Left, Color.white);
            TMP_Text hint = AddText(Place("Hint", row, new Vector2(0f, 0.5f), new Vector2(190f, -3f), new Vector2(240f, 20f)),
                "", 14, TextAlignmentOptions.Left, Muted);
            TMP_Text value = AddText(Place("Value", row, new Vector2(0f, 0.5f), new Vector2(190f, -23f), new Vector2(240f, 24f)),
                "", 20, TextAlignmentOptions.Left, Cyan);

            Image[] pips = new Image[PipCount];

            for (int i = 0; i < PipCount; i++)
            {
                pips[i] = AddImage(Place("Pip " + (i + 1), row, new Vector2(0f, 0.5f), new Vector2(330f + i * 22f, 0f), new Vector2(18f, 26f)),
                    Art("Upgrade Pip"));
                pips[i].color = PipOff;
            }

            TMP_Text level = AddText(Place("Level", row, new Vector2(0f, 0.5f), new Vector2(370f, 0f), new Vector2(110f, 28f)),
                "", 20, TextAlignmentOptions.Center, Cyan);

            Button button = AddButton(row, "Buy Button", "", new Vector2(1f, 0.5f), new Vector2(-82f, 0f), new Vector2(156f, 58f), 24);
            RectTransform label = (RectTransform)button.transform.Find("Label");
            label.anchorMin = new Vector2(0f, 0f);
            label.anchorMax = new Vector2(1f, 1f);
            label.offsetMin = new Vector2(52f, 0f);
            label.offsetMax = new Vector2(-14f, 0f);
            TMP_Text priceLabel = label.GetComponent<TMP_Text>();
            priceLabel.alignment = TextAlignmentOptions.Left;
            priceLabel.color = Gold;
            CompactNumber price = AddNumber(priceLabel);
            Image currency = AddImage(Place("Currency", (RectTransform)button.transform, new Vector2(0f, 0.5f), new Vector2(34f, 0f),
                new Vector2(28f, 28f)), Art("Coin Icon"));
            currency.preserveAspect = true;

            UpgradeRowView view = row.gameObject.AddComponent<UpgradeRowView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_hint").objectReferenceValue = hint;
            so.FindProperty("_value").objectReferenceValue = value;
            SetArray(so.FindProperty("_pips"), pips);
            so.FindProperty("_level").objectReferenceValue = level;
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_price").objectReferenceValue = price;
            so.FindProperty("_priceLabel").objectReferenceValue = priceLabel;
            so.FindProperty("_currency").objectReferenceValue = currency;
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

        internal static GameObject AddSelection(string name, RectTransform parent)
        {
            RectTransform rect = Stretch(name, parent);
            rect.offsetMin = -Vector2.one * GlowPadding;
            rect.offsetMax = Vector2.one * GlowPadding;
            Image image = AddImage(rect, Art("Slot Selected"));
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;

            return image.gameObject;
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

        private static T GetOrAdd<T>(GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();

            return component != null ? component : owner.AddComponent<T>();
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
