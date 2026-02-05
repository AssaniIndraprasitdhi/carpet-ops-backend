namespace CarpetOpsSystem.Config;

public static class EnvLoader
{
    public static void Load(string filePath = ".env")
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Warning: {filePath} not found. Using system environment variables.");
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = trimmedLine.Substring(0, separatorIndex).Trim();
            var value = trimmedLine.Substring(separatorIndex + 1).Trim();

            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
