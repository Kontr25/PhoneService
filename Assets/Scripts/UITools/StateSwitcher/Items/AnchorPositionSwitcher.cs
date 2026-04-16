using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.StateSwitcher
{
    /// <summary>
    /// Режим перемещения
    /// </summary>
    public class AnchorPositionSwitcher : StateItem
    {
        [LabelText("Позиции объектов")] [SerializeField]
        private List<ObjectsAnchorData> _list;

        public List<ObjectsAnchorData> List => _list;

        public override void Set()
        {
            foreach (var objectsAnchorData in _list)
            {
                objectsAnchorData.RectTransform.anchoredPosition = objectsAnchorData.AnchorPos;
            }
        }

        public override void DefaultState()
        {
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            var clone = new AnchorPositionSwitcher();
            var list = new List<ObjectsAnchorData>();
            clone._list = list;
            foreach (var objectsAnchorData in _list)
            {
                list.Add(objectsAnchorData.Clone());
            }

            return clone;
        }

        public override string DropDownGroupName => "Objects";
        public override string DropDownItemName => "Switch Anchor Positions";

#endif

        [Serializable]
        public class ObjectsAnchorData
        {
            [FormerlySerializedAs("rectTransform")]
            [FormerlySerializedAs("_transform")]
            [SerializeField]
            [HideLabel]
            [HorizontalGroup("h1")]
            private RectTransform _rectTransform;

            [SerializeField] [HideLabel] [HorizontalGroup("h1")]
            private Vector3 _anchorPos;

            public RectTransform RectTransform => _rectTransform;

            public Vector3 AnchorPos => _anchorPos;

#if UNITY_EDITOR
            [ShowIf("@_rectTransform != null")]
            [HorizontalGroup("h1")]
            [Button("Дублировать")]
            [GUIColor("@Color.green")]
            private void SetCurrent()
            {
                _anchorPos = _rectTransform.anchoredPosition;
            }

            public ObjectsAnchorData Clone()
            {
                return new ObjectsAnchorData()
                {
                    _anchorPos = _anchorPos,
                    _rectTransform = _rectTransform
                };
            }
#endif
        }
    }
}