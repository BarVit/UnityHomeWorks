using UnityEditor;
using UnityEngine;

namespace EnemyContent
{
    public class BossSpawnerWindow : EditorWindow
    {
        private const string BossesPath = "Assets/Data/Generated/Bosses.asset";
        private const string WeaponsPath = "Assets/Data/Generated/Weapons.asset";

        private Vector2 _scroll;

        [MenuItem("Tools/Enemies/Boss Spawner")]
        public static void Open()
        {
            GetWindow<BossSpawnerWindow>("Боссы");
        }

        private void OnGUI()
        {
            BossTable bosses = AssetDatabase.LoadAssetAtPath<BossTable>(BossesPath);
            WeaponTable weapons = AssetDatabase.LoadAssetAtPath<WeaponTable>(WeaponsPath);

            if (bosses == null || weapons == null)
            {
                EditorGUILayout.HelpBox("Нет сгенерированных таблиц — запустите Tools/Tables/Reimport All.",
                    MessageType.Warning);

                return;
            }

            if (Application.isPlaying == false)
                EditorGUILayout.HelpBox("Выпустить босса можно только в Play.", MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (BossRow row in bosses.Rows)
                DrawRow(row, weapons);

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(BossRow row, WeaponTable weapons)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(row.Title, GUILayout.Width(140));
            WeaponRow weapon = weapons.GetRow(row.Weapon);
            string damage = weapon == null ? "?" : weapon.Damage.ToString();

            EditorGUILayout.LabelField(row.Health + " HP, " + row.Weapon + " ×" + row.Barrels + ", урон "
                + damage + (row.Phases.Count > 0 ? ", фаз " + row.Phases.Count : ""));

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Выпустить", GUILayout.Width(90)))
                Spawn(row, weapons);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private static void Spawn(BossRow row, WeaponTable weapons)
        {
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();

            if (spawner == null)
            {
                Debug.LogWarning("Боссы: в сцене нет EnemySpawner");

                return;
            }

            spawner.Spawn(EnemySetup.ForBoss(row, weapons));
        }
    }
}
