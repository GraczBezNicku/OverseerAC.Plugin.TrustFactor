using System.ComponentModel;

namespace OverseerAC.Plugin.TrustFactor;

public class Config
{
    [Description("When enabled, players marked as untrusted will not be kicked. A warning in the console will be displayed instead.")]
    public bool TestMode { get; set; } = false;

    [Description("Language to be used. Searches in the config folder of the plugin.")]
    public string Language { get; set; } = "english";

    [Description("Base API url for OverseerAC. *Do not edit unless you know what you're doing!*")]
    public string BaseApiUrl { get; set; } = "https://overseer.eu/api";

    [Description("Your OverseerAC API key, *DO NOT SHARE IT!*. If you don't have one, you can request it at our Discord, which can be found here: https://github.com/GraczBezNicku/OverseerAC.Plugin.TrustFactor")]
    public string ApiKey { get; set; } = "";
}