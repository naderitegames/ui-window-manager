using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Naderite.UIWindowManager.Window_Flow_Strategies
{
    public class CarouselFlowStrategy : IWindowFlowStrategy
    {
        public List<IWindow> AllWindows { get; } = new();
        public int ActiveWindowsCount => AllWindows.Count;
        private int _currentIndex = -1;
        private bool _isAnimationInProgress = false;
        public bool AllowRepeatWindows { get; set; }

        public IWindow CurrentWindow =>
            _currentIndex >= 0 && _currentIndex < AllWindows.Count ? AllWindows[_currentIndex] : null;

        public CarouselFlowStrategy(bool allowRepeatWindows)
        {
            AllowRepeatWindows = allowRepeatWindows;
        }

        public async Task OpenWindow(IWindow window, bool animated = true, bool isReversedAnimation = false)
        {
            if (window == null)
            {
                Debug.LogError("Window reference is null! Cannot proceed.");
                return;
            }

            if (!AllWindows.Contains(window))
            {
                Debug.LogError($"Window '{window.WindowName}' not found in AllWindows collection!");
                return;
            }

            if (window.IsOpen)
            {
                Debug.LogWarning($"Window '{window.WindowName}' is already open!");
                return;
            }

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
                Debug.LogWarning($"Cannot close window '{CurrentWindow.WindowName}' while another animation is in progress.");
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

            int nextIndex;
            if (_currentIndex + 1 >= AllWindows.Count)
            {
                if (!AllowRepeatWindows)
                {
                    Debug.Log("Reached the last window. Repeating is disabled.");
                    return;
                }
                nextIndex = 0; // Loop back to the first window
            }
            else
            {
                nextIndex = _currentIndex + 1;
            }

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

            int prevIndex;
            if (_currentIndex - 1 < 0)
            {
                if (!AllowRepeatWindows)
                {
                    Debug.Log("Reached the first window. Repeating is disabled.");
                    return;
                }
                prevIndex = AllWindows.Count - 1; // Loop to the last window
            }
            else
            {
                prevIndex = _currentIndex - 1;
            }

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