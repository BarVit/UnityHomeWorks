using System;
using System.Globalization;

namespace TableImport
{
    public class RowReader
    {
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
