using System;
using CactusPie.MapLocation.Minimap.Events;

namespace CactusPie.MapLocation.Minimap.Services.Interfaces;

public interface IMapDataReceiver : IDisposable
{
    event EventHandler<MapPositionDataReceivedEventArgs>? MapPositionDataReceived;
    void StartReceivingData();
    void StopReceivingData();
}