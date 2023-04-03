using CactusPie.MapLocation.Minimap.Data;
using CactusPie.MapLocation.Minimap.MapHandlers.Data;
using CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;

namespace CactusPie.MapLocation.Minimap.Services.Interfaces;

public interface IMapCreationDataManager
{
    MapCoefficientsGenerationResult MapCoefficientsGenerationResult(string mapPositionMappings, int polynomialDegree);
    (bool Success, string? ErrorMessage) AddNewMap(string mapName, double mapRotation, string mapImagePath);
    void SaveMapPositionData(IMap selectedMap, string positionMappings);
    string? GetMapPositionData(IMap map);
    void UpdateMapUsingMapData(MapData mapData);
}