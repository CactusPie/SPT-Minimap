using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CactusPie.MapLocation.Minimap.Data.Enums;
using CactusPie.MapLocation.Minimap.Events;
using CactusPie.MapLocation.Minimap.Helpers;
using CactusPie.MapLocation.Minimap.MapHandlers;
using CactusPie.MapLocation.Minimap.MapHandlers.Data;
using CactusPie.MapLocation.Minimap.MapHandlers.Interfaces;

namespace CactusPie.MapLocation.Minimap;

public partial class MapControl : UserControl
{
    public IMap? SelectedMap { get; private set; }
    public double PlayerMapXPosition { get; set; } = 1.0f;
    public double PlayerMapZPosition { get; set; } = 1.0f;

    public event EventHandler<MouseButtonEventArgs>? MouseClickedOnCanvas;
    public event EventHandler<MapSelectionChangedEventArgs>? MapSelectionChanged;
    
    private readonly RotateTransform _playerDotRotation = new();
    private readonly ScaleTransform _playerDotScale = new();
    private List<IMap>? _maps;
    private bool _drawingEnabled;

    public bool DrawingEnabled
    {
        get => _drawingEnabled;
        set
        {
            _drawingEnabled = value;
            DrawingInkCanvas.IsHitTestVisible = value;
        }
    }

    public MapControl()
    {
        InitializeComponent();
    }

    public void CenterMapOnPlayer()
    {
        TranslateTransform translateTransform = MapZoomBorder.GetTranslateTransform();
        ScaleTransform scaleTransform = MapZoomBorder.GetScaleTransform();

        translateTransform.X = -Canvas.GetLeft(PlayerDot) * scaleTransform.ScaleX + MapImage.Width / 2;
        translateTransform.Y = -Canvas.GetTop(PlayerDot) * scaleTransform.ScaleY + MapImage.Height / 2;
    }

    public void ClearDrawing()
    {
        DrawingInkCanvas.Strokes.Clear();
    }
    
    public void UndoLastDrawing()
    {
        if (DrawingInkCanvas.Strokes.Count > 0)
        {
            DrawingInkCanvas.Strokes.RemoveAt(DrawingInkCanvas.Strokes.Count - 1);
        }
    }

    public void SetPlayerDotPosition(double xPosition, double zPosition, float angle = 0)
    {
        angle -= 180;

        PlayerMapXPosition = xPosition;
        PlayerMapZPosition = zPosition;
        Canvas.SetLeft(PlayerDot, xPosition - 10);
        Canvas.SetTop(PlayerDot, zPosition - 10);
        _playerDotRotation.Angle = angle;
    }

    public Point GetMousePositionOnCanvas()
    {
        return Mouse.GetPosition(PlayerOverlayCanvas);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_playerDotRotation);
        transformGroup.Children.Add(_playerDotScale);

        PlayerDot.RenderTransform = transformGroup;

        MapComboBox.SelectionChanged += MapComboBoxOnSelectionChanged;
        ReloadMaps();
    }

    public void ReloadMaps()
    {
        var maps = new List<IMap>(16);
        string mapsPath = PathHelper.GetAbsolutePath(@"Maps");
        foreach (string filePath in Directory.GetFiles(mapsPath).Where(x => x.EndsWith("json")))
        {
            var fileContent = File.ReadAllText(filePath);
            MapData? mapData = JsonSerializer.Deserialize<MapData>(fileContent);

            if (mapData == null)
            {
                throw new NullReferenceException("Could not deserialize the map data");
            }
            
            maps.Add(new CustomMap(mapData));
        }

        _maps = maps.OrderBy(x => x.MapName).ToList();
        
        MapComboBox.ItemsSource = _maps.Select(x => x.MapName).ToList();

        if (MapComboBox.SelectedIndex == -1 && _maps.Any())
        {
            MapComboBox.SelectedValue = _maps[0].MapName;
        }

        if (SelectedMap != null)
        {
            LoadMap(SelectedMap.MapName);
        }
    }

    public void SelectMap(string mapName)
    {
        MapComboBox.SelectedValue = mapName;
    }

    public void SetPlayerDotImage(PlayerDotType playerDotType)
    {
        string playerDotImageSource = playerDotType switch
        {
            PlayerDotType.Circle => "pack://application:,,,/Resources/circle.png",
            PlayerDotType.CircleWithArrow => "pack://application:,,,/Resources/circle_with_arrow.png",
            _ => throw new ArgumentOutOfRangeException(nameof(playerDotType), playerDotType, $"Invalid {nameof(PlayerDotType)} value")
        };

        PlayerDot.Source = new BitmapImage(new Uri(playerDotImageSource));
    }

    private void MapComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedMapName = e.AddedItems[0] as string;

        if (selectedMapName == null)
        {
            throw new InvalidOperationException($"{nameof(selectedMapName)} cannot be null!");
        }

        LoadMap(selectedMapName);

        if (SelectedMap != null)
        {
            OnMapSelectionChanged(new MapSelectionChangedEventArgs(SelectedMap));
        }
    }

    private void LoadMap(string mapName)
    {
        IMap? map = _maps?.FirstOrDefault(x => x.MapName == mapName);

        if (map == null)
        {
            return;
        }

        string imagePath = PathHelper.GetAbsolutePath(@"Maps\Images", map.MapImageFile);
        MapImage.Source = new BitmapImage(new Uri(imagePath));
        SelectedMap = map;
        MapViewBox.RenderTransform = new RotateTransform
        {
            Angle = map.MapRotation
        };
    }

    private void PlayerOverlayCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        OnMouseClickedOnCanvas(e);
    }

    protected virtual void OnMouseClickedOnCanvas(MouseButtonEventArgs eventArgs)
    {
        EventHandler<MouseButtonEventArgs>? handler = MouseClickedOnCanvas;
        handler?.Invoke(this, eventArgs);
    }

    protected virtual void OnMapSelectionChanged(MapSelectionChangedEventArgs e)
    {
        EventHandler<MapSelectionChangedEventArgs>? handler = MapSelectionChanged;
        handler?.Invoke(this, e);
    }

    private void PlayerMarkerScaleSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PlayerDot == null)
        {
            return;
        }

        double scale = PlayerMarkerScaleSlider.Value / 100.0;

        _playerDotScale.ScaleX = scale;
        _playerDotScale.ScaleY = scale;
    }

    private void PlayerTransparencySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PlayerDot == null)
        {
            return;
        }
        
        double opacity = PlayerTransparencySlider.Value / 100.0;
        PlayerDot.Opacity = opacity;
    }
}