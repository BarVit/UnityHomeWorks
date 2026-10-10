using System.Collections.Generic;
using UnityEditor;

namespace TableImport
{
    public static class SlotTableImporter
    {
        public const string SourcePath = "Assets/Data/Tables/slots.tsv";
        public const string AssetPath = "Assets/Data/Generated/Slots.asset";

        private const int FirstSlot = 1;

        private static readonly string[] Columns =
        {
            "slot", "price", "weapon"
        };

        public static SlotTable Import(WeaponTable weapons, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<SlotRow> rows = new List<SlotRow>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                int slot = reader.ReadInt("slot", FirstSlot, 16);
                int price = reader.ReadInt("price", 0, 10000000);
                string weapon = reader.ReadOptionalText("weapon");

                if (slot != FirstSlot + rows.Count)
                    reader.Fail("slot: ждали " + (FirstSlot + rows.Count) + ", слоты идут подряд с 1");

                if (slot == FirstSlot && (price != 0 || weapon.Length == 0))
                    reader.Fail("слот 1 открыт с начала: цена 0 и стартовая пушка в weapon");

                CheckWeapon(weapons, weapon, reader);

                rows.Add(new SlotRow(slot, price, weapon));
            }

            SlotTable table = GeneratedAsset.LoadOrCreate<SlotTable>(AssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        private static void CheckWeapon(WeaponTable weapons, string weapon, RowReader reader)
        {
            if (weapons == null || weapon.Length == 0)
                return;

            WeaponRow row = weapons.GetRow(weapon);

            if (row == null)
                reader.Fail("weapon: нет оружия с id " + weapon + " в " + WeaponTableImporter.SourcePath);
            else if (row.Owner != WeaponOwner.Player)
                reader.Fail("weapon: " + weapon + " не пушка игрока");
        }
    }
}
