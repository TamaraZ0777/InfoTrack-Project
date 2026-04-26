public class CsvReadResult
{
    public List<PersonRecord> Records { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public static class CsvReader
{
    public static CsvReadResult ReadData(string filePath)
    {
        var result = new CsvReadResult();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"CSV file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);

        if (lines.Length <= 1)
        {
            result.Warnings.Add("CSV file contains no data rows.");
            return result;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                result.Warnings.Add($"Line {i + 1}: empty row skipped.");
                continue;
            }

            var parts = line.Split(',');

            if (parts.Length < 7)
            {
                result.Warnings.Add($"Line {i + 1}: row skipped because it has fewer than 7 columns.");
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

            result.Records.Add(record);
        }

        AddDuplicateEmailWarnings(result);

        return result;
    }

    private static void AddDuplicateEmailWarnings(CsvReadResult result)
    {
        var duplicateEmails = result.Records
            .Where(r => !string.IsNullOrWhiteSpace(r.Email))
            .GroupBy(r => r.Email)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicateEmails)
        {
            result.Warnings.Add(
                $"Duplicate email found in CSV: {group.Key} appears {group.Count()} times."
            );
        }
    }
}