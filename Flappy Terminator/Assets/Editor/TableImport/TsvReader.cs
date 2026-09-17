using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableImport
{
    public static class TsvReader
    {
        public static TsvDocument Read(string path, TableLog log)
        {
            string text;

            try
            {
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8, true))
                    text = reader.ReadToEnd();
            }
            catch (IOException exception)
            {
                log.Add(path, 0, "файл не читается: " + exception.Message);

                return null;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            List<string> columns = null;
            List<TsvRow> rows = new List<TsvRow>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (IsBlank(lines[i]))
                    continue;

                List<string> cells = SplitLine(lines[i]);

                if (columns == null)
                {
                    columns = Trim(cells);

                    continue;
                }

                rows.Add(new TsvRow(i + 1, cells));
            }

            if (columns == null)
            {
                log.Add(path, 0, "файл пуст: нет даже строки заголовка");

                return null;
            }

            return new TsvDocument(path, columns, rows);
        }

        private static bool IsBlank(string line)
        {
            foreach (char symbol in line)
            {
                if (symbol != '\t' && char.IsWhiteSpace(symbol) == false)
                    return false;
            }

            return true;
        }

        private static List<string> Trim(List<string> cells)
        {
            List<string> trimmed = new List<string>(cells.Count);

            foreach (string cell in cells)
                trimmed.Add(cell.Trim().TrimStart('﻿'));

            return trimmed;
        }

        private static List<string> SplitLine(string line)
        {
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < line.Length; i++)
            {
                char symbol = line[i];

                if (quoted)
                {
                    if (symbol != '"')
                    {
                        cell.Append(symbol);
                    }
                    else if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else if (symbol == '"' && cell.Length == 0)
                {
                    quoted = true;
                }
                else if (symbol == '\t')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                }
                else
                {
                    cell.Append(symbol);
                }
            }

            cells.Add(cell.ToString());

            return cells;
        }
    }
}
