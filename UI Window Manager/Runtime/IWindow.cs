using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Naderite.UI_Window_Manager
{
    public interface IWindow
    {
        string WindowName { get; }
        Sequence AnimateSequence { get; }
        CanvasGroup CanvasGroup { get; }
        RectTransform RectTransform { get; }

        WindowPosition FromPosition { get; }
        WindowPosition StayPosition { get; }
        WindowPosition ToPosition { get; }

        // اضافه کردن متغیرهای جدید برای کنترل میزان جابجایی
        float FromPositionLerpFactor { get; } // مقدار بین 0 تا 1 برای موقعیت شروع
        float ToPositionLerpFactor { get; }   // مقدار بین 0 تا 1 برای موقعیت پایان

        Ease OpeningEase { get; }
        Ease ClosingEase { get; }
        bool IsOpen { get; }

        [Tooltip("If true, the 'Close' operation will await the completion of this window's animation before returning. If false, the animation starts, but the operation returns immediately.")]
        bool WaitUntilClosingEnds { get; } 

        [Tooltip("If true, the 'Open' operation will await the completion of this window's animation before returning. If false, the animation starts, but the operation returns immediately.")]
        bool WaitUntilOpeningEnds { get; } 

        void Initialize();
        Task Open(bool waitForEnd, bool animated = true,bool isReversedAnimation = false); 
        Task Close(bool animated = true,bool isReversedAnimation = false);
        void CloseNow(); 

        Task AsyncAnimate(WindowPosition startPos, WindowPosition endPos, bool alpha, float duration, bool interactable);
        void Animate(WindowPosition startPos, WindowPosition endPos, bool alpha, float duration, bool interactable);

        UnityEvent OnStart { get; }
        UnityEvent OnOpened { get; }
        UnityEvent OnClosed { get; }
    }

    public enum WindowPosition
    {
        Up,
        Left,
        Center,
        Right,
        Down
    }
}