using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace UI.Tweeners
{
    /// <summary>
    /// Анимация смещения по точке якоря (полезно для скрытия частей UI)
    /// </summary>
    public class TweenPivotController : TweenerBase
    {
        [SerializeField, Tooltip("Позиция при скрытие")]
        private Vector2 startPos;

        [SerializeField] [Tooltip("Отключение при скрытии")]
        private bool _hideInStart;

        [SerializeField, Tooltip("Позиция при показе")]
        private Vector2 endPos;

        [SerializeField] [Tooltip("Отключение при скрытии")]
        private bool _hideInEnd;


        /// <summary>
        /// длительность перехода
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float duration = 0.3f;

        private RectTransform rect;
        private TweenerCore<Vector2, Vector2, VectorOptions> _tween;

        private RectTransform Rect
        {
            get
            {
                if (rect == null)
                    rect = transform.GetComponent<RectTransform>();
                return rect;
            }
        }

        public override void Forward(bool skip = false)
        {
            if (!isActiveAndEnabled)
                return;

            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            if (skip)
            {
                Rect.pivot = endPos;
                Rect.gameObject.SetActive(!_hideInEnd);
                return;
            }


            Rect.gameObject.SetActive(true);
            _tween = DOTween.To(() => Rect.pivot, pos => Rect.pivot = pos, endPos, duration).OnComplete(() =>
                Rect.gameObject.SetActive(!_hideInEnd));
        }

        public override void Back(bool skip = false)
        {
            if (!isActiveAndEnabled)
                return;
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }

            if (skip)
            {
                Rect.pivot = startPos;
                Rect.gameObject.SetActive(!_hideInStart);
                return;
            }


            Rect.gameObject.SetActive(true);
            _tween = DOTween.To(() => Rect.pivot, pos => Rect.pivot = pos, startPos, duration).OnComplete(() =>
                Rect.gameObject.SetActive(!_hideInStart));
        }

        private void Awake() => DOTween.Init();
    }
}