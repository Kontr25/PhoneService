using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.StateSwitcher
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class ScaleSwitch : StateItem
    {
        [LabelText("Размеры объектов")] [SerializeField]
        private List<ObjectsScalesData> _list;


        public override void Set()
        {
            foreach (var objectsScalesData in _list)
            {
                if (objectsScalesData.SmoothChange && Application.isPlaying)
                {
                    var startScale = objectsScalesData.UseOwnStartScale
                        ? objectsScalesData.Transform.localScale
                        : objectsScalesData.StartScale;
                    var valueToTween = 0f;
                    DOTween.To(() => valueToTween, x => valueToTween = x, 1, objectsScalesData.Duration)
                        .OnUpdate(() => objectsScalesData.Transform.localScale = Vector3.Lerp(startScale,
                            objectsScalesData.LocaleScale, objectsScalesData.Curve.Evaluate(valueToTween)));
                }
                else
                {
                    objectsScalesData.Transform.localScale = objectsScalesData.LocaleScale;
                }
            }
        }

        public override void DefaultState()
        {
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var clone = new ScaleSwitch();
            var list = new List<ObjectsScalesData>();
            clone._list = list;
            foreach (var objectsScalesData in _list)
            {
                list.Add(objectsScalesData.Clone());
            }

            return clone;
        }

        public override string DropDownGroupName => "Objects";
        public override string DropDownItemName => "Switch Scale";

#endif

        [Serializable]
        internal class ObjectsScalesData
        {
            [SerializeField] [HideLabel] [HorizontalGroup("h1")]
            private Transform _transform;

            [SerializeField] [HideLabel] [HorizontalGroup("h1")]
            private Vector3 _localeScale;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private bool _smoothChange;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private float _duration;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private AnimationCurve _curve;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private bool _useOwnStartScale;

            [HideIf("$_useOwnStartScale")] [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private Vector3 _startScale;

            public AnimationCurve Curve => _curve;

            public bool SmoothChange => _smoothChange;

            public float Duration => _duration;
            public Transform Transform => _transform;

            public Vector3 LocaleScale => _localeScale;
            public Vector3 StartScale => _startScale;
            public bool UseOwnStartScale => _useOwnStartScale;
#if UNITY_EDITOR

            [ShowIf("@_transform != null")]
            [Button("Дублировать")]
            [GUIColor("@Color.green")]
            [HorizontalGroup("h1")]
            private void SetCurrent()
            {
                _localeScale = _transform.localScale;
            }
#endif
            public ObjectsScalesData Clone()
            {
                var clone = new ObjectsScalesData()
                {
                    _transform = _transform,
                    _localeScale = _localeScale,
                    _duration = _duration,
                    _smoothChange = _smoothChange,
                    _curve = _curve
                };
                return clone;
            }
        }
    }
}