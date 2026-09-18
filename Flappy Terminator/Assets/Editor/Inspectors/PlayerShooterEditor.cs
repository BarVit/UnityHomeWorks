using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerShooter))]
public class PlayerShooterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty weapons = serializedObject.FindProperty("_weapons");
        SerializedProperty weaponId = serializedObject.FindProperty("_weaponId");
        SerializedProperty barrels = serializedObject.FindProperty("_barrels");

        EditorGUILayout.PropertyField(weapons);

        WeaponTable table = weapons.objectReferenceValue as WeaponTable;

        if (HasRows(table))
            DrawChoice(table, weaponId, barrels);
        else
            DrawFallback(weaponId, barrels);

        DrawPropertiesExcluding(serializedObject, "m_Script", "_weapons", "_weaponId", "_barrels");

        serializedObject.ApplyModifiedProperties();
    }

    private bool HasRows(WeaponTable table)
    {
        return table != null && table.Rows != null && table.Rows.Count > 0;
    }

    private void DrawChoice(WeaponTable table, SerializedProperty weaponId, SerializedProperty barrels)
    {
        string[] ids = new string[table.Rows.Count];

        for (int i = 0; i < ids.Length; i++)
            ids[i] = table.Rows[i].Id;

        int index = Array.IndexOf(ids, weaponId.stringValue);

        if (index < 0)
            index = 0;

        index = EditorGUILayout.Popup("Weapon", index, ids);
        weaponId.stringValue = ids[index];

        WeaponRow row = table.Rows[index];
        int maxBarrels = Mathf.Max(1, row.BarrelsMax);

        barrels.intValue = EditorGUILayout.IntSlider("Barrels", Mathf.Clamp(barrels.intValue, 1, maxBarrels),
            1, maxBarrels);

        EditorGUILayout.HelpBox(Describe(row), MessageType.None);
    }

    private void DrawFallback(SerializedProperty weaponId, SerializedProperty barrels)
    {
        EditorGUILayout.HelpBox("Подключите таблицу оружия, чтобы выбирать пушку из списка.",
            MessageType.Info);
        EditorGUILayout.PropertyField(weaponId);
        EditorGUILayout.PropertyField(barrels);
    }

    private string Describe(WeaponRow row)
    {
        return row.Delivery + ", урон " + row.Damage
            + ", темп " + row.Rate.ToString("0.##") + " с"
            + ", залп " + row.FirePattern
            + ", стволов до " + row.BarrelsMax;
    }
}
