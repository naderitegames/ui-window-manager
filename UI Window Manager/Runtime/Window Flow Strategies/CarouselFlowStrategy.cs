using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Naderite.UIWindowManager.Window_Flow_Strategies
{
    public class CarouselFlowStrategy : IWindowFlowStrategy
    {
        public List<IWindow> AllWindows { private set; get; } = new();
        public int ActiveWindowsCount => AllWindows.Count;
        private int _currentIndex = -1;
        private bool _isAnimationInProgress = false;
        public bool AllowRepeatWindows { get; set; }

        public IWindow CurrentWindow =>
            _currentIndex >= 0 && _currentIndex < AllWindows.Count ? AllWindows[_currentIndex] : null;

        public CarouselFlowStrategy()
        {
            AllowRepeatWindows = true;
        }

        public async Task OpenWindow(IWindow window, bool animated = true, bool isReversedAnimation = false)
        {
            if (window == null || !AllWindows.Contains(window) || window.IsOpen) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning($"Cannot open window '{window.WindowName}' while another animation is in progress.");
                return;
            }

            _isAnimationInProgress = true;
            try
            {
                if (CurrentWindow != null && CurrentWindow != window)
                {
                    await CurrentWindow.Close(animated, isReversedAnimation);
                }

                _currentIndex = AllWindows.IndexOf(window);
                await window.Open(window.WaitUntilOpeningEnds, animated, isReversedAnimation);
            }
            finally
            {
                _isAnimationInProgress = false;
            }
        }

        public async Task CloseCurrentWindow(bool animated = true)
        {
            if (_currentIndex < 0 || _currentIndex >= AllWindows.Count) return;

            if (_isAnimationInProgress)
            {
                Debug.LogWarning(
                    $"Cannot close window '{CurrentWindow.WindowName}' while another animation is in progress.");
                return;
            }

            _isAnimationInProgress = true;
            try
            {
                await CurrentWindow.Close(animated);
                _currentIndex = -1;
            }
            finally
            {
                _isAnimationInProgress = false;
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

            if (_currentIndex + 1 >= AllWindows.Count)
            {
                if (!AllowRepeatWindows)
                {
                    Debug.Log("Reached the last window. Repeating is disabled.");
                    return;
                }

                _currentIndex = -1;
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

            if (_currentIndex - 1 < 0)
            {
                if (!AllowRepeatWindows)
                {
                    Debug.Log("Reached the first window. Repeating is disabled.");
                    return;
                }

                _currentIndex = AllWindows.Count;
            }

            int prevIndex = (_currentIndex - 1 + AllWindows.Count) % AllWindows.Count;
            await OpenWindow(AllWindows[prevIndex], animated, true);
        }

        public void CloseAllInstantly()
        {
            foreach (var window in AllWindows)
            {
                window.CloseNow();
            }

            _currentIndex = -1;
            _isAnimationInProgress = false;
        }
    }
}