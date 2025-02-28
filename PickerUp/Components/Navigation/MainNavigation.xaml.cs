using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using PickerUp.Components.Pages;
using System;
using System.Collections.Generic;
using WatcherPage=PickerUp.Components.Pages.WatcherPage;

namespace PickerUp.Components.Navigation;

public sealed partial class MainNavigation : UserControl {
    public MainNavigation() {
        InitializeComponent();
    }

    private void SegmentedControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        var navigation = (Segmented) sender;
        var selectedItem = (SegmentedItem) navigation.SelectedItem;

        if (MainWindow.MainFrame == null) return;

        switch (selectedItem.Tag){
            case "watcher":
                MainWindow.MainFrame.Navigate(typeof(WatcherPage));
                break;

            case "palettes":
                MainWindow.MainFrame.Navigate(typeof(PalettesPage));
                break;

            case "settings":
                MainWindow.MainFrame.Navigate(typeof(SettingsPage));
                break;
        }

    }
}
