using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CactusPie.MapLocation.Minimap.Data;
using CactusPie.MapLocation.Minimap.Helpers;
using CactusPie.MapLocation.Minimap.MapHandlers.Data;
using CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;
using CactusPie.MapLocation.Minimap.Services.Interfaces;

namespace CactusPie.MapLocation.Minimap.Services;

public class MapCreationDataManager : IMapCreationDataManager
{
    public MapCoefficientsGenerationResult MapCoefficientsGenerationResult(string mapPositionMappings, int polynomialDegree)
    {
        if (string.IsNullOrEmpty(mapPositionMappings))
        {
            return new MapCoefficientsGenerationResult(null, false, "Map coefficients cannot be empty");
        }

        var gameXPositions = new List<double>();
        var mapXPositions = new List<double>();
        var gameZPositions = new List<double>();
        var mapZPositions = new List<double>();

        string[] lines = mapPositionMappings.Split("\n");
        
        for (var lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = lines[lineNumber];

            string[] split = line.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 4)
            {
                continue;
            }

            if (!double.TryParse(split[0], out double gameXPosition))
            {
                return new MapCoefficientsGenerationResult(null, false, $"Failed to parse first coordinate at line {lineNumber + 1}");
            }
            
            if (!double.TryParse(split[1], out double mapXPosition))
            {
                return new MapCoefficientsGenerationResult(null, false, $"Failed to parse second coordinate at line {lineNumber + 1}");
            }
            
            if (!double.TryParse(split[2], out double gameZPosition))
            {
                return new MapCoefficientsGenerationResult(null, false, $"Failed to parse third coordinate at line {lineNumber + 1}");
            }
            
            if (!double.TryParse(split[3], out double mapZPosition))
            {
                return new MapCoefficientsGenerationResult(null, false, $"Failed to parse fourth coordinate at line {lineNumber + 1}");
            }

            gameXPositions.Add(gameXPosition);
            mapXPositions.Add(mapXPosition);
            gameZPositions.Add(gameZPosition);
            mapZPositions.Add(mapZPosition);
        }

        if (gameXPositions.Count <= 1)
        {
            return new MapCoefficientsGenerationResult(null, false, "You need to add at least 2 sets of map coordinates first");
        }

        double[] gameXPositionsArray = gameXPositions.ToArray();
        double[] mapXPositionsArray = mapXPositions.ToArray();
        double[] gameZPositionsArray = gameZPositions.ToArray();
        double[] mapZPositionsArray = mapZPositions.ToArray();
        
        double[] xCoefficients = PolynomialHelper.FitPolynomial(gameXPositionsArray, mapXPositionsArray, polynomialDegree);
        double[] zCoefficients = PolynomialHelper.FitPolynomial(gameZPositionsArray, mapZPositionsArray, polynomialDegree);

        var coefficients = new MapCoefficients
        {
            XCoefficients = xCoefficients,
            ZCoefficients = zCoefficients,
            GameXPositionsArray  = gameXPositionsArray,
            MapXPositionsArray = mapXPositionsArray,
            GameZPositionsArray = gameZPositionsArray,
            MapZPositionsArray = mapZPositionsArray
        };

        return new MapCoefficientsGenerationResult(coefficients, true);
    }
    
    public (bool Success, string? ErrorMessage) AddNewMap(string mapName, double mapRotation, string mapImagePath)
    {
        string mapImageFileName = Path.GetFileName(mapImagePath);
                
        string newMapFileNamePath = PathHelper.GetAbsolutePath($@"Maps\{mapName}.json");
        string newMapImageDestinationFilePath = PathHelper.GetAbsolutePath($@"Maps\Images\{mapImageFileName}");

        if (File.Exists(newMapFileNamePath))
        {
            return (false, "A map with that name already exists");
        }

        var mapData = new MapData
        {
            MapName = mapName,
            MapRotation = mapRotation,
            MapImageFile = mapImageFileName,
            XCoefficients = Array.Empty<double>(),
            ZCoefficients = Array.Empty<double>(),
        };

        string serializedMapData = JsonSerializer.Serialize(mapData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
                
        File.WriteAllText(newMapFileNamePath, serializedMapData);
        File.Copy(mapImagePath, newMapImageDestinationFilePath, true);

        return (true, null);
    }

    public void SaveMapPositionData(IMap selectedMap, string positionMappings)
    {
        string positionsPath = PathHelper.GetAbsolutePath($@"Maps\MapCreationData\{selectedMap.MapName}.txt");

        if (!File.Exists(positionsPath))
        {
            string? directoryName = Path.GetDirectoryName(positionsPath) ?? throw new InvalidOperationException($"Invalid path: {positionsPath}");
            
            Directory.CreateDirectory(directoryName);
            File.Create(positionsPath).Dispose();
        }
            
        File.WriteAllText(positionsPath, positionMappings);
    }

    public string? GetMapPositionData(IMap map)
    {
        string positionsPath = PathHelper.GetAbsolutePath($@"Maps\MapCreationData\{map.MapName}.txt");

        if (!File.Exists(positionsPath))
        {
            return null;
        }
            
        string positionData = File.ReadAllText(positionsPath);
        return positionData;
    }

    public void UpdateMapUsingMapData(MapData mapData)
    {
        string newMapFileNamePath = PathHelper.GetAbsolutePath($@"Maps\{mapData.MapName}.json");

        string serializedMapData = JsonSerializer.Serialize(mapData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(newMapFileNamePath, serializedMapData);
    }
}