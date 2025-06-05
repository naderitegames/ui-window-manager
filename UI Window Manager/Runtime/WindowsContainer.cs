using System.Threading.Tasks;
using Naderite.Simple_UIManager.Custom_Collections;
using Naderite.UI_Window_Manager.Window_Flow_Strategies;
using UnityEngine;

namespace Naderite.UI_Window_Manager
{
    public sealed class WindowsContainer
    {
        private readonly RegisteredWindowCollection _registeredWindows;
        private readonly IWindowFlowStrategy _flowStrategy;

        public IWindow CurrentWindow => _flowStrategy.CurrentWindow;
        public int ActiveWindowsCount => _flowStrategy.ActiveWindowsCount;

        public WindowsContainer(IWindowFlowStrategy flowStrategy)
        {
            _registeredWindows = new RegisteredWindowCollection();
            _flowStrategy = flowStrategy;
        }

        public void RegisterWindow(IWindow window)
        {
            _registeredWindows.Register(window);
            _flowStrategy.AllWindows.Add(window);
        }

        public async Task OpenWindow(IWindow window, bool animated = true)
        {
            await _flowStrategy.OpenWindow(window, animated);
        }

        public async Task OpenWindow(string windowName, bool animated = true)
        {
            var window = _registeredWindows.GetWindow(windowName);
            if (window != null)
            {
                await _flowStrategy.OpenWindow(window, animated);
            }
            else
            {
                Debug.LogWarning($"Window with name '{windowName}' not found in registered windows.");
            }
        }

        public async Task CloseCurrentWindow(bool animated = true)
        {
            await _flowStrategy.CloseCurrentWindow(animated);
        }

        public void CloseAllWindowsInstantly()
        {
            _flowStrategy.CloseAllInstantly();
        }

        public void OpenNextWindow()
        {
            _flowStrategy.NextWindow();
        }
        public void OpenPreviousWindow()
        {
            _flowStrategy.PreviousWindow();
        }
    }
}