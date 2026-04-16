using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Tweeners
{
    /// <summary>
    /// Анимация вращения
    /// </summary>
    public class TweenerRotationController : TweenerBase
    {
        [SerializeField, SuffixLabel("@(mode?\"Fast\":\"Beyond360\")"),Tooltip("Режим проигрывания")]
        private bool mode;

        /// <summary>
        /// Положение стартовое (скрыто)
        /// </summary>
        [SerializeField,Tooltip("Положение при скрытие")] private Vector3 start;

        /// <summary>
        /// Положение финальное (показано)
        /// </summary>
        [SerializeField,Tooltip("Положение при показе")] private Vector3 end;

        /// <summary>
        /// длительность перехода
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float duration = 0.3f;

        private RectTransform rect;

        private RectTransform Rect
        {
            get
            {
                if (rect == null)
                    rect = transform.GetComponent<RectTransform>();
                return rect;
            }
        }

        [Button]
        public override void Forward(bool skip = false)
        {
            if (skip)
            {
                Rect.localEulerAngles = end;
                return;
            }

            transform.DOLocalRotate(end, duration, !mode ? RotateMode.Fast : RotateMode.FastBeyond360);
        }

        [Button]
        public override void Back(bool skip = false)
        {
            if (skip)
            {
                Rect.localEulerAngles = start;
                return;
            }

            transform.DOLocalRotate(start, duration, !mode ? RotateMode.Fast : RotateMode.FastBeyond360);
        }

        private void Awake() => DOTween.Init();
    }
}