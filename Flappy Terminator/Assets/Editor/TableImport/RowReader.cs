using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace TableImport
{
    public class RowReader
    {
        private const string NoneMark = "none";

        private readonly TsvDocument _document;
        private readonly TsvRow _row;
        private readonly TableLog _log;

        public RowReader(TsvDocument document, TsvRow row, TableLog log)
        {
            _document = document;
            _row = row;
            _log = log;
        }

        public int Line => _row.Line;

        public string ReadText(string column)
        {
            string text = ReadCell(column);

            if (text.Length == 0)
                Fail("пустое значение в столбце " + column);

            return text;
        }

        public string ReadOptionalText(string column)
        {
            return ReadCell(column);
        }

        public int ReadInt(string column, int minimum, int maximum)
        {
            string text = ReadCell(column);
            int value;

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) == false)
            {
                Fail(column + ": " + Describe(text) + " не целое число");

                return minimum;
            }

            if (value < minimum || value > maximum)
                Fail(column + ": " + value + " вне допустимого диапазона " + minimum + ".." + maximum);

            return value;
        }

        public Color ReadColor(string column)
        {
            string text = ReadCell(column);

            if (ColorUtility.TryParseHtmlString("#" + text, out Color color))
                return color;

            Fail(column + ": " + Describe(text) + " не цвет вида RRGGBB");

            return Color.white;
        }

        public float ReadFloat(string column, float minimum, float maximum)
        {
            string text = ReadCell(column);

            if (text.Contains(","))
            {
                Fail(column + ": " + Describe(text)
                    + " — десятичная запятая; в таблице нужна точка, иначе Excel и игра поймут число по-разному");

                return minimum;
            }

            float value;

            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) == false)
            {
                Fail(column + ": " + Describe(text) + " не число");

                return minimum;
            }

            if (value < minimum || value > maximum)
                Fail(column + ": " + value + " вне допустимого диапазона " + minimum + ".." + maximum);

            return value;
        }

        public TEnum ReadEnum<TEnum>(string column) where TEnum : struct
        {
            string text = ReadCell(column);
            TEnum value;

            if (Enum.TryParse(text, true, out value) == false || Enum.IsDefined(typeof(TEnum), value) == false)
            {
                Fail(column + ": " + Describe(text) + " не одно из "
                    + string.Join(", ", Enum.GetNames(typeof(TEnum))));

                return default(TEnum);
            }

            return value;
        }

        public TComponent ReadPrefab<TComponent>(string column, bool required) where TComponent : Component
        {
            string name = ReadCell(column);

            if (name.Length == 0 || name == NoneMark)
            {
                if (required)
                    Fail("пустая ссылка на префаб в столбце " + column);

                return null;
            }

            foreach (string guid in AssetDatabase.FindAssets(name + " t:Prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (System.IO.Path.GetFileNameWithoutExtension(path) != name)
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                TComponent found = prefab.GetComponent<TComponent>();

                if (found != null)
                    return found;

                Fail(column + ": на префабе " + name + " нет компонента " + typeof(TComponent).Name);

                return null;
            }

            Fail(column + ": не найден префаб с именем " + name);

            return null;
        }

        public TAsset ReadAsset<TAsset>(string column, bool required) where TAsset : UnityEngine.Object
        {
            string name = ReadCell(column);

            if (name.Length == 0 || name == NoneMark)
            {
                if (required)
                    Fail("пустая ссылка на ассет в столбце " + column);

                return null;
            }

            foreach (string guid in AssetDatabase.FindAssets(name + " t:" + typeof(TAsset).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                foreach (UnityEngine.Object candidate in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    TAsset found = candidate as TAsset;

                    if (found != null && found.name == name)
                        return found;
                }
            }

            Fail(column + ": не найден " + typeof(TAsset).Name + " с именем " + name);

            return null;
        }

        public void Fail(string message)
        {
            _log.Add(_document.File, _row.Line, message);
        }

        private string ReadCell(string column)
        {
            int index = _document.GetColumnIndex(column);

            if (index < 0)
                return string.Empty;

            return _row.GetCell(index);
        }

        private string Describe(string text)
        {
            if (text.Length == 0)
                return "пусто";

            return "\"" + text + "\"";
        }
    }
}
