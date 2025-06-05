using System.Collections.Generic;
using System.Threading.Tasks;
namespace Naderite.UI_Window_Manager.Window_Flow_Strategies
{
    public interface IWindowFlowStrategy
    {
        List<IWindow> AllWindows { get; }
        Task OpenWindow(IWindow window, bool animated = true,bool isReversedAnimation = false);
        Task CloseCurrentWindow(bool animated = true);
        void CloseAllInstantly();
        IWindow CurrentWindow { get; }
        int ActiveWindowsCount { get; }
        public Task NextWindow(bool animated = true);

        public Task PreviousWindow(bool animated = true);
    }
}