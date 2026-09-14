using System.ComponentModel;

namespace OverseerAC.Plugin.TrustFactor;

public class Config
{
    [Description("When enabled, players marked as untrusted will not be kicked. A warning in the console will be displayed instead.")]
    public bool TestMode = false;

    [Description("Path to the language file to be used. Will try to load from embedded resources first.")]
    public string Language = "Languages.english.yml";

    [Description("Base API url for OverseerAC. *Do not edit unless you know what you're doing!*")]
    public string BaseApiUrl = "https://overseer.eu/api";

    [Description("Your OverseerAC API key, *DO NOT SHARE IT!*. If you don't have one, you can request it at our Discord, which can be found here: https://github.com/GraczBezNicku/OverseerAC.Plugin.TrustFactor")]
    public string ApiKey = "";
}