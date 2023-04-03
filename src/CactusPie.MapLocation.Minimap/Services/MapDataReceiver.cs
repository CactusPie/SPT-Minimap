using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CactusPie.MapLocation.Minimap.Events;
using CactusPie.MapLocation.Minimap.Services.Interfaces;

namespace CactusPie.MapLocation.Minimap.Services;

public sealed class MapDataReceiver : IMapDataReceiver
{
    private readonly UdpClient _udpClient;
    private IPEndPoint _receiveEndpoint;
    private CancellationTokenSource? _dataReceivingCancellationToken;

    public event EventHandler<MapPositionDataReceivedEventArgs>? MapPositionDataReceived;

    public MapDataReceiver(Func<UdpClient> udpClientFactory, IPEndPoint receiveEndpoint)
    {
        _udpClient = udpClientFactory();
        _receiveEndpoint = receiveEndpoint;
        _udpClient.Client.Bind(_receiveEndpoint);
    }

    public void StartReceivingData()
    {
        if (_dataReceivingCancellationToken != null)
        {
            throw new InvalidOperationException("The receive operation is already running");
        }
        
        Task.Run(() =>
        {
            _dataReceivingCancellationToken = new CancellationTokenSource();
            CancellationToken cancellationToken = _dataReceivingCancellationToken.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] receivedData;
                const int blockingCallInterruptedErrorCode = 10004;
                
                try
                {
                    receivedData = _udpClient.Receive(ref _receiveEndpoint);
                }
                catch (SocketException e) when(e.ErrorCode == blockingCallInterruptedErrorCode)
                {
                    return;
                }

                var mapNameLength = BitConverter.ToInt32(receivedData, 0);
                int offset = sizeof(int);
                
                string mapName = Encoding.UTF8.GetString(receivedData, offset, mapNameLength);
                offset += mapNameLength;
                
                var xPosition = BitConverter.ToSingle(receivedData, offset);
                offset += sizeof(float);
                
                var yPosition = BitConverter.ToSingle(receivedData, offset);
                offset += sizeof(float);
                
                var zPosition = BitConverter.ToSingle(receivedData, offset);
                offset += sizeof(float);
                
                var xRotation = BitConverter.ToSingle(receivedData, offset);
                offset += sizeof(float);
                
                var yRotation = BitConverter.ToSingle(receivedData, offset);

                OnMapPositionDataReceived(new MapPositionDataReceivedEventArgs(mapName, xPosition, yPosition, zPosition, xRotation, yRotation));
            }
        });
    }

    public void StopReceivingData()
    {
        _dataReceivingCancellationToken?.Cancel();
    }

    private void OnMapPositionDataReceived(MapPositionDataReceivedEventArgs e)
    {
        EventHandler<MapPositionDataReceivedEventArgs>? handler = MapPositionDataReceived;
        handler?.Invoke(this, e);
    }

    public void Dispose()
    {
        _udpClient.Dispose();
        if (_dataReceivingCancellationToken != null)
        {
            _dataReceivingCancellationToken?.Cancel();
            _dataReceivingCancellationToken?.Dispose();
        }
    }
}