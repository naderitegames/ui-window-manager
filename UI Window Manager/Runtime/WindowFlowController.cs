using System.Collections.Generic;
using System.Threading.Tasks;
using Naderite.Simple_UIManager.Custom_Collections;
using Naderite.UI_Window_Manager;
using Naderite.UI_Window_Manager.Window_Flow_Strategies;
using UnityEngine;

namespace Naderite.Simple_UIManager
{
    public class WindowFlowController : IWindowFlowStrategy
    {
        private readonly ActiveWindowStack _activeStack;
        private bool _isAnimationInProgress = false;

        public IWindow CurrentWindow => _activeStack.CurrentWindow;
        public int ActiveWindowsCount => _activeStack.Count;
        
        
        
        
        public Task NextWindow(bool animated = true)
        {
            throw new System.NotImplementedException();
        }

        public Task PreviousWindow(bool animated = true)
        {
            throw new System.NotImplementedException();
        }

        public WindowFlowController(ActiveWindowStack activeStack)
        {
            _activeStack = activeStack;
        }

        public List<IWindow> AllWindows { get; }
        
        
        
        
        

        public async Task OpenWindow(IWindow windowToOpen, bool animated = true,bool isReversedAnimation= false)
        {
            if (windowToOpen == null || windowToOpen.IsOpen) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning($"Cannot open window '{windowToOpen.WindowName}' while another animation is in progress.");
                return;
            }

            if (_activeStack.CurrentWindow != null && _activeStack.CurrentWindow != windowToOpen)
            {
                _isAnimationInProgress = true;
                try
                {
                    await _activeStack.CurrentWindow.Close(animated);
                }
                finally
                {
                    _isAnimationInProgress = false;
                }
            }

            _isAnimationInProgress = true;
            try
            {
                await windowToOpen.Open(windowToOpen.WaitUntilOpeningEnds, animated,isReversedAnimation);
                _activeStack.Push(windowToOpen);
            }
            finally
            {
                _isAnimationInProgress = false;
            }
        }

        public async Task CloseCurrentWindow(bool animated = true)
        {
            if (_activeStack.Count == 0) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning($"Cannot close window '{_activeStack.CurrentWindow.WindowName}' while another animation is in progress.");
                return;
            }

            var windowToClose = _activeStack.Pop();
            _isAnimationInProgress = true;
            try
            {
                await windowToClose.Close(animated);
            }
            finally
            {
                _isAnimationInProgress = false;
            }

            if (_activeStack.CurrentWindow != null)
            {
                _isAnimationInProgress = true;
                try
                {
                    await _activeStack.CurrentWindow.Open(_activeStack.CurrentWindow.WaitUntilOpeningEnds, animated);
                }
                finally
                {
                    _isAnimationInProgress = false;
                }
            }
            else
            {
                Debug.Log("No previous window in stack to open. All windows are now closed.");
            }
        }

        public void CloseAllInstantly()
        {
            foreach (var window in _activeStack.GetAllActiveWindows())
            {
                window.CloseNow();
            }
            _activeStack.Clear();
            _isAnimationInProgress = false;
        }
    }
}