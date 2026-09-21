using System.Text;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

using OverseerAC.Plugin.TrustFactor.Features;

using UnityEngine.Networking;
using UnityEngine;

using MEC;
using OverseerAC.Plugin.TrustFactor.Models;
using Utf8Json;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using GameCore;

using Version = System.Version;

namespace OverseerAC.Plugin.TrustFactor;

public class TrustFactorEntryPoint : Plugin<Config>
{
    public static TrustFactorEntryPoint? Instance { get; private set; }

    public override string Name => "OverseerAC.Plugin.TrustFactor";

    public override string Description => "SCP:SL plugin used to determine trust of users joining a server using OverseerAC.";

    public override string Author => "GBN";

    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;

    public override Version Version => new Version(1, 1, 0);

    public override void LoadConfigs()
    {
        if (!this.TryLoadConfig(ConfigFileName, out Config? config, true))
        {
            Logger.Warn("Failed to load the configuration file, using default values.");
            config = new Config();
        }

        Config = config;

        if (!Config.LocalConfig)
            return;

        if (this.TryReadConfig(ConfigFileName, out Config? localConfig))
        {
            Config = localConfig;
        }
        else
        {
            this.TrySaveConfig(Config, ConfigFileName, false);
        }
    }

    public override void Disable()
    {
        LabApi.Events.Handlers.PlayerEvents.Joined -= PlayerTrustCheck;
        LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= OutdatedVersionCheck;

        Instance = null;
    }

    public override void Enable()
    {
        Instance = this;

        Localization.CreateDefaultLangFiles();
        Localization.LoadLanguage(Config.Language);

        LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += OutdatedVersionCheck;
        LabApi.Events.Handlers.PlayerEvents.Joined += PlayerTrustCheck;

        if (Config.TestMode)
            Logger.Warn(Localization.GetLocalizedEntry(Localization.LangEntry.TestModeEnabledMessage));
    }

    public void OutdatedVersionCheck()
    {
        string formattedVersion = $"{Version.Major}.{Version.Minor}.{Version.Build}";

        StringBuilder fullUrl = new StringBuilder();

        fullUrl.Append(Config.BaseApiUrl);
        fullUrl.Append("trust/pluginver");

        Timing.RunCoroutine(GetRequest(fullUrl.ToString()));

        IEnumerator<float> GetRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 10;

                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string receivedVer = webRequest.downloadHandler.text;

                    if (formattedVersion != receivedVer)
                    {
                        Logger.Warn(Localization.GetLocalizedEntry(Localization.LangEntry.OutdatedVersionWarning)
                            .Replace("%CURRVER%", formattedVersion)
                            .Replace("%NEWVER%", receivedVer));
                    }
                }

                // Ignore no response. Version is not important.
            }
        }
    }

    public void PlayerTrustCheck(PlayerJoinedEventArgs ev)
    {
        if (ev.Player.ReferenceHub.authManager.BypassBansFlagSet || ev.Player.RemoteAdminAccess)
            return;

        StringBuilder fullUrl = new StringBuilder();

        fullUrl.Append(Config.BaseApiUrl);
        fullUrl.Append("trust/");
        fullUrl.Append(ev.Player.UserId);
        fullUrl.Append($"?key={Config.ApiKey}");
        fullUrl.Append($"&playerAddress={ev.Player.IpAddress}");

        Timing.RunCoroutine(GetRequest(fullUrl.ToString()));

        IEnumerator<float> GetRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 10;

                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    TrustCheckModel response = JsonSerializer.Deserialize<TrustCheckModel>(webRequest.downloadHandler.text);

                    if (!response.trusted)
                    {
                        if (!Config.TestMode)
                            ev.Player.Disconnect(Localization.GetLocalizedEntry(Localization.LangEntry.UntrustedKickMessage));
                        else
                        {
                            string msg = Localization.GetLocalizedEntry(Localization.LangEntry.TestModeUntrustedWarning)
                                .Replace("%AUTHID%", ev.Player.UserId)
                                .Replace("%NICK%", ev.Player.Nickname);

                            Logger.Warn(msg);

                            foreach (Player p in Player.ReadyList)
                            {
                                if (!p.ReferenceHub.serverRoles.AdminChatPerms)
                                    continue;

                                p.SendBroadcast($"<color=#800000FF>[OverseerAC]</color> {msg}", 3);
                            }
                        }
                    }
                }
                else
                {
                    Logger.Error($"{Localization.GetLocalizedEntry(Localization.LangEntry.RequestFailed)}\n{webRequest.error}");
                }
            }
        }
    }
}
