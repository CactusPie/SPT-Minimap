using System.Collections.Generic;

namespace CactusPie.MapLocation.Minimap.MapHandlers.Data;

// is this class needed?
public class MapData
{
    public string? MapName { get; init; }
    public string? MapImageFile { get; init; }
    public double MapRotation { get; init; }
    public IReadOnlyList<double>? XCoefficients { get; init; }
    public IReadOnlyList<double>? ZCoefficients { get; init; }
}