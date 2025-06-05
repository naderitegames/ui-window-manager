#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Naderite.UI_Window_Manager.Editor
{
    public static class UICreationMenu
    {
        private const string BasePathInMenu = "GameObject/UI/SUI Manager/";

        [MenuItem(BasePathInMenu + "Window Panel", false, 10)]
        public static void CreateDefaultWindowPanel()
        {
            CreateWindowPanel();
        }

        [MenuItem(BasePathInMenu + "UI Manager", false, 0)]
        public static void CreateUIManager()
        {
            CreateManager();
        }

        private static void CreateWindowPanel()
        {
            // Get or create canvas
            Canvas canvas = GetOrCreateCanvas();

            // Create new GameObject
            GameObject panelGO = new GameObject("WindowPanel");
            Undo.RegisterCreatedObjectUndo(panelGO, "Create Window Panel");

            // Add required components
            var panel = panelGO.AddComponent<WindowPanel>();
            var canvasGroup = panelGO.GetComponent<CanvasGroup>();
            var rectTransform = panelGO.GetComponent<RectTransform>();

            // Set default values
            //panel.WindowName = "New Window";
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Set parent
            Transform parent = Selection.activeTransform != null ? Selection.activeTransform : canvas.transform;
            Undo.SetTransformParent(panelGO.transform, parent, "Parent Window Panel");

            // Reset transform
            Undo.RecordObject(rectTransform, "Reset RectTransform");
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 600);
            rectTransform.localScale = Vector3.one;

            // Select and focus
            Selection.activeGameObject = panelGO;
            EditorGUIUtility.PingObject(panelGO);
        }

        private static void CreateManager()
        {
            // Check if UIManager already exists
            if (Object.FindObjectOfType<UIManager>() != null)
            {
                Debug.LogWarning("UIManager already exists in the scene!");
                return;
            }

            // Get or create canvas
            Canvas canvas = GetOrCreateCanvas();

            // Create new GameObject
            GameObject managerGO = new GameObject("UIManager");
            Undo.RegisterCreatedObjectUndo(managerGO, "Create UI Manager");

            // Add required components
            managerGO.AddComponent<UIManager>();
            managerGO.AddComponent<Canvas>();
            managerGO.AddComponent<CanvasScaler>();
            managerGO.AddComponent<GraphicRaycaster>();

            // Set parent
            Transform parent = Selection.activeTransform != null ? Selection.activeTransform : canvas.transform;
            Undo.SetTransformParent(managerGO.transform, parent, "Parent UI Manager");

            // Reset transform
            var rectTransform = managerGO.GetComponent<RectTransform>();
            Undo.RecordObject(rectTransform, "Reset RectTransform");
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            // Select and focus
            Selection.activeGameObject = managerGO;
            EditorGUIUtility.PingObject(managerGO);
        }

        private static Canvas GetOrCreateCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            return canvas;
        }
    }
}
#endif