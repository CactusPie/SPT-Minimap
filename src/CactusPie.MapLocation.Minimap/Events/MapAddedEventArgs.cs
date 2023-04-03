using System;

namespace CactusPie.MapLocation.Minimap.Events;

public class MapAddedEventArgs : EventArgs
{
    public string MapName { get; set; }

    public string MapImagePath { get; set; }

    public double MapRotation { get; set; }

    public MapAddedEventArgs(string mapName, string mapImagePath, double mapRotation)
    {
        MapName = mapName;
        MapImagePath = mapImagePath;
        MapRotation = mapRotation;
    }
}