using System;
using Drone.API.Dig.Common;

namespace Drone.API.Dig.Common.Extensions
{
	public static class ExceptionExtensions
	{		
		public static void LogError(Exception e, string method, string additionalMessage = null)
		{
			LogException(e, method, "Error", additionalMessage);
		}

		public static void LogWarning(Exception e, string sourceMethod, string additionalMessage = null)
		{
			LogException(e, sourceMethod, "Warning", additionalMessage);
		}

		public static void LogInformation(string sourceMethod, string additionalMessage)
		{
			LogException(new Exception("WinDig Information"), sourceMethod, "Information", additionalMessage);
		}

		private static void LogException(Exception e, string sourceMethod, string type, string additionalMessage = null)
		{
			//log to file
			Utility.WriteToLogFile(String.Format("WinDig_{0:M_d_yyyy}", DateTime.Today) + ".log", string.Format("[{0}]-[{1}]-[{4}]-{2}- Additional Details: {3}"
                                                                                                                , DateTime.Now.ToString()
                                                                                                                , type
                                                                                                                , e.Message
                                                                                                                , additionalMessage
                                                                                                                , sourceMethod));
		}
	}
}
