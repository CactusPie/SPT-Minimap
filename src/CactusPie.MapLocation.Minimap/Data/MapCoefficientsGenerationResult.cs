namespace CactusPie.MapLocation.Minimap.Data;

public class MapCoefficientsGenerationResult
{
    public MapCoefficients? MapCoefficients { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }
    
    public MapCoefficientsGenerationResult(MapCoefficients? mapCoefficients, bool success, string? errorMessage = null)
    {
        MapCoefficients = mapCoefficients;
        Success = success;
        ErrorMessage = errorMessage;
    }
}