using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.StateSwitcher
{
    /// <summary>
    /// Режим перемещения
    /// </summary>
    public class PositionSwitch : StateItem
    {
        [LabelText("Позиции объектов")] [SerializeField]
        private List<ObjectsPositionsData> _list;

        public List<ObjectsPositionsData> List => _list;

        public override void Set()
        {
            foreach (var objectsPositionsData in _list)
            {
                if (objectsPositionsData.SmoothChange)
                {
                    objectsPositionsData.Transform.DOLocalMove(objectsPositionsData.Coords,
                        objectsPositionsData.Duration).SetEase(objectsPositionsData.Ease);
                }
                else
                {
                    if (objectsPositionsData.Local)
                        objectsPositionsData.Transform.localPosition = objectsPositionsData.Coords;
                    else
                        objectsPositionsData.Transform.position = objectsPositionsData.Coords;
                }
            }
        }

        public override void DefaultState()
        {
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var clone = new PositionSwitch();
            var list = new List<ObjectsPositionsData>();
            clone._list = list;
            foreach (var objectsPositionsData in _list)
            {
                list.Add(objectsPositionsData.Clone());
            }

            return clone;
        }

        public override string DropDownGroupName => "Objects";
        public override string DropDownItemName => "Switch Positions";

#endif

        [Serializable]
        public class ObjectsPositionsData
        {
            [SerializeField] [HideLabel] [HorizontalGroup("h1")]
            private Transform _transform;

            [SerializeField] [HideLabel] [HorizontalGroup("h1")] [FormerlySerializedAs("_localePosition")]
            private Vector3 _coords;

            [ShowIf("@_transform != null")] [SerializeField][LabelWidth(40f)]
            private bool _local;


            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private bool _smoothChange;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private float _duration;

            [ToggleGroup("_smoothChange", "Smooth change")] [SerializeField]
            private Ease _ease = Ease.Linear;

            public Ease Ease => _ease;
            public bool SmoothChange => _smoothChange;

            public float Duration => _duration;
            public bool Local => _local;

            public Transform Transform => _transform;

            public Vector3 Coords => _coords;
#if UNITY_EDITOR
            [ShowIf("@_transform != null")]
            [Button("Дублировать")]
            [GUIColor("@Color.green")]
            [HorizontalGroup("h1")]
            private void SetCurrent()
            {
                _coords = _local ? _transform.localPosition : _transform.position;
            }
#endif
            public ObjectsPositionsData Clone()
            {
                return new ObjectsPositionsData()
                {
                    _coords = _coords,
                    _transform = _transform,
                    _ease = _ease,
                    _smoothChange = _smoothChange,
                    _duration = _duration
                };
            }
        }
    }
}