using System.Collections.Generic;
using System.Linq;
namespace Naderite.UIWindowManager.Custom_Collections
{
    public class ActiveWindowStack
    {
        private readonly Stack<IWindow> _stack = new();

        public IWindow CurrentWindow => currentWindow;
        IWindow currentWindow;
        public int Count => _stack.Count;

        public void Push(IWindow window)
        {
            _stack.Push(window);
            currentWindow = window;
        }

        public IWindow Pop()
        {
            _stack.TryPop(out var window);
            currentWindow = _stack.Peek();
            return window;
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public IEnumerable<IWindow> GetAllActiveWindows()
        {
            return _stack.ToList(); // Return a copy to avoid modification issues
        }
    }
}