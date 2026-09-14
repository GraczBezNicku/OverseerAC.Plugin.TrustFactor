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

namespace OverseerAC.Plugin.TrustFactor;

public class TrustFactorEntryPoint : Plugin<Config>
{
    public static TrustFactorEntryPoint? Instance { get; private set; }

    public override string Name => "OverseerAC.Plugin.TrustFactor";

    public override string Description => "SCP:SL plugin used to determine trust of users joining a server using OverseerAC.";

    public override string Author => "GBN";

    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;

    public override void Disable()
    {
        LabApi.Events.Handlers.PlayerEvents.Joined -= PlayerTrustCheck;

        Instance = null;
    }

    public override void Enable()
    {
        Instance = this;

        Localization.CreateDefaultLangFiles();
        Localization.LoadLanguage(Config.Language);

        LabApi.Events.Handlers.PlayerEvents.Joined += PlayerTrustCheck;
    }

    public void PlayerTrustCheck(PlayerJoinedEventArgs ev)
    {
        if (ev.Player.ReferenceHub.authManager.BypassBansFlagSet)
            return;

        StringBuilder fullUrl = new StringBuilder();

        fullUrl.Append(Config.BaseApiUrl);
        fullUrl.Append("trust/");
        fullUrl.Append(ev.Player.UserId);
        fullUrl.Append($"?key={Config.ApiKey}");

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
                        ev.Player.Kick(Localization.GetLocalizedEntry(Localization.LangEntry.UntrustedKickMessage));
                }
                else
                {
                    Logger.Error($"{Localization.GetLocalizedEntry(Localization.LangEntry.RequestFailed)}\n{webRequest.error}");
                }
            }
        }
    }
}
