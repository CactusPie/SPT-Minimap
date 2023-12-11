using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CactusPie.MapLocation.DI;
using CactusPie.MapLocation.Patches;
using CactusPie.MapLocation.Services;
using CactusPie.MapLocation.Services.Airdrop;
using JetBrains.Annotations;

namespace CactusPie.MapLocation
{
    [BepInPlugin("com.cactuspie.maplocation", "CactusPie.MapLocation", "2.0.2")]
    public sealed class MapLocationPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource MapLocationLogger { get; private set; }

        internal static ConfigEntry<string> ListenIpAddress { get; private set; }

        internal static ConfigEntry<int> ListenPort { get; private set; }

        // We use LightInject here for easy and lightweight dependency injection
        internal static ServiceContainer ServiceContainer { get; private set; }

        [UsedImplicitly]
        internal void Start()
        {
            MapLocationLogger = Logger;
            MapLocationLogger.LogInfo("MapLocation loaded");

            const string configSection = "Map settings";

            ListenIpAddress = Config.Bind(
                configSection,
                nameof(ListenIpAddress),
                "127.0.0.1",
                new ConfigDescription("Listen IP Address (requires restarting the game)")
            );

            ListenPort = Config.Bind(
                configSection,
                nameof(ListenPort),
                45365,
                new ConfigDescription(
                    "Destination Port (requires restarting the game)",
                    new AcceptableValueRange<int>(1024, 65535)
                )
            );

            ServiceContainer = ContainerBuilder.BuildContainer();
            var server = ServiceContainer.GetInstance<IMapDataServer>();
            server.StartServer(ListenIpAddress.Value, ListenPort.Value);

            new MapLocationPatch().Enable();
            new AirdropMapLocationPatch(ServiceContainer.GetInstance<IAirdropService>()).Enable();
            new TryNotifyConditionChangedPatch(ServiceContainer.GetInstance<IMapDataServer>()).Enable();
        }
    }
}