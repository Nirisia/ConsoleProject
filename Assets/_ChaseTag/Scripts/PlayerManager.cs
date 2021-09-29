///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 29/09/2021 18:05
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class PlayerManager : MonoBehaviour {
	
		public static PlayerManager Instance { get; private set; }
		
		private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
	}
}
