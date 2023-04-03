using System;
using System.IO;
using System.Windows;
using CactusPie.MapLocation.Minimap.Events;
using CactusPie.MapLocation.Minimap.Helpers;
using Microsoft.Win32;

namespace CactusPie.MapLocation.Minimap;

public partial class AddNewMapDialog : Window
{
    public event EventHandler<MapAddedEventArgs>? MapAdded;
    
    public AddNewMapDialog()
    {
        InitializeComponent();
    }

    private void AddMapImageButton_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +  
                     "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +  
                     "Portable Network Graphic (*.png)|*.png",
            Multiselect = false
        };

        if(openFileDialog.ShowDialog() == true)
        {
            MapImagePathTextBox.Text = openFileDialog.FileName;
        }
    }

    private void AddMapButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(MapNameTextBox.Text))
        {
            this.ShowError("You must provide a map name");
            return;
        }
        
        if (string.IsNullOrEmpty(MapImagePathTextBox.Text) || !File.Exists(MapImagePathTextBox.Text))
        {
            this.ShowError("You must provide a valid map file path");
            return;
        }
        
        if (MapRotationIntegerUpDown.Value == null)
        {
            this.ShowError("You must provide a valid map rotation");
            return;
        }

        var eventArgs = new MapAddedEventArgs(MapNameTextBox.Text, MapImagePathTextBox.Text, MapRotationIntegerUpDown.Value.Value);
        EventHandler<MapAddedEventArgs>? handler = MapAdded;
        handler?.Invoke(this, eventArgs);
        Close();
    }
}