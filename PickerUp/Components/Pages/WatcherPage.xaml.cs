using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PickerUp.Source;
using PickerUp.Source.Colors;
using PickerUp.Source.EditorDefinition;
using PickerUp.Source.HistoryDefinition;
using PickerUp.Source.PalettesDefinition;
using PickerUp.Source.Watcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Point=System.Drawing.Point;

namespace PickerUp.Components.Pages;

public sealed partial class WatcherPage {
    // private static ArgbColor _focusedColor = new();
    private bool _isPaused = false;

    public Task MainTask { get; set; }
    private readonly CancellationTokenSource _mainTaskCancelation = new();

    private readonly SolidColorBrush _colorBrush = new();
    private readonly SolidColorBrush _contrastingColorBrush = new();

    private readonly SolidColorBrush _pickedColorBrush = new();
    private readonly SolidColorBrush _pickedContrastingColorBrush = new();

    private bool _isFirstLoad = true;

    public WatcherPage() {
        InitializeComponent();
        InitializeListColorFormats();

    }

    private void RunMainProcess() {
        MainTask = Task.Run(
            () => {
                while (true){
                    try{
                        if (_mainTaskCancelation.Token.IsCancellationRequested){
                            Debug.WriteLine("Cancellation requested for (WatcherPage.MainTask). Exiting task...");

                            break;
                        }

                        Source.Watcher.Watcher.WatchPixel();

                        // Si esta pausado, se sobreescribe el color actual por el pickeado
                        if (_isPaused) Source.Watcher.Watcher.SetColor(Editor.GetFocused());

                        (IColor color, Point position, string colorString, string colorFormat) = GetWatchedData();
                        (IColor focusedColor, string focusedColorString) = GetFocusedData();

                        if (DispatcherQueue.HasThreadAccess){
                            UpdateUi(color, position, colorString, colorFormat, focusedColor, focusedColorString);
                        }
                        else{
                            DispatcherQueue?.EnqueueAsync(
                                () => {
                                    UpdateUi(color, position, colorString, colorFormat, focusedColor, focusedColorString);
                                },
                                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal
                            );
                        }
                    }
                    catch (Exception e){
                        Debug.WriteLine(e);
                    }

                }
            },
            _mainTaskCancelation.Token
        );
    }

    private void StopMainTask() {
        _mainTaskCancelation.Cancel();
    }

    private void DisposeMainTask() {
        _mainTaskCancelation.Dispose();
    }

    private static (IColor watchedColor, Point watchedPixel, string watchedColorString, string watchedColorFormat) GetWatchedData() {
        IColor watchedColor = Source.Watcher.Watcher.GetColor();
        Point watchedPixel = Source.Watcher.Watcher.GetPosition();
        string watchedColorString = watchedColor.ToString();
        string watchedColorFormat = Source.Watcher.Watcher.GetFormat().ToUpper();

        return (watchedColor, watchedPixel, watchedColorString, watchedColorFormat);
    }

    private static (IColor focusedColor, string focusedColorString) GetFocusedData() {
        IColor focusedColor = Editor.GetFocused();
        string focusedColorString = focusedColor.ToString();

        return (focusedColor, focusedColorString);
    }

    private void UpdateUi(IColor watchedColor, Point watchedPixel, string watchedColorString, string watchedColorFormat, IColor focusedColor, string focusedColorString) {
        try{

            UpdateFocused(focusedColor, focusedColorString);
            UpdateColor(watchedColor, watchedColorString, watchedColorFormat);
            UpdateHistory();
            UpdatePalette();

            BitmapImage bitmapImage = new();

            using MemoryStream stream = new();
            Source.Watcher.Watcher.Preview.Save(stream, ImageFormat.Png);
            Source.Watcher.Watcher.Preview.Dispose();
            stream.Position = 0;
            bitmapImage.SetSource(stream.AsRandomAccessStream());
            _previewCanvas.ImageSource = bitmapImage;

            _labelMousePos.Text = $"({watchedPixel.X}, {watchedPixel.Y})";

            _isFirstLoad = false;
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private void UpdateColor(IColor watchedColor, string watchedColorString, string watchedColorFormat) {
        if (!Source.Watcher.Watcher.IsColorChanged && !_isFirstLoad)
            return;

        _colorBrush.Color = watchedColor.ToWinColor();
        _contrastingColorBrush.Color = GetContrastingColor(watchedColor).ToWinColor();

        _panelColor.Background = _colorBrush;
        _labelColorCode.Foreground = _contrastingColorBrush;

        _listColorFormats.Foreground = _contrastingColorBrush;

        Grid listColorFormatsGridBase = (Grid) VisualTreeHelper.GetChild(_listColorFormats, 0);
        ((AnimatedIcon) VisualTreeHelper.GetChild(listColorFormatsGridBase, 6)).Foreground = _contrastingColorBrush;

        _labelMousePos.Foreground = _contrastingColorBrush;
        _buttonPauseWatcherIcon.Foreground = _contrastingColorBrush;

        _labelColorCode.Text = watchedColorString;
        // _labelColorFormat.Text = watchedColorFormat;
    }

    private void UpdateFocused(IColor focusedColor, string focusedColorString) {
        if (!Editor.IsColorChanged && !_isFirstLoad)
            return;

        _pickedColorBrush.Color = focusedColor.ToWinColor();
        _pickedContrastingColorBrush.Color = GetContrastingColor(focusedColor).ToWinColor();

        _textColorCode.Text = focusedColorString;
        _textColorCode.Background = _pickedColorBrush;
        _textColorCode.Foreground = _pickedContrastingColorBrush;

        // ColorPickerSlider.Color = focusedColor.ToWinColor();
        // ColorPickerSlider2.Color = focusedColor.ToWinColor();
        // ColorPickerSlider4.Color = focusedColor.ToWinColor();
        // ColorPickerSlider5.Color = focusedColor.ToWinColor();
        // colorPreviewer.ShowAccentColors = true;
        // colorPreviewer.HsvColor = new HsvColor { H = 20, S = 46, V = 46, A = 255 };
        // colorPreviewer.Background = new SolidColorBrush( focusedColor.ToWinColor());
        // colorPreviewer.UpdateLayout();
    }

    private void UpdateHistory() {
        if (!Editor.IsColorChanged && !_isFirstLoad)
            return;

        var colorHistories = new[] {
            _colorHistory1, _colorHistory2, _colorHistory3,
            _colorHistory4, _colorHistory5, _colorHistory6
        };

        for (int i = 0; i < History.Colors.Length; i++){
            colorHistories[i].Background = History.Colors[i].ToBrush();
            ToolTipService.SetToolTip(colorHistories[i], History.Colors[i].ToString());
        }
    }

    private void UpdatePalette() {
        if (!Editor.IsColorChanged && !_isFirstLoad)
            return;

        var colorPalettes = new[] {
            _colorPalette1, _colorPalette2, _colorPalette3, _colorPalette4, _colorPalette5,
            _colorPalette6, _colorPalette7, _colorPalette8, _colorPalette9, _colorPalette10
        };

        for (int i = 0; i < SaturationPalette.Colors.Length; i++){
            colorPalettes[i].Background = SaturationPalette.Colors[i].ToBrush();
            ToolTipService.SetToolTip(colorPalettes[i], SaturationPalette.Colors[i].ToString());
        }
    }


    private void InitializeListColorFormats() {
        var formatOptions = new List<KeyValuePair<string, string>>() {
            new("rgb", "RGB"),
            new("hex", "HEX"),
            // new("argb", "ARGB"),
            // new("hsv", "HSV"),
            // new("hsl", "HSL")
        };

        _listColorFormats.SelectionChanged += ListColorFormats_SelectionChanged;
        _listColorFormats.ItemsSource = formatOptions;
        _listColorFormats.DisplayMemberPath = "Value";
        _listColorFormats.SelectedValuePath = "Key";
        _listColorFormats.SelectedValue = "rgb";
    }

    private static void ListColorFormats_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        if (e.AddedItems is null || e.AddedItems.Count == 0)
            return;

        string selectedFormat = ((KeyValuePair<string, string>) e.AddedItems[0]).Value;
        Source.Watcher.Watcher.SetFormat(selectedFormat);

    }

    private static IColor GetContrastingColor(IColorFormat backgroundColor) {
        // Función para determinar el color de contraste basado en la luminosidad
        return IsDarkColor(backgroundColor) ? new IColor(255, 255, 255) : new IColor(0, 0, 0);
    }

    public static void PickColor(IColor? desiredColor = null) {
        if (desiredColor != null){
            Editor.FocusColor(desiredColor);
            Source.Watcher.Watcher.SetColor(Editor.GetFocused());
        }

        (IColor watchedColor, _) = Source.Watcher.Watcher.WatchPixel();
        Editor.FocusColor(watchedColor);

        Watcher.IsColorChanged = true;
        Editor.IsColorChanged = true;

        if (Settings.AutoCopy){
            CopyColorToClipboard(watchedColor);
        }
    }

    private static void CopyColorToClipboard(IColor colorToCopy) {
        var dataPackage = new DataPackage();
        dataPackage.SetText(colorToCopy.ToString());
        Clipboard.SetContent(dataPackage);
        Clipboard.Flush();
    }


    private static bool IsDarkColor(IColorFormat color) {
        // Función para determinar si un color es oscuro
        var brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;

        return brightness < 128;
    }

    private void WatcherPage_OnKeyDown(object sender, KeyRoutedEventArgs e) {
        Debug.WriteLine("aa");
    }

    private void _buttonPauseWatcher_OnClick(object sender, RoutedEventArgs e) {
        _isPaused = !_isPaused;
        _buttonPauseWatcherIcon.Glyph = (_isPaused) ? "\uF5B0" : "\uF8AE";

        // SetFocusedColor();

    }
    private void WatcherPage_OnLoaded(object sender, RoutedEventArgs e) {
        // _mainTaskCancelation.TryReset();
        RunMainProcess();
        // DispatcherQueue.TryEnqueue(() => {
        //     var contentPresenter = VisualTreeHelper.GetParent(this.Content);
        //     var layoutRoot = VisualTreeHelper.GetParent(contentPresenter);
        //     var titleBar = VisualTreeHelper.GetChild(layoutRoot, 1) as Grid;
        //     var buttonContainer = VisualTreeHelper.GetChild(titleBar, 0) as Grid;
        //     var closeButton = VisualTreeHelper.GetChild(buttonContainer, 2) as Button;
        //     if (closeButton != null){
        //         closeButton.Visibility = Visibility.Collapsed;//Hides the button.
        //     }
        //     // VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(layoutRoot)), 1)
        // });
    }
    private void WatcherPage_OnUnloaded(object sender, RoutedEventArgs e) {
        StopMainTask();
        _listColorFormats.SelectionChanged -= ListColorFormats_SelectionChanged;
        // DisposeMainTask();
    }
    private void CopyToClipboardButton_OnClick(object sender, RoutedEventArgs e) {
        CopyColorToClipboard(Editor.GetFocused());
    }
}
