///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:19
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	[CreateAssetMenu(menuName = "Prototype/PlayerSpecs")]
	public class PlayerSpecs: ScriptableObject {

		[Header("Speed")]
		[SerializeField] private float normalSpeed = 1f;
		[SerializeField] private float catSpeed = 1f;
		[SerializeField] private float mouseSpeed = 1f;

		public float NormalSpeed => normalSpeed;
		public float CatSpeed => catSpeed;
		public float MouseSpeed => mouseSpeed;

		[Header("Drag")]
		[SerializeField] private float normalDrag = 1f;
		[SerializeField] private float catDrag = 1f;
		[SerializeField] private float mouseDrag = 1f;

		public float NormalDrag => normalDrag;
		public float CatDrag => catDrag;
		public float MouseDrag => mouseDrag;

		[Header("Dash")]
		[SerializeField] private float normalDash = 5f;
		[SerializeField] private float catDash = 5f;
		[SerializeField] private float mouseDash = 5f;

		public float NormalDash => normalDash;
		public float CatDash => catDash;
		public float MouseDash => mouseDash;
	}
}
