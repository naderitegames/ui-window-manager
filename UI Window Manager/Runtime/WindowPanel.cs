using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Naderite.UIWindowManager
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class WindowPanel : MonoBehaviour, IWindow
    {
        public string WindowName => windowName;
        [SerializeField] private string windowName;

        [Header("Animation Settings")]
        [SerializeField] private Ease openingEase = Ease.OutBack;
        [SerializeField] private Ease closingEase = Ease.InBack;

        [SerializeField] private WindowPosition fromPosition = WindowPosition.Down;
        [SerializeField] private WindowPosition stayPosition = WindowPosition.Center;
        [SerializeField] private WindowPosition toPosition = WindowPosition.Down;

        [Header("Animation Lerp Factors")]
        [SerializeField, Range(0f, 1f)] private float fromPositionLerpFactor = 1f;
        [SerializeField, Range(0f, 1f)] private float toPositionLerpFactor = 1f;

        [Header("Scale Settings")] // اضافه کردن بخش جدید برای تنظیمات مقیاس
        [SerializeField, Range(0f, 2f)] private float fromScale = 1f;
        [SerializeField, Range(0f, 2f)] private float stayScale = 1f;
        [SerializeField, Range(0f, 2f)] private float toScale = 1f;

        [SerializeField] private float openingDuration = 0.5f;
        [SerializeField] private float closingDuration = 0.5f;

        public bool WaitUntilClosingEnds => waitUntilClosingEnds;
        [SerializeField] private bool waitUntilClosingEnds = true;

        public bool WaitUntilOpeningEnds => waitUntilOpeningEnds;
        [SerializeField] private bool waitUntilOpeningEnds = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onStart;
        [FormerlySerializedAs("onOpen")] [SerializeField] private UnityEvent onOpened;
        [FormerlySerializedAs("onClose")] [SerializeField] private UnityEvent onClosed;

        public Sequence AnimateSequence { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Ease OpeningEase => openingEase;
        public Ease ClosingEase => closingEase;
        public float FromPositionLerpFactor => fromPositionLerpFactor;
        public float ToPositionLerpFactor => toPositionLerpFactor;

        // پیاده‌سازی متغیرهای مقیاس
        public float FromScale => fromScale;
        public float StayScale => stayScale;
        public float ToScale => toScale;

        public WindowPosition FromPosition => fromPosition;
        public WindowPosition StayPosition => stayPosition;
        public WindowPosition ToPosition => toPosition;

        public bool IsOpen { get; private set; }

        public UnityEvent OnStart => onStart;
        public UnityEvent OnOpened => onOpened;
        public UnityEvent OnClosed => onClosed;

        private Canvas _parentCanvas;

        public void Initialize()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            if (_parentCanvas == null)
            {
                Debug.LogError(
                    $"WindowPanel '{gameObject.name}' cannot find a parent Canvas. UI animations might not work correctly.",
                    this);
            }

            CloseNow();
        }

        public virtual async Task Open(bool waitForEnd, bool animated = true, bool isReversedAnimation = false)
        {
            AnimateSequence?.Kill(true);
            if (animated)
            {
                if (waitForEnd)
                    await AsyncAnimate(
                        isReversedAnimation ? toPosition : fromPosition,
                        stayPosition,
                        true,
                        openingDuration,
                        true
                    );
                else
                    Animate(
                        isReversedAnimation ? toPosition : fromPosition,
                        stayPosition,
                        true,
                        openingDuration,
                        true
                    );
            }
            else
            {
                ChangeStatusTo(true);
            }
        }

        public virtual async Task Close(bool animated = true, bool isReversedAnimation = false)
        {
            AnimateSequence?.Kill();
            if (animated)
            {
                if (WaitUntilClosingEnds)
                    await AsyncAnimate(
                        stayPosition,
                        isReversedAnimation ? fromPosition : toPosition,
                        false,
                        closingDuration,
                        false
                    );
                else
                    Animate(
                        stayPosition,
                        isReversedAnimation ? fromPosition : toPosition,
                        false,
                        closingDuration,
                        false
                    );
            }
            else
            {
                ChangeStatusTo(false);
            }
        }

        public void CloseNow()
        {
            ChangeStatusTo(false);
        }

        void ChangeStatusTo(bool status)
        {
            CanvasGroup.alpha = status ? 1 : 0;
            CanvasGroup.interactable = status;
            CanvasGroup.blocksRaycasts = status;
            RectTransform.anchoredPosition = GetWindowPosition(status ? stayPosition : toPosition);
            RectTransform.localScale = Vector3.one * (status ? stayScale : toScale);
            IsOpen = status;
        }

        public virtual async Task AsyncAnimate(WindowPosition startPos, WindowPosition endPos, bool alpha,
            float duration, bool interactable)
        {
            BaseAnimate(startPos, endPos, alpha, duration, interactable);
            if (AnimateSequence != null && AnimateSequence.IsActive())
            {
                await AnimateSequence.AsyncWaitForCompletion();
            }
        }

        public void Animate(WindowPosition startPos, WindowPosition endPos, bool alpha, float duration,
            bool interactable)
        {
            BaseAnimate(startPos, endPos, alpha, duration, interactable);
        }

        void BaseAnimate(WindowPosition startPos, WindowPosition endPos, bool status, float duration,
            bool targetInteractable)
        {
            AnimateSequence?.Kill();
            AnimateSequence = DOTween.Sequence().SetUpdate(true)
                .OnStart(() =>
                {
                    IsOpen = status;
                    if (status)
                        OnStart?.Invoke();
                    RectTransform.anchoredPosition = GetWindowPosition(startPos,
                        status ? FromPositionLerpFactor : ToPositionLerpFactor);
                    RectTransform.localScale = Vector3.one * (status ? fromScale : stayScale);
                    CanvasGroup.alpha = status ? 0f : 1f;
                    CanvasGroup.interactable = false;
                    CanvasGroup.blocksRaycasts = false;
                })
                .Append(RectTransform.DOAnchorPos(
                    GetWindowPosition(endPos, status ? FromPositionLerpFactor : ToPositionLerpFactor), duration))
                .Join(CanvasGroup.DOFade(status ? 1 : 0, duration))
                .Join(RectTransform.DOScale(status ? stayScale : toScale, duration))
                .SetEase(status ? openingEase : closingEase)
                .OnComplete(() =>
                {
                    CanvasGroup.interactable = targetInteractable;
                    CanvasGroup.blocksRaycasts = targetInteractable;

                    if (status)
                    {
                        OnOpened?.Invoke();
                    }
                    else
                    {
                        OnClosed?.Invoke();
                    }
                });
        }

        private Vector2 GetWindowPosition(WindowPosition position, float lerpFactor = 1f)
        {
            if (_parentCanvas == null)
                return RectTransform.anchoredPosition;

            RectTransform canvasRectTransform = _parentCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRectTransform.rect.size;
            Vector2 canvasCenter = Vector2.zero;

            float panelWidth = RectTransform.rect.width * RectTransform.localScale.x;
            float panelHeight = RectTransform.rect.height * RectTransform.localScale.y;

            Vector2 targetPosition = position switch
            {
                WindowPosition.Up => new Vector2(canvasCenter.x, canvasSize.y / 2f + panelHeight / 2f),
                WindowPosition.Left => new Vector2(-canvasSize.x / 2f - panelWidth / 2f, canvasCenter.y),
                WindowPosition.Center => canvasCenter,
                WindowPosition.Right => new Vector2(canvasSize.x / 2f + panelWidth / 2f, canvasCenter.y),
                WindowPosition.Down => new Vector2(canvasCenter.x, -canvasSize.y / 2f - panelHeight / 2f),
                _ => canvasCenter
            };

            return Vector2.Lerp(canvasCenter, targetPosition, lerpFactor);
        }

        private void OnDestroy()
        {
            AnimateSequence?.Kill();
        }
    }
}