using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.Components
{
    /// <summary>
    /// Auto-height с поддержкой иерархии пересчёта:
    /// - Статические дети задаются вручную, хранятся как есть.
    /// - Динамические дети не задаются вручную — только логика.
    /// - Если waitForChildren == true → компонент подписывается к родителю
    ///   и ждёт всех детей перед пересчётом.
    /// - Пересчёт всегда идёт снизу вверх.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AutoHeightSmart : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private LayoutGroup _layoutGroup;

        [Header("Auto-height logic")] [SerializeField]
        private bool autoUpdate = true;

        [Tooltip("Если включено — этот узел будет ждать своих детей и подписываться к родителю.")] [SerializeField]
        private bool waitForChildren = false;

        [Header("Static Children (ручной список)")] [SerializeField]
        private List<AutoHeightSmart> staticChildren = new();

        [Header("Dynamic Children (runtime, read only)")]
        [NonSerialized]
        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowPaging = false, DraggableItems = false)]
        private readonly List<AutoHeightSmart> dynamicChildren = new();

        private AutoHeightSmart _parent;
        private bool _scheduled;
        private int _pendingChildren = 0;
        private bool _isRuntimeRebuildSubscribed;

        /// <summary>
        /// При включении планирует пересборку высоты.
        /// </summary>
        private void OnEnable()
        {
            EnsureReferences();
            ScheduleRebuild();
        }

        /// <summary>
        /// Сбрасывает запланированные editor-callback при отключении компонента.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeRuntimeRebuild();
#if UNITY_EDITOR
            EditorApplication.delayCall -= RebuildDelayed;
#endif
            _scheduled = false;
        }

        /// <summary>
        /// При старте настраивает связи в иерархии AutoHeightSmart.
        /// </summary>
        private void Start()
        {
            SetupHierarchyLinks();
        }

        /// <summary>
        /// При изменении списка дочерних объектов перепривязывает иерархию и планирует пересчёт.
        /// </summary>
        private void OnTransformChildrenChanged()
        {
            SetupHierarchyLinks();
            ScheduleRebuild();
        }

        /// <summary>
        /// При смене родителя обновляет связи и планирует пересчёт.
        /// </summary>
        private void OnTransformParentChanged()
        {
            SetupHierarchyLinks();
            ScheduleRebuild();
        }

        /// <summary>
        /// В редакторе очищает статических детей, обновляет иерархию и планирует пересборку.
        /// </summary>
        private void OnValidate()
        {
            EnsureReferences();
            CleanupStaticChildren();
            SetupHierarchyLinks();
            ScheduleRebuild();
        }

        /// <summary>
        /// Очищает динамических детей, статических от null, находит родителя и при необходимости регистрируется у него.
        /// </summary>
        private void SetupHierarchyLinks()
        {
            for (int i = dynamicChildren.Count - 1; i >= 0; i--)
            {
                if (dynamicChildren[i] == null)
                    dynamicChildren.RemoveAt(i);
            }

            dynamicChildren.Clear();
            CleanupStaticChildren();

            _parent = transform.parent != null
                ? transform.parent.GetComponent<AutoHeightSmart>()
                : null;

            if (waitForChildren && _parent != null)
            {
                _parent.TryRegisterChild(this);
            }
        }

        /// <summary>
        /// Удаляет null из списка статических детей.
        /// </summary>
        private void CleanupStaticChildren()
        {
            for (int i = staticChildren.Count - 1; i >= 0; i--)
            {
                if (staticChildren[i] == null)
                    staticChildren.RemoveAt(i);
            }
        }

        /// <summary>
        /// Регистрирует ребёнка у родителя. Если родитель не ждёт детей — вызов игнорируется.
        /// </summary>
        private void TryRegisterChild(AutoHeightSmart child)
        {
            if (!waitForChildren)
                return;

            if (!dynamicChildren.Contains(child))
                dynamicChildren.Add(child);
        }

        /// <summary>
        /// Уведомляет родителя о готовности этого узла.
        /// </summary>
        private void NotifyParentReady()
        {
            if (_parent != null)
                _parent.OnChildReady();
        }

        /// <summary>
        /// Вызывается родителем при готовности ребёнка; при готовности всех детей пересчитывает и уведомляет своего родителя.
        /// </summary>
        private void OnChildReady()
        {
            _pendingChildren--;

            if (_pendingChildren <= 0)
            {
                RebuildNow();
                NotifyParentReady();
            }
        }

        /// <summary>
        /// Готовит пересборку: при отсутствии детей пересчитывает сразу и уведомляет родителя, иначе ждёт отчёт от всех детей.
        /// </summary>
        private void PrepareToRebuild()
        {
            var totalChildren = staticChildren.Count + dynamicChildren.Count;

            if (totalChildren == 0 && !waitForChildren)
            {
                RebuildNow();
                NotifyParentReady();
                return;
            }

            _pendingChildren = totalChildren;

            if (_pendingChildren == 0)
            {
                RebuildNow();
                NotifyParentReady();
            }
        }

        /// <summary>
        /// Планирует отложенную пересборку: в игре через одноразовую подписку на Canvas, в редакторе через delayCall.
        /// </summary>
        private void ScheduleRebuild()
        {
            if (!autoUpdate)
                return;

            if (_scheduled)
                return;

            _scheduled = true;

            if (Application.isPlaying)
            {
                SubscribeRuntimeRebuild();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall -= RebuildDelayed;
                EditorApplication.delayCall += RebuildDelayed;
#endif
            }
        }

        /// <summary>
        /// Выполняет отложенную пересборку: сбрасывает флаг и запускает подготовку.
        /// </summary>
        private void RebuildDelayed()
        {
            if (!_scheduled)
            {
                return;
            }

            _scheduled = false;
            UnsubscribeRuntimeRebuild();
#if UNITY_EDITOR
            EditorApplication.delayCall -= RebuildDelayed;
#endif
            PrepareToRebuild();
        }

        /// <summary>
        /// Пересчитывает высоту и принудительно перестраивает layout.
        /// </summary>
        private void RebuildNow()
        {
            EnsureReferences();
            if (!TryCalculateHeight(out float totalHeight))
            {
                ScheduleRebuild();
                return;
            }

            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }

        /// <summary>
        /// Принудительно запускает перерасчёт снизу вверх: сначала у всех дочерних AutoHeightSmart в иерархии _rect, затем у этого компонента.
        /// Вызов с корневого элемента обеспечивает порядок: листья → их родители → корень.
        /// </summary>
        public void RebuildRecursive()
        {
            if (_rect != null)
            {
                for (int i = 0; i < _rect.childCount; i++)
                {
                    var child = _rect.GetChild(i);
                    if (child.TryGetComponent<AutoHeightSmart>(out var childSmart))
                        childSmart.RebuildRecursive();
                }
            }

            RebuildNow();
        }

        /// <summary>
        /// Пытается вычислить итоговую высоту через стандартные утилиты layout-системы Unity.
        /// </summary>
        /// <param name="totalHeight">Итоговая высота контейнера.</param>
        /// <returns>TRUE, если высота вычислена корректно; иначе FALSE.</returns>
        private bool TryCalculateHeight(out float totalHeight)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
            totalHeight = LayoutUtility.GetPreferredHeight(_rect);

            int activeChildrenCount = 0;
            for (int i = 0; i < _rect.childCount; i++)
            {
                if (_rect.GetChild(i).gameObject.activeSelf)
                {
                    activeChildrenCount++;
                }
            }

            if (activeChildrenCount > 0 && Mathf.Approximately(totalHeight, 0f))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Подписывает компонент на одноразовый runtime-callback перестроения.
        /// </summary>
        private void SubscribeRuntimeRebuild()
        {
            if (_isRuntimeRebuildSubscribed)
            {
                return;
            }

            Canvas.willRenderCanvases += RebuildDelayed;
            _isRuntimeRebuildSubscribed = true;
        }

        /// <summary>
        /// Отписывает компонент от runtime-callback перестроения.
        /// </summary>
        private void UnsubscribeRuntimeRebuild()
        {
            if (!_isRuntimeRebuildSubscribed)
            {
                return;
            }

            Canvas.willRenderCanvases -= RebuildDelayed;
            _isRuntimeRebuildSubscribed = false;
        }

        /// <summary>
        /// Проверяет и кеширует обязательные зависимости компонента.
        /// </summary>
        private void EnsureReferences()
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }

            if (_layoutGroup == null)
            {
                _layoutGroup = GetComponent<LayoutGroup>();
            }

            if (_rect == null || _layoutGroup == null)
            {
                throw new InvalidOperationException();
            }
        }
    }
}