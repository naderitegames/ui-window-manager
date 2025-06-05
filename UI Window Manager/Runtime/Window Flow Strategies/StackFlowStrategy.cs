using System.Collections.Generic;
using System.Threading.Tasks;
using Naderite.Simple_UIManager.Custom_Collections;
using UnityEngine;

namespace Naderite.UI_Window_Manager.Window_Flow_Strategies
{
    public class StackFlowStrategy : IWindowFlowStrategy
    {
        private readonly ActiveWindowStack _activeStack = new ActiveWindowStack();
        private bool _isAnimationInProgress = false;
        private int _currentIndex = -1; // برای ردیابی ایندکس فعلی در AllWindows

        public IWindow CurrentWindow => _activeStack.CurrentWindow;
        public int ActiveWindowsCount => _activeStack.Count;
        public List<IWindow> AllWindows { get; } = new List<IWindow>();

        public async Task OpenWindow(IWindow windowToOpen, bool animated = true, bool isReversedAnimation = false)
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
                    await _activeStack.CurrentWindow.Close(animated,isReversedAnimation);
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
                _currentIndex = AllWindows.IndexOf(windowToOpen); // به‌روزرسانی ایندکس
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
                _currentIndex = _activeStack.CurrentWindow != null ? AllWindows.IndexOf(_activeStack.CurrentWindow) : -1;
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

        public async Task NextWindow(bool animated = true)
        {
            if (AllWindows.Count == 0) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning("Cannot move to next window while another animation is in progress.");
                return;
            }

            int nextIndex = (_currentIndex + 1) % AllWindows.Count;
            await OpenWindow(AllWindows[nextIndex], animated);
        }

        public async Task PreviousWindow(bool animated = true)
        {
            if (AllWindows.Count == 0) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning("Cannot move to previous window while another animation is in progress.");
                return;
            }

            int prevIndex = (_currentIndex - 1 + AllWindows.Count) % AllWindows.Count;
            await OpenWindow(AllWindows[prevIndex], animated);
        }

        public void CloseAllInstantly()
        {
            foreach (var window in _activeStack.GetAllActiveWindows())
            {
                window.CloseNow();
            }

            _activeStack.Clear();
            _currentIndex = -1;
            _isAnimationInProgress = false;
        }
    }
}