using System.Collections.Generic;
using UnityEngine;

namespace Naderite.UIWindowManager.Custom_Collections
{
    public class RegisteredWindowCollection
    {
        private readonly Dictionary<string, IWindow> _windowsByName = new();

        public void Register(IWindow window)
        {
            if (_windowsByName.ContainsKey(window.WindowName))
            {
                Debug.LogWarning($"A window with the name '{window.WindowName}' is already registered.");
                return;
            }

            _windowsByName.Add(window.WindowName, window);
            window.Initialize();
        }

        public void Unregister(string windowName)
        {
            if (_windowsByName.Remove(windowName))
            {
                // Optionally add cleanup logic for the unregistered window
            }
            else
            {
                Debug.LogWarning($"Window with name '{windowName}' not found for unregistration.");
            }
        }

        public IWindow GetWindow(string windowName)
        {
            _windowsByName.TryGetValue(windowName, out var window);
            if (window == null)
            {
                Debug.LogWarning("This registered window not found : " + windowName);
            }

            return window;
        }

        public IEnumerable<IWindow> GetAllWindows()
        {
            return _windowsByName.Values;
        }
    }
}