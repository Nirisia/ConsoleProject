///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 07/09/2021 14:02
///-----------------------------------------------------------------

using System;
using UnityEngine;

namespace Com.IsartDigital.Common {
    /// <summary>
    /// By default, creates a timer of 1 second. Use Init method to Set limitDuration as well as timeScale.
    /// </summary>
	public class Timer : MonoBehaviour {

        public event Action OnTimerCompleted;

        private float limitDuration = 1f;
        private float timeScale = 1f;

        private float elapsedTime = 0f;

        public float ElapsedTime => elapsedTime;
        public float InvertedElapsedTime => limitDuration - elapsedTime;

        public float Ratio => ElapsedTime / limitDuration;
        public float InvertedRatio => 1f - Ratio;

        public void SetTimeScale(float timeScale = 1f) => this.timeScale = timeScale;
        public void Pause() => SetTimeScale(0);

        public void Init(float limitDuration, float timeScale = 1f)
        {
            this.limitDuration = limitDuration;
            SetTimeScale(timeScale);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime * timeScale;

            if (elapsedTime >= limitDuration)
            {
                OnTimerCompleted?.Invoke();
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            OnTimerCompleted = null;
        }
    }
}
