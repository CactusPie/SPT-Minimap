using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using EFT;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CactusPie.MapLocation
{
    public sealed class MapLocationBroadcastService : IDisposable
    {
        private readonly GamePlayerOwner _gamePlayerOwner;
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _sendEndpoint;
        private readonly Timer _timer;

        public MapLocationBroadcastService(GamePlayerOwner gamePlayerOwner, IPEndPoint ipEndPoint)
        {
            _gamePlayerOwner = gamePlayerOwner;
            _udpClient = new UdpClient();
            _sendEndpoint = ipEndPoint;
            _timer = new Timer
            {
                AutoReset = true,
                Interval = 1000,
            };

            _timer.Elapsed += (sender, args) =>
            {
                try
                {
                    SendData();
                }
                catch (Exception e)
                {
                    MapLocationPlugin.MapLocationLogger.LogError($"Exception {e.GetType()} occured. Message: {e.Message}. StackTrace: {e.StackTrace}");
                }
            };
        }

        public void StartBroadcastingPosition(double interval = 1000)
        {
            _timer.Interval = interval;
            _timer.Start();
        }

        public void StopBroadcastingPosition()
        {
            _timer.Stop();
        }
        
        public void ChangeInterval(double interval)
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Interval = interval;
                _timer.Start();
            }
            else
            {
                _timer.Interval = interval;
            }
        }

        private void SendData()
        {
            string mapName = _gamePlayerOwner.Player.Location;
            Vector3 playerPosition = _gamePlayerOwner.Player.Position;
            Vector2 playerRotation = _gamePlayerOwner.Player.Rotation;

            byte[] mapNameBytes = Encoding.UTF8.GetBytes(mapName);
            
            IEnumerable<byte[]> GetArrays()
            {
                yield return BitConverter.GetBytes(mapNameBytes.Length);
                yield return mapNameBytes;
                yield return BitConverter.GetBytes(playerPosition.x);
                yield return BitConverter.GetBytes(playerPosition.y);
                yield return BitConverter.GetBytes(playerPosition.z);
                yield return BitConverter.GetBytes(playerRotation.x);
                yield return BitConverter.GetBytes(playerRotation.y);
            };

            byte[] content = GetArrays().SelectMany(x => x).ToArray();

            _udpClient.Send(content, content.Length, _sendEndpoint);
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
            _timer?.Dispose();
        }
    }
}