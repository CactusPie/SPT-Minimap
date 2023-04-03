using System.Windows;

namespace CactusPie.MapLocation.Minimap.Helpers;

public static class MessageBoxHelper
{
    public static void ShowError(this Window window, string message)
    {
        MessageBox.Show(window, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}