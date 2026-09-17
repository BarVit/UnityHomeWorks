namespace TableImport
{
    public class TableProblem
    {
        public TableProblem(string file, int line, string message)
        {
            File = file;
            Line = line;
            Message = message;
        }

        public string File { get; private set; }

        public int Line { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            if (Line > 0)
                return File + ":" + Line + " — " + Message;

            return File + " — " + Message;
        }
    }
}
