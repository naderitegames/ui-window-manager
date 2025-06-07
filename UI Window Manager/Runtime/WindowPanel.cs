using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Naderite.UIWindowManager
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public class WindowPanel : MonoBehaviour, IWindow
    {
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

        [Header("Scale Settings")]
        [SerializeField] private Vector2 fromScale = Vector2.one;
        [SerializeField] private Vector2 stayScale = Vector2.one;
        [SerializeField] private Vector2 toScale = Vector2.one;

        [Header("Rotation Settings")]
        [SerializeField] private Vector3 fromRotation = Vector3.zero;
        [SerializeField] private Vector3 stayRotation = Vector3.zero;
        [SerializeField] private Vector3 toRotation = Vector3.zero;

        [Header("Alpha Settings")]
        [SerializeField, Range(0f, 1f)] private float fromAlpha = 0f;
        [SerializeField, Range(0f, 1f)] private float stayAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float toAlpha = 0f;

        [Header("Duration Settings")]
        [SerializeField] private float openingDuration = 0.5f;
        [SerializeField] private float closingDuration = 0.5f;

        [SerializeField] private bool waitUntilClosingEnds = true;
        [SerializeField] private bool waitUntilOpeningEnds = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onStart;
        [FormerlySerializedAs("onOpen")] [SerializeField] private UnityEvent onOpened;
        [FormerlySerializedAs("onClose")] [SerializeField] private UnityEvent onClosed;

        public string WindowName => windowName;
        public Sequence AnimateSequence { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Ease OpeningEase => openingEase;
        public Ease ClosingEase => closingEase;
        public float FromPositionLerpFactor => fromPositionLerpFactor;
        public float ToPositionLerpFactor => toPositionLerpFactor;
        public Vector2 FromScale => fromScale;
        public Vector2 StayScale => stayScale;
        public Vector2 ToScale => toScale;
        public Vector3 FromRotation => fromRotation;
        public Vector3 StayRotation => stayRotation;
        public Vector3 ToRotation => toRotation;
        public float FromAlpha => fromAlpha;
        public float StayAlpha => stayAlpha;
        public float ToAlpha => toAlpha;
        public WindowPosition FromPosition => fromPosition;
        public WindowPosition StayPosition => stayPosition;
        public WindowPosition ToPosition => toPosition;
        public bool IsOpen { get; private set; }
        public bool WaitUntilClosingEnds => waitUntilClosingEnds;
        public bool WaitUntilOpeningEnds => waitUntilOpeningEnds;
        public UnityEvent OnStart => onStart;
        public UnityEvent OnOpened => onOpened;
        public UnityEvent OnClosed => onClosed;

        private Canvas _parentCanvas;

        public void Initialize()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();

            if (!_parentCanvas)
            {
                Debug.LogError($"WindowPanel '{gameObject.name}' cannot find a parent Canvas. UI animations might not work correctly.", this);
            }

            CloseNow();
        }

        public async Task Open(bool waitForEnd, bool animated = true, bool isReversedAnimation = false)
        {
            AnimateSequence?.Kill(true);
            var startPos = isReversedAnimation ? toPosition : fromPosition;
            var endPos = stayPosition;
            var startScale = isReversedAnimation ? toScale : fromScale;
            var endScale = stayScale;
            var startRotation = isReversedAnimation ? toRotation : fromRotation;
            var endRotation = stayRotation;
            var startAlpha = isReversedAnimation ? toAlpha : fromAlpha;
            var endAlpha = stayAlpha;

            RectTransform.SetAsLastSibling(); // Move to top layer (last child)

            if (animated)
            {
                await Animate(startPos, endPos, startScale, endScale, startRotation, endRotation, startAlpha, endAlpha, true, openingDuration, true, waitForEnd);
            }
            else
            {
                ChangeStatusTo(true);
            }
        }

        public async Task Close(bool animated = true, bool isReversedAnimation = false)
        {
            AnimateSequence?.Kill(true);
            var startPos = stayPosition;
            var endPos = isReversedAnimation ? fromPosition : toPosition;
            var startScale = stayScale;
            var endScale = isReversedAnimation ? fromScale : toScale;
            var startRotation = stayRotation;
            var endRotation = isReversedAnimation ? fromRotation : toRotation;
            var startAlpha = stayAlpha;
            var endAlpha = isReversedAnimation ? fromAlpha : toAlpha;

            RectTransform.SetAsFirstSibling(); // Move to bottom layer (first child)

            if (animated)
            {
                await Animate(startPos, endPos, startScale, endScale, startRotation, endRotation, startAlpha, endAlpha, false, closingDuration, false, waitUntilClosingEnds);
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

        private async Task Animate(WindowPosition startPos, WindowPosition endPos, Vector2 startScale, Vector2 endScale, Vector3 startRotation, Vector3 endRotation, float startAlpha, float endAlpha, bool status, float duration, bool targetInteractable, bool waitForEnd)
        {
            AnimateSequence?.Kill(true);
            AnimateSequence = DOTween.Sequence().SetUpdate(true)
                .OnStart(() =>
                {
                    IsOpen = status;
                    if (status)
                    {
                        OnStart?.Invoke();
                        RectTransform.SetAsLastSibling(); // Move to top layer when open
                    }
                    RectTransform.anchoredPosition = GetWindowPosition(startPos, status ? fromPositionLerpFactor : toPositionLerpFactor);
                    RectTransform.localScale = startScale;
                    RectTransform.localEulerAngles = startRotation;
                    CanvasGroup.alpha = startAlpha;
                    CanvasGroup.interactable = false;
                    CanvasGroup.blocksRaycasts = false;
                })
                .Append(RectTransform.DOAnchorPos(GetWindowPosition(endPos, status ? fromPositionLerpFactor : toPositionLerpFactor), duration))
                .Join(CanvasGroup.DOFade(endAlpha, duration))
                .Join(RectTransform.DOScale(endScale, duration))
                .Join(RectTransform.DORotate(endRotation, duration))
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
                        RectTransform.SetAsFirstSibling(); // Move to bottom layer when closed
                    }
                });

            if (waitForEnd && AnimateSequence.IsActive())
            {
                await AnimateSequence.AsyncWaitForCompletion();
            }
        }

        private void ChangeStatusTo(bool status)
        {
            CanvasGroup.alpha = status ? stayAlpha : toAlpha;
            CanvasGroup.interactable = status;
            CanvasGroup.blocksRaycasts = status;
            RectTransform.anchoredPosition = GetWindowPosition(status ? stayPosition : toPosition);
            RectTransform.localScale = status ? stayScale : toScale;
            RectTransform.localEulerAngles = status ? stayRotation : toRotation;
            IsOpen = status;
        }

        private Vector2 GetWindowPosition(WindowPosition position, float lerpFactor = 1f)
        {
            if (_parentCanvas == null) return RectTransform.anchoredPosition;

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

        public async Task AsyncAnimate(WindowPosition startPos, WindowPosition endPos, bool alpha, float duration, bool interactable)
        {
            await Animate(startPos, endPos, alpha ? fromScale : stayScale, alpha ? stayScale : toScale, alpha ? fromRotation : stayRotation, alpha ? stayRotation : toRotation, alpha ? fromAlpha : stayAlpha, alpha ? stayAlpha : toAlpha, alpha, duration, interactable, true);
        }

        public void Animate(WindowPosition startPos, WindowPosition endPos, bool alpha, float duration, bool interactable)
        {
            Animate(startPos, endPos, alpha ? fromScale : stayScale, alpha ? stayScale : toScale, alpha ? fromRotation : stayRotation, alpha ? stayRotation : toRotation, alpha ? fromAlpha : stayAlpha, alpha ? stayAlpha : toAlpha, alpha, duration, interactable, false).GetAwaiter().GetResult();
        }
    }
}