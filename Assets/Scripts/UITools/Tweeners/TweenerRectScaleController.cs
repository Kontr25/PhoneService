using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Tweeners
{
    /// <summary>
    /// Анимация масштабирования
    /// </summary>
    public class TweenerRectScaleController : TweenerBase
    {
        /// <summary>
        /// Размер стартовый (скрыто)
        /// </summary>
        [SerializeField, Tooltip("Размер при скрытие")] private Vector2 start = Vector2.zero;

        /// <summary>
        /// Размер финальный (показано)
        /// </summary>
        [SerializeField,Tooltip("Размер при показе")] private Vector2 end = Vector2.one;

        /// <summary>
        /// длительность перехода
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float duration = 0.3f;

        /// <summary>
        /// Кривая скорости анимации в прямом направлении
        /// </summary>
        [SerializeField] private AnimationCurve _animationCurveForward;
        
        /// <summary>
        /// Кривая скорости анимации в обратном направлении
        /// </summary>
        [SerializeField] private AnimationCurve _animationCurveBack;
        
        /// <summary>
        /// Флаг, указвающий на необходимость сброса скейла перед анимацией (tween)
        /// </summary>
        [SerializeField] private bool _resetScaleBeforeTween;

        private RectTransform rect;
        private Coroutine _coroutine;

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
                Rect.sizeDelta = end;
                return;
            }
            
            if(_resetScaleBeforeTween) Rect.sizeDelta = start;

            var currentSize = Rect.sizeDelta;
            DoSize(currentSize,end,duration);
        }

        [Button]
        public override void Back(bool skip = false)
        {
            if (skip)
            {
                Rect.sizeDelta = start;
                return;
            }
            
            if(_resetScaleBeforeTween) Rect.sizeDelta = end;

            var currentSize = Rect.sizeDelta;
            DoSize(currentSize,start,duration);
        }

        private void DoSize(Vector2 from, Vector2 to, float duration, AnimationCurve curve = null)
        {
            TryStopDoSize();
            _coroutine = StartCoroutine(DoSizeCoroutine(from, to, duration));
        }

        private void TryStopDoSize()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private IEnumerator DoSizeCoroutine(Vector2 from, Vector2 to, float duration, AnimationCurve curve = null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                
                Vector2 newValue = new Vector2();
                if (curve != null) newValue = Vector2.Lerp(from, to, curve.Evaluate(t));
                else newValue =  Vector2.Lerp(from, to, t);
                
                Rect.sizeDelta = newValue;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Rect.sizeDelta = to;
        }
    }
}