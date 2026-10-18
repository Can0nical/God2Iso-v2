using System;
using System.Runtime.InteropServices;

namespace God2Iso
{
    /// <summary>
    /// Minimal replacement for the long-dead Microsoft.WindowsAPICodePack.
    /// Talks directly to Shell's ITaskbarList3 via COM for Windows 7+ progress.
    /// </summary>
    internal static class TaskbarProgress
    {
        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig] void HrInit();
            [PreserveSig] void AddTab(IntPtr hwnd);
            [PreserveSig] void DeleteTab(IntPtr hwnd);
            [PreserveSig] void ActivateTab(IntPtr hwnd);
            [PreserveSig] void SetActiveAlt(IntPtr hwnd);
            // ITaskbarList2
            [PreserveSig] void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);
            // ITaskbarList3
            [PreserveSig] void SetProgressValue(IntPtr hwnd, ulong completed, ulong total);
            [PreserveSig] void SetProgressState(IntPtr hwnd, TaskbarStates state);
        }

        [ComImport]
        [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class CTaskbarList { }

        [Flags]
        private enum TaskbarStates : uint
        {
            NoProgress = 0, Indeterminate = 0x1, Normal = 0x2, Error = 0x4, Paused = 0x8,
        }

        private static ITaskbarList3? _tb;
        private static bool _tried;

        private static ITaskbarList3? GetInstance()
        {
            if (_tb is not null || _tried) return _tb;
            _tried = true;
            try
            {
                _tb = (ITaskbarList3)new CTaskbarList();
                _tb.HrInit();
            }
            catch { _tb = null; }
            return _tb;
        }

        public static void SetProgress(IntPtr hwnd, int current, int max)
        {
            var tb = GetInstance();
            if (tb is null || max <= 0) return;
            try
            {
                tb.SetProgressState(hwnd, TaskbarStates.Normal);
                tb.SetProgressValue(hwnd, (ulong)current, (ulong)max);
            }
            catch { }
        }

        public static void ClearProgress(IntPtr hwnd)
        {
            try { GetInstance()?.SetProgressState(hwnd, TaskbarStates.NoProgress); } catch { }
        }
    }
}
