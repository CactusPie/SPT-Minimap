using System;

namespace CactusPie.MapLocation.Minimap.Events;

public class MapPositionDataReceivedEventArgs : EventArgs
{
    public string? MapName { get; }

    public float XPosition { get; }

    public float YPosition { get; }

    public float ZPosition { get; }

    public float XRotation { get; }

    public float YRotation { get; }

    public MapPositionDataReceivedEventArgs(string? mapName, float xPosition, float yPosition, float zPosition, float xRotation, float yRotation)
    {
        MapName = mapName;
        XPosition = xPosition;
        YPosition = yPosition;
        ZPosition = zPosition;
        XRotation = xRotation;
        YRotation = yRotation;
    }
}