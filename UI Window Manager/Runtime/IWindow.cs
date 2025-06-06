using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Naderite.UIWindowManager
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

        float FromPositionLerpFactor { get; }
        float ToPositionLerpFactor { get; }

        float FromScale { get; }
        float StayScale { get; }
        float ToScale { get; }

        Vector3 FromRotation { get; } // زاویه چرخش در موقعیت شروع (X, Y, Z)
        Vector3 StayRotation { get; } // زاویه چرخش در موقعیت ثابت (X, Y, Z)
        Vector3 ToRotation { get; }   // زاویه چرخش در موقعیت پایان (X, Y, Z)

        Ease OpeningEase { get; }
        Ease ClosingEase { get; }
        bool IsOpen { get; }

        bool WaitUntilClosingEnds { get; }
        bool WaitUntilOpeningEnds { get; }

        void Initialize();
        Task Open(bool waitForEnd, bool animated = true, bool isReversedAnimation = false);
        Task Close(bool animated = true, bool isReversedAnimation = false);
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