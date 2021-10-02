
using UnityEngine;
///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 30/09/2021 21:58
///-----------------------------------------------------------------
namespace Com.IsartDigital.ChaseTag.Common {
	public static class RandomTool {
		
		public static Vector3 insideUniteCube => new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		
		/// <summary>
		/// returns a random 3d position inside a cube
		/// </summary>
		/// <param name="start">the angle with min x, y & z coordinates possible in the cube</param>
		/// <param name="end">the angle with max x, y & z coordinates possible in the cube</param>
		/// <returns></returns>
		public static Vector3 GetRandomPositionInCube(Vector3 start, Vector3 end)
        {
			return new Vector3(Random.Range(start.x, end.x), Random.Range(start.y, end.y), Random.Range(start.z, end.z));
        }

	}
}
