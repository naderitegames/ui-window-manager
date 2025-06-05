using System.Threading.Tasks;
using Naderite.UI_Window_Manager.Window_Flow_Strategies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Naderite.UI_Window_Manager
{
    public enum WindowStrategyFlowType
    {
        Carousel = 0,
        Stack = 1
    }

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(Canvas))]
    public class WindowManager : MonoBehaviour
    {
        [Header("Window References")] [SerializeField]
        private WindowPanel[] windows;

        [Tooltip("Leave it empty if you want")] [SerializeField]
        private string defaultWindowName;

        [Header("Settings")] [SerializeField]
        private WindowStrategyFlowType _flowType = WindowStrategyFlowType.Carousel;

        [SerializeField] private bool disableAllByDefault = true;
        [SerializeField] private UnityEvent onStart;
        private WindowsContainer _windowsContainer;
        IWindowFlowStrategy flowStrategy;
        public int ActiveWindowsCount => _windowsContainer.ActiveWindowsCount;
        public IWindow CurrentActiveWindow => _windowsContainer.CurrentWindow;

        private void Awake()
        {
            switch (_flowType)
            {
                case WindowStrategyFlowType.Stack:
                    flowStrategy = new StackFlowStrategy();
                    break;
                default:
                case WindowStrategyFlowType.Carousel:
                    flowStrategy = new CarouselFlowStrategy();
                    break;
            }

            _windowsContainer = new WindowsContainer(flowStrategy ?? new CarouselFlowStrategy());
        }

        private void Start()
        {
            RegisterWindows();

            if (disableAllByDefault)
                CloseAllWindows();

            if (!string.IsNullOrEmpty(defaultWindowName))
                OpenThisWindow(defaultWindowName, false);

            onStart?.Invoke();
        }

        private void RegisterWindows()
        {
            foreach (var target in windows)
            {
                if (target)
                    _windowsContainer.RegisterWindow(target);
            }
        }

        public void OpenThisWindow(string windowName)
        {
            _ = _windowsContainer.OpenWindow(windowName);
        }
        public void OpenThisWindow(string windowName, bool animated)
        {
            _ = _windowsContainer.OpenWindow(windowName, animated);
        }
        public void OpenThisWindow(IWindow targetWindow, bool animated = true)
        {
            _ = _windowsContainer.OpenWindow(targetWindow, animated);
        }

        public void CloseAllWindows()
        {
            _windowsContainer.CloseAllWindowsInstantly();
        }

        public async Task CloseLastWindow(bool animated = true)
        {
            await _windowsContainer.CloseCurrentWindow(animated);
        }

        public void OpenNextWindow()
        {
            _windowsContainer.OpenNextWindow();
        }

        public void OpenPreviousWindow()
        {
            _windowsContainer.OpenPreviousWindow();
        }
    }
}