using System.Reflection;
using System.Text;
using LabApi.Loader;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace OverseerAC.Plugin.TrustFactor.Features;

public static class Localization
{
    private static readonly Dictionary<string, string> _DefaultLanguages = new Dictionary<string, string>()
    {
        { "english", ReadManifestData("Languages.english.yml") },
        { "polish", ReadManifestData("Languages.polish.yml") }
    };

    private static readonly Dictionary<LangEntry, string> _LoadedLanguage = new Dictionary<LangEntry, string>();

    public static string GetLocalizedEntry(LangEntry entry)
    {
        return _LoadedLanguage.ContainsKey(entry) ? _LoadedLanguage[entry] : string.Empty;
    }

    public static void CreateDefaultLangFiles()
    {
        if (TrustFactorEntryPoint.Instance == null)
            throw new InvalidOperationException("Plugin must be initialized for this operation!");

        DirectoryInfo cfgDir = TrustFactorEntryPoint.Instance.GetConfigDirectory();

        foreach (KeyValuePair<string, string> langToFile in _DefaultLanguages)
        {
            if (!File.Exists(Path.Combine(cfgDir.FullName, $"{langToFile.Key}.yml")))
                File.WriteAllText(Path.Combine(cfgDir.FullName, $"{langToFile.Key}.yml"), langToFile.Value);
        }
    }

    public static void LoadLanguage(string langName)
    {
        _LoadedLanguage.Clear();

        if (TrustFactorEntryPoint.Instance == null)
            throw new InvalidOperationException("Plugin must be initialized for this operation!");

        DirectoryInfo cfgDir = TrustFactorEntryPoint.Instance.GetConfigDirectory();

        if (!File.Exists(Path.Combine(cfgDir.FullName, $"{langName}.yml")))
        {
            Logger.Warn($"The specified language \"{langName}\" does not exist! Falling back to english...");
            langName = "english";
        }

        string fileContent = File.ReadAllText(Path.Combine(cfgDir.FullName, $"{langName}.yml"));

        IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

        var result = deserializer.Deserialize<Dictionary<string, string>>(fileContent);
        var fallbackResult = deserializer.Deserialize<Dictionary<string, string>>(_DefaultLanguages["english"]);

        foreach (LangEntry langEntry in Enum.GetValues(typeof(LangEntry)))
        {
            if (!result.ContainsKey(langEntry.ToString()))
            {
                Logger.Warn($"LangEntry \"{langEntry}\" does not exist in the language file. Using fallback...");

                if (!fallbackResult.ContainsKey(langEntry.ToString()))
                {
                    Logger.Error($"LangEntry \"{langEntry}\" does not exist in the fallback language file. Things will break!");

                    _LoadedLanguage.Add(langEntry, string.Empty);

                    continue;
                }

                _LoadedLanguage.Add(langEntry, fallbackResult[langEntry.ToString()]);

                continue;
            }

            _LoadedLanguage.Add(langEntry, result[langEntry.ToString()]);
        }

        Logger.Info(GetLocalizedEntry(LangEntry.LanguageLoaded));
    }

    private static string ReadManifestData(string embeddedFileName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException("Could not load manifest resource stream.");
        }

        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public enum LangEntry
    {
        LanguageLoaded,
    }
}