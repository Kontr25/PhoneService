using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.StateSwitcher
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class ColorSwitch : StateItem
    {
        [HorizontalGroup("h1")] [HideLabel] [SerializeField]
        private Color _color = Color.white;

        [HorizontalGroup("h1")] [HideLabel] [SerializeField] [OnValueChanged("CheckObject")]
        private Object _object;

        [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
        private bool _smoothChange;

        [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
        private float _duration;

        [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
        private AnimationCurve _curve;

        [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
        private bool _useOwnStartColor;

        [HideIf("$_useOwnStartColor")] [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
        private Color _startColor = Color.white;


        public override void Set()
        {
            if (_object == null) return;
            switch (_object)
            {
                case Outline outline:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? outline.effectColor : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() =>
                                outline.effectColor = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        outline.effectColor = _color;

                    break;
                case Shadow shd:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? shd.effectColor : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() =>
                                shd.effectColor = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        shd.effectColor = _color;

                    break;
                case Camera cam:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? cam.backgroundColor : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() =>
                                cam.backgroundColor = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        cam.backgroundColor = _color;

                    break;
                case SpriteRenderer sprRend:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? sprRend.color : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() =>
                                sprRend.color = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        sprRend.color = _color;

                    break;
                case Graphic img:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? img.color : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() => img.color = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        if(img != null) img.color = _color;

                    break;
                case MeshRenderer mesh:
                    if (_smoothChange && Application.isPlaying)
                    {
                        var startColor = _useOwnStartColor ? mesh.material.color : _startColor;
                        var valueToTween = 0f;
                        DOTween.To(() => valueToTween, x => valueToTween = x, 1, _duration)
                            .OnUpdate(() =>
                                mesh.material.color = Color.Lerp(startColor, _color, _curve.Evaluate(valueToTween)));
                    }
                    else
                        mesh.material.color = _color;

                    break;
            }
        }

        public override void DefaultState()
        {
            if(_object == null) return;
            switch (_object)
            {
                case Outline outline:
                    outline.effectColor = Color.white;
                    break;
                case Shadow shd:
                    shd.effectColor = Color.white;
                    break;
                case Camera cam:
                    cam.backgroundColor = _color;
                    break;
                case SpriteRenderer sprRend:
                    sprRend.color = Color.white;
                    break;
                case Graphic img:
                    if(img != null) img.color = Color.white;
                    break;
                case MeshRenderer mesh:
                    mesh.material.color = Color.white;
                    break;
            }
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var clone = new ColorSwitch
            {
                _color = _color,
                _duration = _duration,
                _object = _object,
                _smoothChange = _smoothChange,
                _startColor = _startColor,
                _curve = _curve,
                _useOwnStartColor = _useOwnStartColor
            };
            return clone;
        }

        [ShowIf("@_object != null")]
        [Button("Get from...")]
        [GUIColor("@Color.green")]
        [HorizontalGroup("h1")]
        private void SetCurrent()
        {
            switch (_object)
            {
                case Outline outline:
                    _color = outline.effectColor;
                    break;
                case Shadow shd:
                    _color = shd.effectColor;
                    break;
                case Camera cam:
                    _color = cam.backgroundColor;
                    break;
                case SpriteRenderer sprRend:
                    _color = sprRend.color;
                    break;
                case Graphic img:
                    _color = img.color;
                    break;
                case MeshRenderer mesh:
                    _color = mesh.material.color;
                    break;
            }
        }

        public override string DropDownItemName => "Switch Color";
        public override string DropDownGroupName => "Graphics";

        private void CheckObject()
        {
            switch (_object)
            {
                case Outline outline:
                case Shadow shd:
                case Camera cam:
                case Renderer sprRen:
                case Text text:
                case Image img:
                    break;
                default:
                    var go = (_object as GameObject)?.gameObject;
                    if (go == null)
                        go = (_object as UnityEngine.Component)?.gameObject;
                    if (go == null)
                    {
                        _object = null;
                        return;
                    }

                    Object cmp = go.GetComponent<Renderer>();
                    if (cmp == null)
                        cmp = go.GetComponent<Graphic>();

                    if (cmp != null)
                        _object = cmp;
                    break;
            }
        }

#endif
    }
}