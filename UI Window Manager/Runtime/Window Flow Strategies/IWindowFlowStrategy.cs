using System.Collections.Generic;
using System.Threading.Tasks;
namespace Naderite.UIWindowManager.Window_Flow_Strategies
{
    public interface IWindowFlowStrategy
    {
        List<IWindow> AllWindows { get; }
        bool AllowRepeatWindows { get; set; }
        Task OpenWindow(IWindow window, bool animated = true, bool isReversedAnimation = false);
        Task CloseCurrentWindow(bool animated = true);
        void CloseAllInstantly();
        IWindow CurrentWindow { get; }
        int ActiveWindowsCount { get; }
        Task NextWindow(bool animated = true);
        Task PreviousWindow(bool animated = true);
    }
}