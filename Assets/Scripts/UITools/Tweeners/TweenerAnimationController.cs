using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Tweeners
{
    [RequireComponent(typeof(Animator))]
    public class TweenerAnimationController : TweenerBase
    {
        [InfoBox("Для выбора ключа сделайте активным объект", InfoMessageType.Warning, "$IsNotActive")]
        [SerializeField]
        [ValueDropdown("$GetAnimatorKeys")]
        private string _activeKey = "";

        [SerializeField] private Animator _animator;

        [SerializeField] private UnityEvent beforeTween;

        public override void Forward(bool skip = false)
        {
            beforeTween?.Invoke();
            _animator.SetBool(_activeKey, true);
            _animator.SetTrigger(_activeKey);
        }

        public override void Back(bool skip = false)
        {
            beforeTween?.Invoke();
            _animator.SetBool(_activeKey, false);
        }

#if UNITY_EDITOR
        private bool IsNotActive => !gameObject.activeSelf;

        private void OnValidate()
        {
            if (_animator == null)
                _animator = transform.GetComponent<Animator>();
        }

        private List<string> GetAnimatorKeys()
        {
            var result = new List<string>();
            if (IsNotActive)
                return result;
            foreach (var param in _animator.parameters)
                if (param.type == AnimatorControllerParameterType.Bool
                    || param.type == AnimatorControllerParameterType.Trigger)
                    result.Add(param.name);
            return result;
        }
#endif
    }
}