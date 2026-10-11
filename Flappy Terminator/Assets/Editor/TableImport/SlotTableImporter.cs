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
            "gun", "slot", "price", "weapon"
        };

        public const string ModuleSourcePath = "Assets/Data/Tables/module_slots.tsv";
        public const string ModuleAssetPath = "Assets/Data/Generated/ModuleSlots.asset";

        private static readonly string[] ModuleColumns =
        {
            "slot", "price"
        };

        public static SlotTable ImportModuleSlots(TableLog log)
        {
            TsvDocument document = TsvReader.Read(ModuleSourcePath, log);

            if (document == null || document.HasColumns(ModuleColumns, log) == false)
                return null;

            List<SlotRow> rows = new List<SlotRow>();

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                int slot = reader.ReadInt("slot", FirstSlot, 3);
                int price = reader.ReadInt("price", 0, 10000000);

                if (slot != FirstSlot + rows.Count)
                    reader.Fail("slot: ждали " + (FirstSlot + rows.Count) + ", слоты идут подряд с 1");

                rows.Add(new SlotRow(SlotRow.AllGuns, slot, price, string.Empty));
            }

            SlotTable table = GeneratedAsset.LoadOrCreate<SlotTable>(ModuleAssetPath);
            table.Fill(rows.ToArray());
            EditorUtility.SetDirty(table);

            return table;
        }

        public static SlotTable Import(WeaponTable weapons, TableLog log)
        {
            TsvDocument document = TsvReader.Read(SourcePath, log);

            if (document == null)
                return null;

            if (document.HasColumns(Columns, log) == false)
                return null;

            List<SlotRow> rows = new List<SlotRow>();
            List<RowReader> gunRows = new List<RowReader>();
            int commonCount = 0;

            foreach (TsvRow raw in document.Rows)
            {
                RowReader reader = new RowReader(document, raw, log);
                string gun = reader.ReadOptionalText("gun");
                int slot = reader.ReadInt("slot", FirstSlot, 16);
                int price = reader.ReadInt("price", 0, 10000000);
                string weapon = reader.ReadOptionalText("weapon");

                if (gun.Length > 0)
                {
                    CheckWeapon(weapons, gun, reader);

                    if (slot == FirstSlot)
                        reader.Fail("slot: первый ствол у пушки есть сразу, своя цена бывает у стволов 2 и дальше");

                    if (weapon.Length > 0)
                        reader.Fail("weapon: стартовая пушка задаётся только в общей строке слота 1");

                    gunRows.Add(reader);
                    rows.Add(new SlotRow(gun, slot, price, string.Empty));

                    continue;
                }

                if (slot != FirstSlot + commonCount)
                    reader.Fail("slot: ждали " + (FirstSlot + commonCount) + ", общие слоты идут подряд с 1");

                if (slot == FirstSlot && (price != 0 || weapon.Length == 0))
                    reader.Fail("слот 1 открыт с начала: цена 0 и стартовая пушка в weapon");

                CheckWeapon(weapons, weapon, reader);

                rows.Add(new SlotRow(SlotRow.AllGuns, slot, price, weapon));
                commonCount++;
            }

            foreach (RowReader reader in gunRows)
            {
                if (reader.ReadInt("slot", FirstSlot, 16) > commonCount)
                    reader.Fail("slot: такого слота нет в общих строках (их " + commonCount + ")");
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
