using UnityEngine;

namespace Naderite.UIWindowManager
{
    public class UIManager : MonoBehaviour
    {
        // why I repeat these properties here? because i did not want to edit other scripts fo now ...
        [SerializeField] WindowManager targetWindowManager;
        public WindowManager WindowManager => targetWindowManager;
        public IWindow CurrentActiveWindow => WindowManager.CurrentActiveWindow;
        public int ActiveWindowsCount => WindowManager.ActiveWindowsCount;

        public enum SingletonType
        {
            Global,
            Scene
        }

        [SerializeField] protected SingletonType singletonType = SingletonType.Global;

        static UIManager _instance;

        public static UIManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (!_instance)
                    {
                        GameObject single = new GameObject(typeof(UIManager).Name + " instance");
                        single.AddComponent<UIManager>();
                        _instance = single.GetComponent<UIManager>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                if (singletonType == SingletonType.Global)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OpenThisWindow(string name, bool animate = true)
        {
            WindowManager.OpenThisWindow(name, animate);
        }

        public void CloseLastWindow()
        {
            _ = WindowManager.CloseLastWindow();
        }

        public void CloseAllWindows()
        {
            WindowManager.CloseAllWindows();
        }
    }
}