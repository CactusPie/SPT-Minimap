using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CactusPie.MapLocation.Minimap.Data;
using CactusPie.MapLocation.Minimap.Data.Enums;
using CactusPie.MapLocation.Minimap.Events;
using CactusPie.MapLocation.Minimap.Helpers;
using CactusPie.MapLocation.Minimap.MapHandlers.Data;
using CactusPie.MapLocation.Minimap.Services.Interfaces;

namespace CactusPie.MapLocation.Minimap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMapCreationDataManager _mapCreationDataManager;
        private readonly Func<AddNewMapDialog> _addNewMapDialogFactory;
        private readonly Func<PlotWindow> _plotWindowFactory;
        private readonly Func<ThemeSelector> _themeSelectorFactory;
        private readonly IMapDataReceiver _mapDataReceiver;

        public MainWindow
        (
            Func<IMapDataReceiver> mapDataReceiverFactory,
            IMapCreationDataManager mapCreationDataManager,
            Func<AddNewMapDialog> addNewMapDialogFactory,
            Func<PlotWindow> plotWindowFactory,
            Func<ThemeSelector> themeSelectorFactory
        )
        {
            _mapCreationDataManager = mapCreationDataManager;
            _addNewMapDialogFactory = addNewMapDialogFactory;
            _plotWindowFactory = plotWindowFactory;
            _themeSelectorFactory = themeSelectorFactory;
            _mapDataReceiver = mapDataReceiverFactory();
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ReloadMapCreationPositionData();
            ControlsStackPanel.Children.Add(_themeSelectorFactory());

            _mapDataReceiver.MapPositionDataReceived += MapDataReceiverOnMapPositionDataReceived;
            _mapDataReceiver.StartReceivingData();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _mapDataReceiver.StopReceivingData();
            _mapDataReceiver.Dispose();
        }

        private void MapDataReceiverOnMapPositionDataReceived(object? sender, MapPositionDataReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (MapControl.SelectedMap == null)
                {
                    return;
                }
                
                double normalizedXPosition = MapControl.SelectedMap.TransformXPosition(e.XPosition);
                double normalizedZPosition = MapControl.SelectedMap.TransformZPosition(e.ZPosition);

                if (MapCreationModeCheckbox.IsChecked == true)
                {
                    CurrentPositionTextBox.Text = $"{e.XPosition} {MapControl.PlayerMapXPosition} {e.ZPosition} {MapControl.PlayerMapZPosition}";
                }
                else
                {
                    MapControl.SetPlayerDotPosition(normalizedXPosition, normalizedZPosition, e.XRotation);
                    if (FollowPlayerCheckbox.IsChecked == true)
                    {
                        MapControl.CenterMapOnPlayer();
                    }
                    CurrentPositionTextBox.Text = $"{e.XPosition} {normalizedXPosition} {e.ZPosition} {normalizedZPosition}";
                }
            });
        }

        private void ClearDrawingButton_OnClick(object sender, RoutedEventArgs e)
        {
            MapControl.ClearDrawing();
        }

        private void EnableDrawingCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            MapControl.DrawingEnabled = true;
        }

        private void EnableDrawingCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            MapControl.DrawingEnabled = false;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                MapControl.UndoLastDrawing();
            }
            else if (e.Key == Key.D)
            {
                EnableDrawingCheckbox.IsChecked = !EnableDrawingCheckbox.IsChecked;
            }
            else if (e.Key == Key.F)
            {
                FollowPlayerCheckbox.IsChecked = !FollowPlayerCheckbox.IsChecked;
            }
        }

        private void FollowPlayerCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            MapControl.CenterMapOnPlayer();
        }

        private void MapControl_OnMouseClickedOnCanvas(object? sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
            {
                return;
            }
            
            if (MapCreationModeCheckbox.IsChecked == true)
            {
                Point clickPosition = MapControl.GetMousePositionOnCanvas();
                MapControl.SetPlayerDotPosition(clickPosition.X, clickPosition.Y);

                if (FollowPlayerCheckbox.IsChecked == true)
                {
                    MapControl.CenterMapOnPlayer();
                }
            }
        }

        private void MapCreationModeCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            PositionDataGrid.Visibility = Visibility.Visible;
            SplitterColumnDefinition.Width = GridLength.Auto;
            MapCreationDataTextBoxColumnDefinition.Width = new GridLength(Width / 2);
            MapCreationSplitter.Visibility = Visibility.Visible;
            MapDataGrid.Visibility = Visibility.Visible;
            MapCreationButtonsStackPanel.Visibility = Visibility.Visible;
            MapControl.SetPlayerDotImage(PlayerDotType.Circle);
        }

        private void MapCreationModeCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PositionDataGrid.Visibility = Visibility.Collapsed;
            SplitterColumnDefinition.Width = new GridLength(0);
            MapCreationDataTextBoxColumnDefinition.Width = new GridLength(0);
            MapCreationSplitter.Visibility = Visibility.Collapsed;
            MapDataGrid.Visibility = Visibility.Collapsed;
            MapCreationButtonsStackPanel.Visibility = Visibility.Collapsed;
            MapControl.SetPlayerDotImage(PlayerDotType.CircleWithArrow);
        }

        private void StartCreatingMapButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddNewMapDialog addMapDialog = _addNewMapDialogFactory();
            addMapDialog.MapAdded += (_, args) =>
            {
                (bool Success, string? ErrorMessage) addNewMapResult = _mapCreationDataManager.AddNewMap(args.MapName, args.MapRotation, args.MapImagePath);

                if (!addNewMapResult.Success)
                {
                    this.ShowError(addNewMapResult.ErrorMessage ?? "Failed to add a new map");
                }
                
                MapControl.ReloadMaps();
                MapControl.SelectMap(args.MapName);
            };
            
            addMapDialog.ShowDialog();
        }

        private void ReloadMapCreationPositionData()
        {
            if (MapControl.SelectedMap != null)
            {
                MapCreationDataTextBox.Text = _mapCreationDataManager.GetMapPositionData(MapControl.SelectedMap);
            }
        }

        private void MapControl_OnMapSelectionChanged(object? sender, MapSelectionChangedEventArgs e)
        {
            ReloadMapCreationPositionData();
        }

        private void AddPositionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPositionTextBox.Text))
            {
                this.ShowError("Position data is not yet available. Start the game and begin a new round first");
            }

            if (!MapCreationDataTextBox.Text.EndsWith('\n'))
            {
                MapCreationDataTextBox.Text += '\n';
            }
            
            MapCreationDataTextBox.Text += $"{CurrentPositionTextBox.Text}\n";
            SaveMapPositionDataToFile();
        }

        private void SaveMapPositionDataToFile()
        {
            if (MapControl.SelectedMap != null)
            {
                _mapCreationDataManager.SaveMapPositionData(MapControl.SelectedMap, MapCreationDataTextBox.Text);
            }
        }

        private void ClearPositionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MapCreationDataTextBox.Text = null;
            SaveMapPositionDataToFile();
        }

        private void UpdateMapTransformsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MapControl.SelectedMap == null)
            {
                throw new InvalidOperationException("A map must first be selected before calculating the coefficients");
            }
            
            MapCoefficientsGenerationResult result = _mapCreationDataManager.MapCoefficientsGenerationResult(MapCreationDataTextBox.Text, PolynomialDegreeIntegerUpDown.Value ?? 1);

            if (!result.Success)
            {
                this.ShowError(result.ErrorMessage ?? "An error occured when generating coefficients");
                return;
            }

            if (result.MapCoefficients == null)
            {
                throw new InvalidOperationException("Received null coefficients");
            }
            
            MapCoefficients mapCoefficients = result.MapCoefficients;
            ShowMapCoefficientsPlot(mapCoefficients);

            var mapData = new MapData
            {
                MapName = MapControl.SelectedMap.MapName,
                MapRotation = MapControl.SelectedMap.MapRotation,
                MapImageFile = MapControl.SelectedMap.MapImageFile,
                XCoefficients = mapCoefficients.XCoefficients,
                ZCoefficients = mapCoefficients.ZCoefficients
            };
                
            _mapCreationDataManager.UpdateMapUsingMapData(mapData);
            MapControl.ReloadMaps();
            MapControl.SelectMap(mapData.MapName);
        }

        private void ShowMapCoefficientsPlot(MapCoefficients mapCoefficients)
        {
            var xPlotData = new PlotData
            {
                GameCoordinates = mapCoefficients.GameXPositionsArray,
                MapCoordinates = mapCoefficients.MapXPositionsArray,
                Coefficients = mapCoefficients.XCoefficients
            };

            var zPlotData = new PlotData
            {
                GameCoordinates = mapCoefficients.GameZPositionsArray,
                MapCoordinates = mapCoefficients.MapZPositionsArray,
                Coefficients = mapCoefficients.ZCoefficients
            };

            PlotWindow plotWindow = _plotWindowFactory();
            plotWindow.RenderPlot(xPlotData, zPlotData);
            plotWindow.ShowDialog();
        }
    }
}