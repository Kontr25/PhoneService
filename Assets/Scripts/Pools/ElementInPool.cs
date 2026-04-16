using UnityEngine;

namespace Pools
{
    /// <summary>
    /// Базовый элемент, управляемый пулом.
    /// </summary>
    public abstract class ElementInPool : MonoBehaviour
    {
        /// <summary>
        /// Пул, который владеет текущим элементом.
        /// </summary>
        private Pool _ownerPool;

        /// <summary>
        /// Текущее состояние активности элемента.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Возвращает признак того, что элемент активен в сцене.
        /// </summary>
        /// <returns>Признак активности элемента.</returns>
        public bool IsActive()
        {
            return _isActive;
        }

        /// <summary>
        /// Привязывает элемент к конкретному пулу.
        /// </summary>
        /// <param name="ownerPool">Владелец элемента.</param>
        internal void SetOwnerPool(Pool ownerPool)
        {
            _ownerPool = ownerPool;
        }

        /// <summary>
        /// Активирует элемент после получения из пула.
        /// </summary>
        internal void ActivateFromPool()
        {
            SetActive(true);
            OnRent();
        }

        /// <summary>
        /// Деактивирует элемент перед возвратом в пул.
        /// </summary>
        internal void DeactivateToPool()
        {
            OnReturn();
            SetActive(false);
        }

        /// <summary>
        /// Вызывается после получения элемента из пула.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Вызывается перед возвратом элемента в пул.
        /// </summary>
        public virtual void DeInit()
        {
        }

        /// <summary>
        /// Вызов пользовательской инициализации при выдаче из пула.
        /// </summary>
        protected virtual void OnRent()
        {
            Init();
        }

        /// <summary>
        /// Вызов пользовательской деинициализации при возврате в пул.
        /// </summary>
        protected virtual void OnReturn()
        {
            DeInit();
        }

        /// <summary>
        /// Возвращает элемент в свой пул.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Элемент не привязан к пулу.</exception>
        public void Remove()
        {
            if (_ownerPool == null)
            {
                throw new System.InvalidOperationException();
            }

            _ownerPool.Release(this);
        }

        /// <summary>
        /// Выставляет активность объекта и внутреннего флага.
        /// </summary>
        /// <param name="mode">Требуемое состояние активности.</param>
        private void SetActive(bool mode)
        {
            _isActive = mode;
            gameObject.SetActive(mode);
        }

        /// <summary>
        /// Выставляет позицию объекта среди соседних трансформов.
        /// </summary>
        /// <param name="position">Индекс позиции в иерархии.</param>
        public void SetPosition(int position)
        {
            transform.SetSiblingIndex(position);
        }
    }
}