///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:19
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	[CreateAssetMenu(menuName = "Prototype/PlayerSpecs")]
	public class PlayerSpecs: ScriptableObject {

		[SerializeField] private float normalSpeed = 1f;
		[SerializeField] private float catSpeed = 1f;
		[SerializeField] private float mouseSpeed = 1f;

		public float NormalSpeed => normalSpeed;
		public float CatSpeed => catSpeed;
		public float MouseSpeed => mouseSpeed;
	}
}
