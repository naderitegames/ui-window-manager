using System.Threading.Tasks;
using Naderite.UIWindowManager.Window_Flow_Strategies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Naderite.UIWindowManager
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

        [SerializeField] private string defaultWindowName;

        [Header("Button References")] [SerializeField]
        private Button targetNextButton;

        [SerializeField] private Button targetPreviousButton;

        [Header("Settings")] [SerializeField]
        private WindowStrategyFlowType _flowType = WindowStrategyFlowType.Carousel;

        [SerializeField] private bool disableAllByDefault = true;
        [SerializeField] private bool allowRepeatWindows = true;
        [SerializeField] private UnityEvent onStart;
        private WindowsContainer _windowsContainer;
        private IWindowFlowStrategy flowStrategy;
        public int ActiveWindowsCount => _windowsContainer.ActiveWindowsCount;
        public IWindow CurrentActiveWindow => _windowsContainer.CurrentWindow;

        private void Awake()
        {
            switch (_flowType)
            {
                case WindowStrategyFlowType.Stack:
                    flowStrategy = new StackFlowStrategy(allowRepeatWindows);
                    break;
                default:
                case WindowStrategyFlowType.Carousel:
                    flowStrategy = new CarouselFlowStrategy(allowRepeatWindows);
                    break;
            }

            _windowsContainer = new WindowsContainer(flowStrategy ?? new CarouselFlowStrategy(allowRepeatWindows));
        }

        private void OnEnable()
        {
            if (targetNextButton != null)
            {
                targetNextButton.onClick.AddListener(OpenNextWindow);
            }

            if (targetPreviousButton != null)
            {
                targetPreviousButton.onClick.AddListener(OpenPreviousWindow);
            }

            UpdateButtonStates();
        }

        private void OnDisable()
        {
            if (targetNextButton != null)
            {
                targetNextButton.onClick.RemoveListener(OpenNextWindow);
            }

            if (targetPreviousButton != null)
            {
                targetPreviousButton.onClick.RemoveListener(OpenPreviousWindow);
            }
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
            UpdateButtonStates();
        }

        public void OpenThisWindow(string windowName, bool animated)
        {
            _ = _windowsContainer.OpenWindow(windowName, animated);
            UpdateButtonStates();
        }

        public void OpenThisWindow(IWindow targetWindow, bool animated = true)
        {
            _ = _windowsContainer.OpenWindow(targetWindow, animated);
            UpdateButtonStates();
        }

        public void CloseAllWindows()
        {
            _windowsContainer.CloseAllWindowsInstantly();
            UpdateButtonStates();
        }

        public async Task CloseLastWindow(bool animated = true)
        {
            await _windowsContainer.CloseCurrentWindow(animated);
            UpdateButtonStates();
        }

        public void OpenNextWindow()
        {
            _windowsContainer.OpenNextWindow();
            UpdateButtonStates();
        }

        public void OpenPreviousWindow()
        {
            _windowsContainer.OpenPreviousWindow();
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (flowStrategy == null || flowStrategy.AllWindows.Count == 0)
            {
                SetButtonInteractable(targetNextButton, false);
                SetButtonInteractable(targetPreviousButton, false);
                return;
            }

            int currentIndex = GetCurrentWindowIndex();
            bool canGoNext = allowRepeatWindows || currentIndex + 1 < flowStrategy.AllWindows.Count;
            bool canGoPrevious = allowRepeatWindows || currentIndex - 1 >= 0;

            SetButtonInteractable(targetNextButton, canGoNext);
            SetButtonInteractable(targetPreviousButton, canGoPrevious);
        }

        private int GetCurrentWindowIndex()
        {
            if (CurrentActiveWindow == null) return -1;
            return flowStrategy.AllWindows.IndexOf(CurrentActiveWindow);
        }

        private void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}