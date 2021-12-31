using System.Diagnostics.CodeAnalysis;
using Windows.System;
using Windows.UI.Core;

namespace Ghostly.Uwp
{
    internal static class CoreWindowExtensions
    {
        public static bool IsCtrlKeyPressed(this CoreWindow window)
        {
            try
            {
                var ctrlState = window.GetKeyState(VirtualKey.Control);
                return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            }
            catch
            {
                return false;
            }
        }
    }
}
