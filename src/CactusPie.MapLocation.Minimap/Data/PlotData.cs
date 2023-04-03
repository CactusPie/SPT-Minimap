using System.Collections.Generic;

namespace CactusPie.MapLocation.Minimap.Data;

public class PlotData
{
    public double[]? GameCoordinates { get; init; }
    public double[]? MapCoordinates { get; init; }
    public double[]? Coefficients { get; init; }
}