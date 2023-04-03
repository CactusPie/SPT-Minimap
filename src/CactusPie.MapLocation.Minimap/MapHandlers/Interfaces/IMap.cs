namespace CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;

public interface IMap
{
    string MapImageFile { get; }
    string MapName { get; }
    double TransformXPosition(double mapXPosition);
    double TransformZPosition(double mapZPosition);
    double MapRotation => 0.0f;
}