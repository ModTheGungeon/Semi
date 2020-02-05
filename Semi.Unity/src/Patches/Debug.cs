using System;
using System.Runtime.CompilerServices;
using MonoMod;
using UnityEngine;
using Logger = ModTheGungeon.Logger;
using Object = UnityEngine.Object;

namespace Semi.Patches {
    [MonoModPatch("UnityEngine.Debug")]
    internal class UnityEngineDebug {
        public static Logger Logger = new Logger("Gungeon");
        public static Logger.Subscriber UnityEngineDebugSubscriber;
        public static Logger.Subscriber[] UnityEngineSubscriberBlacklist;

        public extern static void orig_cctor();

        [MonoModOriginalName("orig_cctor")]
        [MonoModConstructor]
        public static void cctor() {
            orig_cctor();
            UnityEngineDebugSubscriber = (logger, loglevel, indent, str) => {
                var lstr = logger.String(loglevel, str, indent);
                switch (loglevel) {
                case Logger.LogLevel.Debug: orig_Log(lstr); break;
                case Logger.LogLevel.Info: orig_Log(lstr); break;
                case Logger.LogLevel.Warn: orig_Log(lstr); break;
                case Logger.LogLevel.Error: orig_LogError(lstr); break;
                }
            };
            UnityEngineSubscriberBlacklist = new Logger.Subscriber[1] { UnityEngineDebugSubscriber };
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.stackTraceLogType = StackTraceLogType.None;
            Logger.WriteConsoleDefault = false;
            Logger.Subscribe(UnityEngineDebugSubscriber);
        }

        private static void LogOrig(string text) {
            // gungeon is pretty spammy
            // we'll treat one liners as debug, but more than that as info
            if (text.IndexOf('\n') == -1) {
                orig_Log(Logger.String(Logger.LogLevel.Debug, text));
                Logger.NotifySubscribers(Logger.LogLevel.Debug, false, text, subscriber_blacklist: UnityEngineSubscriberBlacklist);
            } else {
                var split = text.Split('\n');
                for (var i = 0; i < split.Length; i++) {
                    orig_Log(Logger.String(Logger.LogLevel.Info, split[i], i > 0));
                    Logger.NotifySubscribers(Logger.LogLevel.Info, i > 0, split[i], subscriber_blacklist: UnityEngineSubscriberBlacklist);
                }
            }
        }

        public extern static void orig_Log(object message);
        public static void Log(object message) {
            LogOrig(message.ToString());
        }

        public static void Log(object message, Object context) {
            LogOrig($"{message}, context: {context}");
        }

        public static void LogFormat(string format, params object[] args) {
            LogOrig(string.Format(format, args));
        }

        public static void LogFormat(Object context, string format, params object[] args) {
            LogOrig($"{string.Format(format, args)}, context: {context}");
        }

        private static void LogErrorOrig(string text) {
            if (text.IndexOf('\n') == -1) {
                orig_LogError(Logger.String(Logger.LogLevel.Error, text));
                Logger.NotifySubscribers(Logger.LogLevel.Error, false, text, subscriber_blacklist: UnityEngineSubscriberBlacklist);
            }  else {
                var split = text.Split('\n');
                for (var i = 0; i < split.Length; i++) {
                    orig_LogError(Logger.String(Logger.LogLevel.Error, split[i], i > 0));
                    Logger.NotifySubscribers(Logger.LogLevel.Error, i > 0, split[i], subscriber_blacklist: UnityEngineSubscriberBlacklist);
                }
            }
        }

        public extern static void orig_LogError(object message);
        public static void LogError(object message) {
            LogErrorOrig(message.ToString());
        }

        public static void LogError(object message, Object context) {
            LogErrorOrig($"{message}, context: {context}");
        }

        public static void LogErrorFormat(string format, params object[] args) {
            LogErrorOrig(string.Format(format, args));
        }

        public static void LogErrorFormat(Object context, string format, params object[] args) {
            LogErrorOrig($"{string.Format(format, args)}, context: {context}");
        }

        public static void LogException(Exception exception) {
            LogErrorOrig($"Exception: {exception}");
        }

        public static void LogException(Exception exception, Object context) {
            LogErrorOrig($"Exception: {exception}; context: {context}");
        }

        private static void LogWarningOrig(string text) {
            if (text.IndexOf('\n') == -1) {
                orig_Log(Logger.String(Logger.LogLevel.Warn, text));
                Logger.NotifySubscribers(Logger.LogLevel.Warn, false, text, subscriber_blacklist: UnityEngineSubscriberBlacklist);
            } else {
                var split = text.Split('\n');
                for (var i = 0; i < split.Length; i++) {
                    orig_Log(Logger.String(Logger.LogLevel.Warn, split[i], i > 0));
                    Logger.NotifySubscribers(Logger.LogLevel.Warn, i > 0, split[i], subscriber_blacklist: UnityEngineSubscriberBlacklist);
                }
            }
        }

        public extern static void orig_LogWarning(object message);
        public static void LogWarning(object message) {
            LogWarningOrig(message.ToString());
        }

        public static void LogWarning(object message, Object context) {
            LogWarningOrig($"{message}, context: {context}");
        }

        public static void LogWarningFormat(string format, params object[] args) {
            LogWarningOrig(string.Format(format, args));
        }

        public static void LogWarningFormat(Object context, string format, params object[] args) {
            LogWarningOrig($"{string.Format(format, args)}, context: {context}");
        }
    }
}