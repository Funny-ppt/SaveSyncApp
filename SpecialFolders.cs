using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SaveSyncApp;

internal static class SpecialFolders
{
    public readonly static string UserProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public readonly static string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public readonly static string ApplicationDataFolder2 = Path.Combine(UserProfileFolder, "AppData", "LocalLow");
    public readonly static string DocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public const string UserProfilePlaceholder = "<USERPROFILE>";
    public const string ApplicationDataPlaceholder = "<APPDATA>";
    public const string ApplicationDataPlaceholder2 = "<LOCALLOW>";
    public const string DocumentPlaceholder = "<DOCUMENTS>";

    public readonly static Dictionary<string, string> PlaceholderToPath = new()
    {
        { UserProfilePlaceholder, UserProfileFolder },
        { ApplicationDataPlaceholder, ApplicationDataFolder },
        { ApplicationDataPlaceholder2, ApplicationDataFolder2 },
        { DocumentPlaceholder, DocumentFolder },
    };

    public readonly static Dictionary<string, string> PathToPlaceholder = new()
    {
        { UserProfileFolder, UserProfilePlaceholder },
        { ApplicationDataFolder, ApplicationDataPlaceholder },
        { ApplicationDataFolder2, ApplicationDataPlaceholder2 },
        { DocumentFolder, DocumentPlaceholder },
    };

    static readonly Regex _regex1 = new(@$"{UserProfilePlaceholder}|{ApplicationDataPlaceholder}|{ApplicationDataPlaceholder2}|{DocumentPlaceholder}", RegexOptions.Compiled);
    public static string ReplacePlaceholdersWithPaths(string input)
    {
        return _regex1.Replace(input, match =>
        {
            if (PlaceholderToPath.TryGetValue(match.Value, out var path))
            {
                return path;
            }
            return match.Value;
        });
    }

    public static string ReplacePathsWithPlaceholds(string input)
    {
        foreach (var pair in PathToPlaceholder)
        {
            input = input.Replace(pair.Key, pair.Value);
        }
        return input;
    }
}
