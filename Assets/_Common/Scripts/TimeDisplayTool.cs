///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 08/10/2021 12:37
///-----------------------------------------------------------------

using System.Text;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag.Common {
	public static class TimeDisplayTool {
		
		public static string DisplayTime(float timeInSeconds, int precision = 0)
        {
			StringBuilder stringBuilder = new StringBuilder();

			int seconds = Mathf.FloorToInt(timeInSeconds);
			float rest = timeInSeconds - seconds;

			stringBuilder.Append(seconds.ToString());

			if (precision > 0)
            {
				stringBuilder.Append('.');

				if (rest.ToString().Length > 2)
                {
					stringBuilder.Append(rest.ToString().Substring(2, precision));
                }
				else
                {
					stringBuilder.Append('0');
                }
            }

			return stringBuilder.ToString();
		}
	}
}
