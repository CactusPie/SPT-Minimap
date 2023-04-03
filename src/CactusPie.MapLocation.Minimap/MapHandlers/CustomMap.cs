using System;
using System.Collections.Generic;
using System.Linq;
using CactusPie.MapLocation.Minimap.Helpers;
using CactusPie.MapLocation.Minimap.MapHandlers.Data;
using CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;

namespace CactusPie.MapLocation.Minimap.MapHandlers;

public class CustomMap : IMap
{
    public string MapImageFile { get; }
    public string MapName { get; }
    private IReadOnlyList<double> XCoefficients { get; }
    private IReadOnlyList<double> ZCoefficients { get; }
    public double MapRotation { get; }

    public CustomMap(MapData mapData)
    {
        ArgumentNullException.ThrowIfNull(mapData.MapName);
        ArgumentNullException.ThrowIfNull(mapData.MapImageFile);
        ArgumentNullException.ThrowIfNull(mapData.XCoefficients);
        ArgumentNullException.ThrowIfNull(mapData.ZCoefficients);
        
        MapName = mapData.MapName;
        MapImageFile = mapData.MapImageFile;
        XCoefficients = mapData.XCoefficients;
        ZCoefficients = mapData.ZCoefficients;
        MapRotation = mapData.MapRotation;
    }
    
    public double TransformXPosition(double mapXPosition)
    {
        return PolynomialHelper.CalculatePolynomialValue(mapXPosition, XCoefficients);
    }

    public double TransformZPosition(double mapZPosition)
    {
        return PolynomialHelper.CalculatePolynomialValue(mapZPosition, ZCoefficients);
    }
}