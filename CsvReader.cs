public static class CsvReader
{
    public static List<PersonRecord> ReadData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"CSV file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);

        if (lines.Length <= 1)
        {
            return new List<PersonRecord>();
        }

        var records = new List<PersonRecord>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');

            if (parts.Length < 7)
            {
                Console.WriteLine($"Skipping invalid CSV row: {line}");
                continue;
            }

            var record = new PersonRecord
            {
                FirstName = parts[0].Trim(),
                LastName = parts[1].Trim(),
                Email = parts[2].Trim(),
                Age = parts[3].Trim(),
                Salary = parts[4].Trim(),
                Department = parts[5].Trim(),
                UpdatedSalary = parts[6].Trim()
            };

            records.Add(record);
        }

        return records;
    }
}