using System.Collections.Generic;

namespace CactusPie.MapLocation.Minimap.Data;

public class MapCoefficients
{
    public double[] XCoefficients { get; init; } = default!;
    
    public double[] ZCoefficients { get; init; } = default!;
    
    public double[] GameXPositionsArray  { get; init; } = default!;
    
    public double[] MapXPositionsArray { get; init; } = default!;
    
    public double[] GameZPositionsArray { get; init; } = default!;
    
    public double[] MapZPositionsArray { get; init; } = default!;
}