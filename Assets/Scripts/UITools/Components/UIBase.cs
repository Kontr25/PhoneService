using System;
using UnityEngine;

namespace UI.Components
{
    public class UIBase: MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        
        protected virtual void Open()
        {
            _content.SetActive(true);
        }
        
        protected virtual void Close()
        {
            _content.SetActive(false);
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void OnDestroy()
        {
            
        }
    }
}