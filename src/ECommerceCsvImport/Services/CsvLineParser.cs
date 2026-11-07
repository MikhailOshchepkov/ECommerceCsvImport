using System.Text;

namespace ECommerceCsvImport.Services
{
    // Разбор одной строки CSV на поля.
    // Поддерживает поля в кавычках (внутри может быть разделитель)
    // и экранированные кавычки ("" превращается в ").
    public static class CsvLineParser
    {
        public static List<string> Parse(string line)
        {
            return Parse(line, ',');
        }

        public static List<string> Parse(string line, char delimiter)
        {
            List<string> fields = new List<string>();
            if (line == null)
                return fields;

            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Две кавычки подряд внутри кавычек - это одна кавычка в тексте
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString().Trim());
            return fields;
        }

        // Определяем разделитель по строке заголовка: какой символ встречается чаще
        public static char DetectDelimiter(string headerLine)
        {
            int commas = headerLine.Count(c => c == ',');
            int semicolons = headerLine.Count(c => c == ';');
            int tabs = headerLine.Count(c => c == '\t');

            if (semicolons > commas && semicolons > tabs)
                return ';';
            if (tabs > commas && tabs > semicolons)
                return '\t';
            return ',';
        }

        // Подготовка значения для записи в CSV (используется при экспорте)
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(',') || value.Contains('"'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }
    }
}
