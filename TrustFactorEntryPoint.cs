using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace OverseerAC.Plugin.TrustFactor;

public class TrustFactorEntryPoint : Plugin<Config>
{
    public override string Name => "OverseerAC.Plugin.TrustFactor";

    public override string Description => "SCP:SL plugin used to determine trust of users joining a server using OverseerAC.";

    public override string Author => "GBN";

    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;

    public override void Disable()
    {

    }

    public override void Enable()
    {

    }
}
