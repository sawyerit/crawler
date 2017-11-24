using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrawlDaddy.Core.BILoggerService;

namespace CrawlDaddy.Core
{
	public static class ExceptionExtensions
	{
		public static BIException ConvertToBIException(this Exception ex, LogActionEnum logAction, LogTypeEnum logType, string title, string sourceMethod, string user, string server, string additionalMessage = null)
		{
			return new BIException { ApplicationName = "CrawlDaddy", LogAction = logAction, LogType = logType, Message = ex.Message + " - Additional Info: " + additionalMessage, StackTrace = ex.StackTrace, Title = title, User = user, Server = server, Source = sourceMethod };
		}

		public static void LogError(Exception e, string method, string additionalMessage = null)
		{
			LogException(e, method, LogTypeEnum.Error, additionalMessage);
		}

		public static void LogWarning(Exception e, string sourceMethod, string additionalMessage = null)
		{
			LogException(e, sourceMethod, LogTypeEnum.Warning, additionalMessage);
		}

		public static void LogInformation(string sourceMethod, string additionalMessage)
		{
			LogException(new Exception("CrawlDaddy Information"), sourceMethod, LogTypeEnum.Information, additionalMessage);
		}

		private static void LogException(Exception e, string sourceMethod, LogTypeEnum type, string additionalMessage = null)
		{
			using (var logClient = new BILoggerServiceClient())
			{
				logClient.HandleBIExceptionAsync(e.ConvertToBIException(LogActionEnum.LogAndEmail
																				, type
																				, sourceMethod + " Error"
																				, sourceMethod
																				, "nouser"
																				, System.Environment.MachineName
																				, additionalMessage));
			}
		}
	}
}
