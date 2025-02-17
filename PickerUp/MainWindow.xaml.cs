using PickerUp.Source.Colors;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using Windows.Graphics;
using WinRT;
using PickerUp;
using PickerUp.Components.Navigation;
using PickerUp.Source;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using KeyModifiers=Vanara.PInvoke.User32.HotKeyModifiers;
using Keys=Vanara.PInvoke.User32.VK;
using WatcherPage=PickerUp.Components.Pages.WatcherPage;

namespace PickerUp;

public sealed partial class MainWindow : Window {

    private WindowsSystemDispatcherQueueHelper? _windowsSystemDispatcher;
    private DesktopAcrylicController? _acrylicController;
    private SystemBackdropConfiguration? _backdropConfiguration;
    private User32.WindowProc _originalProcess;
    private User32.WindowProc _hotKeyHandler;
    private static HWND Handle { get; set; }
    public static AppWindow _appWindow { get; set; }
    public static Frame MainFrame { get; set; }
    
    public readonly static string AppLocalFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Dango",
        "PickerUp"
    );

    /// <summary>
    /// Punto de inicio, creación de la ventana
    /// </summary>
    public MainWindow() {
        InitializeComponent();

        EnsureAppLocalFolderExistsAsync();
        Settings.LoadSettingsFromFileAsync();
        
        InitializeWindow();
        RegisterEvents();

        // ApplicationData.Current
        InitializeHotKeys();
        
        MainFrame = _mainFrame;
        _mainFrame.Navigate(typeof(WatcherPage));
    }

    private void InitializeWindow() {
        Handle = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(this).ToInt32());
        WindowId windowId = Win32Interop.GetWindowIdFromWindow((IntPtr) Handle);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        _appWindow.SetIcon("/Assets/logo.ico");

        _appWindow.SetPresenter(CreateCustomPresenter());

        string? sExe = Environment.ProcessPath;
        var ico = System.Drawing.Icon.ExtractAssociatedIcon(sExe);

        User32.SendMessage(Handle, 0x0080, 0, ico.Handle);
        User32.SendMessage(Handle, 0x0080, 1, ico.Handle);
        User32.SendMessage(Handle, 0x0080, 2, ico.Handle);
        SetTitleBar(_titleBar);
        
         _ = TrySetAcrylicBackdrop();

        _appWindow.Resize(new SizeInt32(_Width: 300, _Height: 420));
    }

    private void InitializeHotKeys() {
        _hotKeyHandler = HotKeyHandler;
        _originalProcess = GetDelegateForHotkeyHandler();

        User32.RegisterHotKey(Handle, 0xBFFF, KeyModifiers.MOD_ALT, (uint) Keys.VK_D);
    }

    public static HWND GetWindowHandle() {
        return Handle;
    }

    private User32.WindowProc GetDelegateForHotkeyHandler() {
        nint hotKeyPrcPointer = Marshal.GetFunctionPointerForDelegate(_hotKeyHandler);

        return Marshal.GetDelegateForFunctionPointer<User32.WindowProc>(
            PInvoke.User32.SetWindowLongPtr(
                (IntPtr) Handle,
                PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC,
                hotKeyPrcPointer
            )
        );
    }

    private void RegisterEvents() {
        this.Activated += OnActivated;
        _appWindow.Closing += OnClosing;
        this.Closed += OnClosed;
        ((FrameworkElement) Content).ActualThemeChanged += OnThemeChanged;
    }

    private OverlappedPresenter CreateCustomPresenter() {
        var customPresenter = OverlappedPresenter.CreateForDialog();
        customPresenter.IsMaximizable = false;
        customPresenter.IsResizable = false;
        customPresenter.IsAlwaysOnTop = Settings.AlwaysOnTop; // Valor desde las configuraciones
        ExtendsContentIntoTitleBar = true;
        _appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

        return customPresenter;
    }

    /// <summary>
    /// Procesos que se ejecutan cuando se solicita cerrar todo
    /// </summary>
    private async void OnClosing(object sender, AppWindowClosingEventArgs e) {
        Debug.WriteLine("OnClosing");
        e.Cancel = true;

        Close();
    }

    /// <summary>
    /// Evento de cierre definitivo del programa
    /// </summary>
    private void OnClosed(object sender, WindowEventArgs e) {
        Debug.WriteLine("OnClosed");

        if (_acrylicController is not null){
            _acrylicController.Dispose();
            _acrylicController = null;
        }

        this.Activated -= OnActivated;
        _backdropConfiguration = null;
    }

    public static void SetAlwaysOnTop(bool state = true) {
        ((OverlappedPresenter) _appWindow.Presenter).IsAlwaysOnTop = state;
    }
    private bool TrySetAcrylicBackdrop() {
        if (DesktopAcrylicController.IsSupported() is false) return false;// Acrylic is not supported on this system

        _windowsSystemDispatcher = new WindowsSystemDispatcherQueueHelper();
        // _windowsSystemDispatcher.EnsureWindowsSystemDispatcherQueueController();

        // Hooking up the policy object
        _backdropConfiguration = new SystemBackdropConfiguration {
            IsInputActive = true
        };

        SetConfigurationSourceTheme();

        _acrylicController = new DesktopAcrylicController();
        _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);

        return true;// succeeded

    }

    private void OnActivated(object sender, WindowActivatedEventArgs args) {
        if (_backdropConfiguration is null) return;
        _backdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        
        SetAlwaysOnTop(Settings.AlwaysOnTop);
    }

    private void OnThemeChanged(FrameworkElement sender, object args) {
        if (_backdropConfiguration is null) return;
        SetConfigurationSourceTheme();
    }

    private void SetConfigurationSourceTheme() {
        if (_backdropConfiguration is null) return;

        _backdropConfiguration.Theme = ((FrameworkElement) Content).ActualTheme switch {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Default => SystemBackdropTheme.Default,
            _ => _backdropConfiguration.Theme
        };
    }

    /// <summary>
    /// Maneja los hotkeys
    /// </summary>
    private IntPtr HotKeyHandler(HWND hwnd, uint uMsg, IntPtr wParam, IntPtr lParam) {

        IntPtr toOriginalCaller = User32.CallWindowProc(_originalProcess, hwnd, uMsg, wParam, lParam);

        if (uMsg != 0x0312){
            // Si el evento no es un atajo registrado lo devuelve por donde mismo vino
            return toOriginalCaller;
        }

        var keyModifiers = (KeyModifiers) ((int) lParam & 0xFFFF);// Tecla modificadora (ALT, CTRL, etc.)
        var keyPressed = (Keys) ((int) lParam >> 16 & 0xFFFF);// Tecla presionada

        switch (keyModifiers){
            case KeyModifiers.MOD_ALT when keyPressed == Keys.VK_D:// ALT + D
                WatcherPage.PickColor();

                return IntPtr.Zero;

            default:// Si no es ningun atajo registrado lo devuelve al caller
                return toOriginalCaller;
        }
    }

    private static void EnsureAppLocalFolderExistsAsync() {
        if (!Directory.Exists(AppLocalFolder)){
            Directory.CreateDirectory(AppLocalFolder);
        }
    }

}
