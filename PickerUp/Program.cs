
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;
using System.Runtime.InteropServices;

// DANGOS.DEV
// REESCRITURA DEL PROGRAM.CS AUTOGENERADO DE WINUI3 PARA PERMITIR COMPILAR EN SINGLE EXE

namespace PickerUp;
internal class Program {
    private static IntPtr _redirectEventHandle = IntPtr.Zero;

    [STAThread] static void Main(string[] args) {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        if (!HandleRedirection()){
            StartApplication();
        }
    }

    private static void StartApplication() {
        Microsoft.UI.Xaml.Application.Start(
            (p) => {
                SetSynchronizationContext();
                _ = new App();
            }
        );
    }

    private static bool HandleRedirection() {
        AppActivationArguments? args = AppInstance.GetCurrent().GetActivatedEventArgs();
        try{
            AppInstance? keyInstance = AppInstance.FindOrRegisterForKey("randomKey");
            if (keyInstance.IsCurrent){
                keyInstance.Activated += OnActivated;

                return false;// No redirection
            }

            // Redirecciona la activación a la instancia ya registrada
            RedirectActivationTo(args, keyInstance);

            return true;
        }
        catch (Exception ex){
            Console.Error.WriteLine($"Error during redirection: {ex.Message}");

            return false;
        }
    }

    private static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance) {
        _redirectEventHandle = CreateEvent(
            IntPtr.Zero,
            true,
            false,
            ""
        );

        // Ejecuta la redireccion en otro hilo
        Task.Run(
            () => {
                try{
                    keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                    SetEvent(_redirectEventHandle);
                }
                catch (Exception ex){
                    Console.Error.WriteLine($"Redirection failed: {ex.Message}");
                }
            }
        );

        WaitForRedirection();
    }

    private static void WaitForRedirection() {
        CoWaitForMultipleObjects(
            0,
            0xFFFFFFFF,
            1,
            [_redirectEventHandle],
            out _
        );
    }

    private static void SetSynchronizationContext() {
        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
    }

    private static void OnActivated(object? sender, AppActivationArguments args) {
        Debug.WriteLine($"App activated with kind: {args.Kind}");
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private extern static IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll")] private extern static bool SetEvent(IntPtr hEvent);

    [DllImport("ole32.dll")] private extern static uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);
}
