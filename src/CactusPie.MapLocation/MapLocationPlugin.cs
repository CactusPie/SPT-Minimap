using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using JetBrains.Annotations;

namespace CactusPie.MapLocation
{
    [BepInPlugin("com.cactuspie.maplocation", "CactusPie.MapLocation", "1.0.0")]
    public class MapLocationPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource MapLocationLogger { get; private set; }
        internal static ConfigEntry<int> RefreshIntervalMillieconds { get; private set; }
        internal static ConfigEntry<string> DestinationIpAddress { get; private set; }
        internal static ConfigEntry<int> DestinationPort { get; private set; }

        [UsedImplicitly]
        internal void Start()
        {
            MapLocationLogger = Logger;
            MapLocationLogger.LogInfo("MapLocation loaded");

            const string configSection = "Map settings";
            
            RefreshIntervalMillieconds = Config.Bind
            (
                configSection,
                nameof(RefreshIntervalMillieconds),
                1000,
                new ConfigDescription
                (
                    "Map position refresh interval in milliseconds (1 second = 1000 milliseconds)", 
                    new AcceptableValueRange<int>(50, 5000)
                )
            );
            
            DestinationIpAddress = Config.Bind
            (
                configSection,
                nameof(DestinationIpAddress),
                "127.0.0.1",
                new ConfigDescription("Destination IP Address (requires starting a new round)")
            );
            
            DestinationPort = Config.Bind
            (
                configSection,
                nameof(DestinationPort),
                44365,
                new ConfigDescription
                (
                    "Destination Port (requires starting a new round)", 
                    new AcceptableValueRange<int>(1024, 65535)
                )
            );

            new MapLocationPatch().Enable();
        }
        
    }
}
