using System;
using System.Net;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

namespace CactusPie.MapLocation
{
    public class MapLocationController : MonoBehaviour
    {
        private MapLocationBroadcastService _mapLocationBroadcastService;
        
        [UsedImplicitly]
        public void Start()
        {
            var gamePlayerOwner = GetLocalPlayerFromWorld().GetComponentInChildren<GamePlayerOwner>();
            MapLocationPlugin.RefreshIntervalMillieconds.SettingChanged += RefreshIntervalSecondsOnSettingChanged;
            
            if (_mapLocationBroadcastService == null)
            {
                IPAddress ipAddress = IPAddress.Parse(MapLocationPlugin.DestinationIpAddress.Value);
                var destinationEndpoint = new IPEndPoint(ipAddress, MapLocationPlugin.DestinationPort.Value);
                _mapLocationBroadcastService = new MapLocationBroadcastService(gamePlayerOwner, destinationEndpoint);
            }

            _mapLocationBroadcastService.StartBroadcastingPosition(MapLocationPlugin.RefreshIntervalMillieconds.Value);
        }

        private void RefreshIntervalSecondsOnSettingChanged(object sender, EventArgs e)
        {
            _mapLocationBroadcastService.ChangeInterval(MapLocationPlugin.RefreshIntervalMillieconds.Value);
        }

        [UsedImplicitly]
        public void Stop()
        {
            _mapLocationBroadcastService?.StopBroadcastingPosition();
        }
        
        private Player GetLocalPlayerFromWorld()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                return null;
            }

            return gameWorld.MainPlayer;
        }

        [UsedImplicitly]
        public void OnDestroy()
        {
            _mapLocationBroadcastService.Dispose();
            Destroy(this);
        }
    }
}