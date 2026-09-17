using System.Collections.Generic;
using UnityEngine;

namespace TableImport
{
    public class TableLog
    {
        private readonly List<TableProblem> _problems = new List<TableProblem>();

        public bool HasProblems => _problems.Count > 0;

        public int Count => _problems.Count;

        public void Add(string file, int line, string message)
        {
            _problems.Add(new TableProblem(file, line, message));
        }

        public void Report()
        {
            foreach (TableProblem problem in _problems)
                Debug.LogError("Таблицы: " + problem);
        }
    }
}
