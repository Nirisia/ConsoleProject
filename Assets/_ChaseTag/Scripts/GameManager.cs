///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 02/10/2021 15:52
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class GameManager : MonoBehaviour {
	
		public static GameManager Instance { get; private set; }
		
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
