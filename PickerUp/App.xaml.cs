using Microsoft.UI.Xaml;

namespace PickerUp;

public partial class App {
    private Window? _mWindow;

    public App() {
        InitializeComponent();
        // try{
        //     Updater.CheckUpdate();
        // }
        // catch{
        //     Debug.WriteLine("Failed to check for updates");
        // }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        _mWindow = new MainWindow();
        _mWindow.Activate();

    }


}
