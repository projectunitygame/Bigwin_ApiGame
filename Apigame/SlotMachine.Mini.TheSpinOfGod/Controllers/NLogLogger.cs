using System;
using System.Diagnostics;
using System.Text;
using NLog;
using NLog.Targets;

namespace MinigameVuabai.SignalR.Controllers
{
    public static class NLogLogger
    {


        /// <summary>
        /// 
        /// </summary>
        volatile static Logger _log;

        private static Logger Logger
        {
            get
            {

                if (_log == null)
                {
                    try
                    {
                        if (LogManager.Configuration != null && LogManager.Configuration.LoggingRules.Count > 0)
                        {
                            _log = LogManager.GetCurrentClassLogger();

                            return _log;
                        }

                    }
                    catch { }

                    FileTarget target = new FileTarget();
                    target.Layout = "${longdate} ${logger} ${message}";
                    target.FileName = "${basedir}/_LOG/${date:format=yyyyMMdd}_SSO.txt";
                    target.ArchiveFileName = "${basedir}/archives/${date:format=yyyyMMdd}_SSO_log.txt";
                    target.ArchiveAboveSize = 1024 * 1024 * 100; // archive files greater than 10 KB
                    //    target.ArchiveNumbering = FileTarget.ArchiveNumberingMode.Sequence;
                    target.Name = "Debug";
                    // this speeds up things when no other processes are writing to the file
                    target.ConcurrentWrites = true;

                    NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

                    FileTarget warning = new FileTarget();
                    warning.Layout = "${longdate} ${logger} ${message}";
                    warning.FileName = "${basedir}/_LOG/${date:format=yyyyMMdd}_SSO.txt";
                    warning.ArchiveFileName = "${basedir}/archives/${date:format=yyyyMMdd}_SSO_log.txt";
                    warning.ArchiveAboveSize = 1024 * 1024 * 100; // archive files greater than 10 KB
                    //   warning.ArchiveNumbering = FileTarget.ArchiveNumberingMode.Sequence;
                    warning.Name = "Warning";
                    // this speeds up things when no other processes are writing to the file
                    warning.ConcurrentWrites = true;
                    NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(warning, LogLevel.Warn);

                    _log = LogManager.GetLogger("eBank");
                }

                return _log;

            }
        }

        public static void LogInfo(string message)
        {
            Info(message, false);
        }

        public static void Info(string message, bool sendMail)
        {
            var mes = GetCalleeString() + Environment.NewLine + "\t" + message;
            Logger.Info(":\t" + mes);

        }

        public static void Info(string message)
        {
            Info(message, false);
        }


        public static void TraceMessage(string message)
        {
            Logger.Trace("\t" + message);
        }
        public static void PublishException(Exception ex, bool sendmail)
        {
            DebugMessage(ex.Message + Environment.NewLine + ex.StackTrace, sendmail);

        }
        public static void PublishException(Exception ex)
        {
            PublishException(ex, true);
        }

        public static void DebugMessage(object o)
        {
            DebugMessage(GetValueOfObject(o));
        }

        public static void DebugMessage(string message, bool sendEmail)
        {
            var m = GetCalleeString() + Environment.NewLine + "\t" + message;
            Logger.Info(":\t" + m);
        }

        public static void DebugMessage(string message)
        {
            DebugMessage(message, false);
        }

        public static void LogDebug(string p)
        {
            DebugMessage(p);
        }

        public static void LogWarning(object o)
        {
            LogWarning(GetValueOfObject(o));
        }

        public static void LogWarning(string message, bool sendMail)
        {
            var error = GetCalleeString() + Environment.NewLine + "\t" + message;

            Logger.Warn(":\t" + error);
        }

        public static void LogWarning(string message)
        {
            LogWarning(message, true);
        }

        public static void Fatal(string message)
        {
            Logger.Fatal(":\t" + GetCalleeString() + Environment.NewLine + "\t" + message);
        }

        private static string GetCalleeString()
        {
            foreach (var sf in new StackTrace().GetFrames())
            {
                if (sf.GetMethod().ReflectedType.Namespace != "PhatLoc.Utility")
                {
                    return string.Format("{0}.{1} ", sf.GetMethod().ReflectedType.Name, sf.GetMethod().Name);
                }
            }

            return string.Empty;
        }


        public static string GetValueOfObject(object ob)
        {
            var sb = new StringBuilder();
            try
            {
                foreach (System.Reflection.PropertyInfo piOrig in ob.GetType().GetProperties())
                {
                    object editedVal = ob.GetType().GetProperty(piOrig.Name).GetValue(ob, null);
                    sb.AppendFormat("{0}:{1}\t ", piOrig.Name, editedVal);

                }
            }
            catch { }
            return sb.ToString();
        }
    }
}