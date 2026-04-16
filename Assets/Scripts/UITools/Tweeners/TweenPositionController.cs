using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Tweeners
{
    public class TweenPositionController : TweenerBase
    {
        [OnValueChanged("FeelData", true)]
        [field: SerializeField]
        public TweenData[] Animations { get; private set; } = Array.Empty<TweenData>();

        private TweenerCore<Vector3, Vector3[], Vector3ArrayOptions> tweenCore;

        /// <summary>
        /// Запуск алгоритма анимации
        /// </summary>
        protected void Run(Vector3[] data, float[] durations)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                transform.localPosition = data[^1];
                return;
            }
#endif
            tweenCore = DOTween.ToArray(() => transform.localPosition, pos => transform.localPosition = pos, data,
                durations);
        }

        /// <summary>
        /// Массив данных для анимации
        /// </summary>
        private Vector3[] data;

        /// <summary>
        /// Реверсивный массив данных для анимации
        /// </summary>
        private Vector3[] dataR;

        /// <summary>
        /// Массив данных временных отметок для анимации
        /// </summary>
        private float[] timers;

        /// <summary>
        /// Реверсивный массив данных временных отметок для анимации
        /// </summary>
        private float[] timersR;

        /// <summary>
        /// Массив данных для анимации
        /// </summary>
        protected Vector3[] Data
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    data = null;
#endif
                if (data != null)
                    return data;
                FeelData();
                return data;
            }
        }

        /// <summary>
        /// Реверсивный массив данных для анимации
        /// </summary>
        protected Vector3[] DataR
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    dataR = null;
#endif
                if (dataR != null)
                    return dataR;
                FeelData();
                return dataR;
            }
        }

        /// <summary>
        /// Массив данных временных отметок для анимации
        /// </summary>
        protected float[] Timers
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    timers = null;
#endif
                if (timers != null)
                    return timers;
                FeelData();
                return timers;
            }
        }

        /// <summary>
        /// Реверсивный массив данных временных отметок для анимации
        /// </summary>
        protected float[] TimersR
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    timersR = null;
#endif
                if (timersR != null)
                    return timersR;
                FeelData();
                return timersR;
            }
        }

        private void FeelData()
        {
            data = new Vector3[Animations.Length];
            dataR = new Vector3[Animations.Length];
            timers = new float[Animations.Length];
            timersR = new float[Animations.Length];
            for (var i = 0; i < Animations.Length; i++)
            {
                data[i] = Animations[i].Data;
                timers[i] = Animations[i].Duration;
                dataR[^(i + 1)] = Animations[i].Data;
                timersR[^(i + 1)] = Animations[i].Duration;
            }
        }

        /// <summary>
        /// Анимация в прямом направлении
        /// </summary>
        /// <param name="skip">Мгновенный переход</param>
        public override void Forward(bool skip)
        {
            if (!gameObject.activeInHierarchy)
                skip = true;

            tweenCore.Kill();
            if (skip)
            {
                transform.localPosition = DataR[0];
                return;
            }

            Run(Data, Timers);
        }

        /// <summary>
        /// Анимация в обратном направлении
        /// </summary>
        /// <param name="skip">Мгновенный переход</param>
        public override void Back(bool skip)
        {
            if (!gameObject.activeInHierarchy)
                skip = true;

            tweenCore.Kill();
            if (skip)
            {
                transform.localPosition = Data[0];
                return;
            }

            Run(DataR, TimersR);
        }
    }

    [Serializable]
    public class TweenData
    {
        [field: SerializeField] public Vector3 Data { get; private set; }

        [field: SerializeField, Range(0.1f, 2f)]
        public float Duration { get; private set; } = 0.1f;
    }
}