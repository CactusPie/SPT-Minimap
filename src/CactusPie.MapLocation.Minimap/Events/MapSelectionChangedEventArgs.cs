using System;
using CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;

namespace CactusPie.MapLocation.Minimap.Events;

public class MapSelectionChangedEventArgs : EventArgs
{
    public IMap SelectedMap { get; }

    public MapSelectionChangedEventArgs(IMap selectedMap)
    {
        SelectedMap = selectedMap;
    }
}