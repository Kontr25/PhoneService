using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tweeners
{
    /// <summary>
    /// Твинер изменения цвета
    /// </summary>
    public class TweenerColorChange : TweenerBase
    {
        /// <summary>
        /// Конечный цвет
        /// </summary>
        [ColorPalette]
        public Color _startColor;
        
        /// <summary>
        /// Начальный цвет
        /// </summary>
        [ColorPalette]
        public Color _endColor;
        
        /// <summary>
        /// Целевое изображение
        /// </summary>
        [SerializeField] private Image _image;
        
        /// <summary>
        /// длительность перехода
        /// </summary>
        [SerializeField, Range(0.1f, 2f)] private float duration = 0.2f;
        
        private void Awake() => DOTween.Init();
        
        public override void Forward(bool skip = false)
        {
            if (skip)
            {
                _image.color = _endColor;
                return;
            }
            
            DOTween.To(() => _image.color, color => _image.color = color, _endColor, duration);
        }

        public override void Back(bool skip = false)
        {
            if (skip)
            {
                _image.color = _startColor;
                return;
            }
            
            DOTween.To(() => _image.color, color => _image.color = color, _startColor, duration);
        }
    }
}