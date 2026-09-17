using System.Collections.Generic;
using System.Text;

namespace FlappyBridge
{
    public static class Json
    {
        public static string Error(string message)
        {
            return "{\"ok\":false,\"error\":" + Quote(message) + "}";
        }

        public static string Object(params string[] fields)
        {
            return "{" + string.Join(",", fields) + "}";
        }

        public static string Field(string name, string value)
        {
            return Quote(name) + ":" + Quote(value);
        }

        public static string Field(string name, bool value)
        {
            return Quote(name) + ":" + (value ? "true" : "false");
        }

        public static string Field(string name, int value)
        {
            return Quote(name) + ":" + value;
        }

        public static string Raw(string name, string value)
        {
            return Quote(name) + ":" + value;
        }

        public static string Array(IEnumerable<string> items)
        {
            return "[" + string.Join(",", items) + "]";
        }

        public static string Quote(string value)
        {
            if (value == null)
                return "null";

            StringBuilder builder = new StringBuilder(value.Length + 2);
            builder.Append('"');

            foreach (char symbol in value)
            {
                if (symbol == '"')
                    builder.Append("\\\"");
                else if (symbol == '\\')
                    builder.Append("\\\\");
                else if (symbol == '\n')
                    builder.Append("\\n");
                else if (symbol == '\r')
                    builder.Append("\\r");
                else if (symbol == '\t')
                    builder.Append("\\t");
                else if (symbol < ' ')
                    builder.Append("\\u").Append(((int)symbol).ToString("x4"));
                else
                    builder.Append(symbol);
            }

            builder.Append('"');

            return builder.ToString();
        }
    }
}
