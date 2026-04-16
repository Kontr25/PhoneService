using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Tweeners
{
    /// <summary>
    /// Анимация скрытие и отображения <see cref="CanvasGroup"/> по <see cref="CanvasGroup.alpha"/>
    /// </summary>
    public class TweenerCanvasGroupController : TweenerBase
    {
        /// <summary>
        /// Отображение
        /// </summary>
        private const float EndAlpha = 1f;
        /// <summary>
        /// Скрытие
        /// </summary>
        private const float StartAlpha = 0f;

        [SerializeField] private CanvasGroup canvasGroup;

        /// <summary>
        /// длительность перехода
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float duration = 0.3f;

        [Button]
        public override void Forward(bool skip = false)
        {
            if (skip)
            {
                canvasGroup.alpha = EndAlpha;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                return;
            }

            DOTween.To(() => canvasGroup.alpha, pos => canvasGroup.alpha = pos, EndAlpha, duration);
        }

        [Button]
        public override void Back(bool skip = false)
        {
            if (skip)
            {
                canvasGroup.alpha = StartAlpha;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                return;
            }

            DOTween.To(() => canvasGroup.alpha, pos => canvasGroup.alpha = pos, StartAlpha, duration);
        }

        private void Awake() => DOTween.Init();
    }
}