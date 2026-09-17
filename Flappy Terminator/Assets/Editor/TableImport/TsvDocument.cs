using System.Collections.Generic;

namespace TableImport
{
    public class TsvDocument
    {
        private readonly List<string> _columns;
        private readonly List<TsvRow> _rows;

        public TsvDocument(string file, List<string> columns, List<TsvRow> rows)
        {
            File = file;
            _columns = columns;
            _rows = rows;
        }

        public string File { get; private set; }

        public IReadOnlyList<TsvRow> Rows => _rows;

        public bool HasColumns(string[] expected, TableLog log)
        {
            bool complete = true;

            foreach (string column in expected)
            {
                if (_columns.Contains(column) == false)
                {
                    log.Add(File, 1, "нет столбца " + column
                        + "; в файле сейчас: " + string.Join(", ", _columns));
                    complete = false;
                }
            }

            foreach (string column in _columns)
            {
                if (System.Array.IndexOf(expected, column) < 0)
                {
                    log.Add(File, 1, "лишний столбец " + column
                        + "; ожидались: " + string.Join(", ", expected));
                    complete = false;
                }
            }

            return complete;
        }

        public int GetColumnIndex(string column)
        {
            return _columns.IndexOf(column);
        }
    }
}
