using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PickerUp.Source;
using PickerUp.Source.Colors;
using PickerUp.Source.Palettes;
using PickerUp.Source.Watcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using BitmapImage=Microsoft.UI.Xaml.Media.Imaging.BitmapImage;
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

        // _historyPalette.Columns = History.Colors.Length;
        // _colorPalette.Columns = SaturationPalette.Colors.Length;

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

                        Watcher.WatchPixel();

                        // Si esta pausado, se sobreescribe el color actual por el pickeado
                        if (_isPaused) Watcher.SetColor(ColorPicked.Get());

                        (IColor color, Point position, string colorString, string colorFormat) = GetWatchedData();
                        (IColor focusedColor, string focusedColorString) = GetColorPickedData();

                        if (DispatcherQueue.HasThreadAccess){
                            UpdateUi(color, position, colorString, colorFormat, focusedColor, focusedColorString);
                            Watcher.IsColorChanged = false;
                            ColorPicked.HasChanged = false;
                        }
                        else{
                            DispatcherQueue?.EnqueueAsync(
                                () => {
                                    UpdateUi(color, position, colorString, colorFormat, focusedColor, focusedColorString);

                                    Watcher.IsColorChanged = false;
                                    ColorPicked.HasChanged = false;
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

    private static (IColor colorPicked, string colorPickedString) GetColorPickedData() {
        IColor _colorPicked = ColorPicked.Get();
        string _colorPickedString = _colorPicked.ToString();

        return (_colorPicked, _colorPickedString);
    }

    private void UpdateUi(IColor watchedColor, Point watchedPixel, string watchedColorString, string watchedColorFormat, IColor focusedColor, string focusedColorString) {
        try{

            UpdateFocused(focusedColor, focusedColorString);
            UpdateColor(watchedColor, watchedColorString, watchedColorFormat);
            UpdateHistory();
            UpdatePalette();
            SetPreviewCanvasImage();

            _labelMousePos.Text = $"({watchedPixel.X}, {watchedPixel.Y})";

            _isFirstLoad = false;
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private void UpdateColor(IColor watchedColor, string watchedColorString, string watchedColorFormat) {
        if (!Watcher.IsColorChanged && !_isFirstLoad)
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
        if (!ColorPicked.HasChanged && !_isFirstLoad)
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
        if (!ColorPicked.HasChanged && !_isFirstLoad)
            return;

        IColor[] recentColors = History.Colors.Take(10).ToArray();

        // History.Colors.CopyTo(_colorPalette.Colors, 0);
        _historyPalette.Columns = recentColors.Length;

        _historyPalette.Colors = [];
        _historyPalette.Colors = recentColors;
    }

    private void UpdatePalette() {
        if (!ColorPicked.HasChanged && !_isFirstLoad)
            return;

        _colorPalette.Columns = 10;
        _colorPalette.Colors = [];
        _colorPalette.Colors = ShadesPalette.FromColor(ColorPicked.Get(), 10);
    }

    private void SetPreviewCanvasImage() {
        BitmapImage bitmapImage = new();

        using MemoryStream stream = new();
        Watcher.Preview.Save(stream, ImageFormat.Png);
        Watcher.Preview.Dispose();
        stream.Position = 0;

        bitmapImage.SetSource(stream.AsRandomAccessStream());
        _previewCanvas.ImageSource = bitmapImage;
    }

    private void InitializeListColorFormats() {
        var formatOptions = new List<KeyValuePair<string, string>>() {
            new("rgb", "RGB"),
            new("hex", "HEX"),
            // new("argb", "ARGB"),
            // new("hsv", "HSV"),
            new("hsl", "HSL")
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
            ColorPicked.Pick(desiredColor);
            Watcher.SetColor(ColorPicked.GetAndCheck());
        }

        (IColor watchedColor, _) = Watcher.WatchPixel();
        ColorPicked.Pick(watchedColor);

        Watcher.IsColorChanged = true;
        ColorPicked.HasChanged = true;

        if (Settings.AutoCopy){
            CopyColorToClipboard(watchedColor);
        }

        SessionManager.SaveSession();
    }

    private static void CopyColorToClipboard(IColor colorToCopy) {
        try{
            var dataPackage = new DataPackage();
            dataPackage.SetText(colorToCopy.ToString());
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }

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
        RunMainProcess();
    }

    private void WatcherPage_OnUnloaded(object sender, RoutedEventArgs e) {
        StopMainTask();
        _listColorFormats.SelectionChanged -= ListColorFormats_SelectionChanged;
        // DisposeMainTask();
    }

    private void CopyToClipboardButton_OnClick(object sender, RoutedEventArgs e) {
        // ToggleThemeTeachingTip1.IsOpen = true;
        CopyColorToClipboard(ColorPicked.Get());
    }
}
