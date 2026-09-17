using System.Collections.Generic;

namespace TableImport
{
    public class TsvRow
    {
        private readonly List<string> _cells;

        public TsvRow(int line, List<string> cells)
        {
            Line = line;
            _cells = cells;
        }

        public int Line { get; private set; }

        public string GetCell(int index)
        {
            if (index < 0 || index >= _cells.Count)
                return string.Empty;

            return _cells[index].Trim();
        }
    }
}
