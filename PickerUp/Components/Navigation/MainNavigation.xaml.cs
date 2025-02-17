using CommunityToolkit.WinUI.Controls;
using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using PickerUp.Components.Pages;
using PickerUp.Source.Watcher;
using WatcherPage=PickerUp.Components.Pages.WatcherPage;

namespace PickerUp {
    public sealed partial class MainNavigation : UserControl {
        private Dictionary<Type, Page> _cachedPages = new();
        public MainNavigation() {
            this.InitializeComponent();
        }
        private void SegmentedControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var navigation = (Segmented) sender;
            var selectedItem = (SegmentedItem) navigation.SelectedItem;

            if (MainWindow.MainFrame == null) return;

            // string navItemTag = selectedItem.Tag.ToString();
            // Type pageType = Type.GetType(navItemTag);
            //
            // if (pageType != null){
            //     NavigateToPage(pageType);
            // }

            switch (selectedItem.Tag){
                case "watcher":
                    MainWindow.MainFrame.Navigate(typeof(WatcherPage));

                    break;

                case "settings":
                    MainWindow.MainFrame.Navigate(typeof(SettingsPage));

                    break;

                default:
                    break;
            }

        }
        private void NavigateToPage(Type pageType) {
            if (MainWindow.MainFrame.Content.GetType() == pageType){
                return;
            }

            Page page = (Page) Activator.CreateInstance(pageType)!;
            MainWindow.MainFrame.Content = page;
        }
    }
}
