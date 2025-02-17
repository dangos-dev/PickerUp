using System.Runtime.InteropServices;

namespace PickerUp;

internal class WindowsSystemDispatcherQueueHelper {
    private object? _dispatcherQueueController = null;

    public void EnsureWindowsSystemDispatcherQueueController() {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            // Uno ya existe... asi que solo usa ese
            return;
        }

        if (_dispatcherQueueController != null) return;

        DispatcherQueueOptions options;
        options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
        options.threadType = 2;// DQTYPE_THREAD_CURRENT
        options.apartmentType = 2;// DQTAT_COM_STA

        CreateDispatcherQueueController(options, ref _dispatcherQueueController);
    }

    [DllImport("CoreMessaging.dll")] private extern static int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }
}
