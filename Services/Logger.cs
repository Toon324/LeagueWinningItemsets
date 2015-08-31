using System;
using System.Collections.Generic;
using System.Linq;
namespace LeagueStatisticallyBestItemset.Services
{
    public static class Logger
    {
        /// <summary>
        /// Logs a message to log.txt in the logs Azure Container
        /// </summary>
        /// <param name="callingClass">String indicating what class is logging the message</param>
        /// <param name="msg">Message to log</param>
        public static void LogMessageToFile(string callingClass, string msg)
        {
            try
            {
                var logLine = System.String.Format(
                    "{0:G}: [{2}] - {1}.", System.DateTime.Now, msg,
                    callingClass.Replace("LeagueStatisticallyBestItemset.", ""));

                var container = ApiTools.GetBlobContainer("logs");

                var logBlob = container.GetAppendBlobReference("log.txt");

                if (!logBlob.Exists())
                    logBlob.CreateOrReplace();

                logBlob.AppendText(logLine + "\n");
            }
            catch
            {
                // ignored
            }
        }
    }
}