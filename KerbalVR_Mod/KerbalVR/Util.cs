using System.Diagnostics;

namespace FirstPersonKerbal
{
	internal static class Util
	{
		#region Logging
		[Conditional("DEBUG")]
		public static void LogMessage(string msg)
		{
			UnityEngine.Debug.LogFormat("[FirstPersonKerbal] {0}", msg);
		}

		[Conditional("DEBUG")]
		public static void LogWarning(string msg)
		{
			UnityEngine.Debug.LogWarningFormat("[FirstPersonKerbal] {0}", msg);
		}

		public static void LogError(string msg)
		{
			UnityEngine.Debug.LogErrorFormat("[FirstPersonKerbal] {0}", msg);
		}
		#endregion
	}
}