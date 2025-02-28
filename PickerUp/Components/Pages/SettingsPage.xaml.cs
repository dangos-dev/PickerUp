using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PickerUp.Source;
using System;

namespace PickerUp.Components.Pages;

public sealed partial class SettingsPage {

    public SettingsPage() {
        InitializeComponent();

        DispatcherQueue.TryEnqueue(
            () => {
                _toggleAlwaysOnTop.IsOn = Settings.AlwaysOnTop;
                _toggleAutoCopy.IsOn = Settings.AutoCopy;
                _toggleShowAlpha.IsOn = Settings.ShowAlpha;
            }
        );
    }

    private void AlwaysOnTop_Toggled(object sender, RoutedEventArgs e) {
        Settings.AlwaysOnTop = ((ToggleSwitch) sender).IsOn;
        MainWindow.SetAlwaysOnTop(Settings.AlwaysOnTop);
    }

    private void AutoCopy_Toggled(object sender, RoutedEventArgs e) {
        Settings.AutoCopy = ((ToggleSwitch) sender).IsOn;
    }

    private void ShowAlpha_Toggled(object sender, RoutedEventArgs e) {
        Settings.ShowAlpha = ((ToggleSwitch) sender).IsOn;
    }
    private async void GoToWebpage(object sender, RoutedEventArgs e) {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://dangos.dev/pickerup.html"));
    }

    private async void VisitDango(object sender, RoutedEventArgs e) {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dangos-dev"));
    }
}
