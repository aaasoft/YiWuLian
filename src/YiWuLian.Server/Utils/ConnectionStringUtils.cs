using System;

namespace YiWuLian.Server.Utils;

public class ConnectionStringUtils
{
    public const string PARAMETER_SPLIT_STRING = ";";
    public const string KEY_VALUE_SPLIT_STRING = "=";
    public static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(connectionString))
            return dict;

        foreach (var parameters in connectionString.Split(PARAMETER_SPLIT_STRING))
        {
            if (string.IsNullOrEmpty(parameters))
                continue;
            var index = parameters.IndexOf(KEY_VALUE_SPLIT_STRING);
            if (index <= 0)
                continue;
            var key = parameters.Substring(0, index).Trim();
            if (string.IsNullOrEmpty(key))
                continue;
            var value = parameters.Substring(index + 1).Trim();
            if (string.IsNullOrEmpty(value))
                continue;
            dict[key] = value;
        }
        return dict;
    }

    public static string ToConnectionString(Dictionary<string, string> dict)
    {
        return string.Join(PARAMETER_SPLIT_STRING, dict.Where(t => !string.IsNullOrEmpty(t.Key) && !string.IsNullOrEmpty(t.Value))
            .Select(t => $"{t.Key}={t.Value}"));
    }
}
