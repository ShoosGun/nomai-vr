﻿using NomaiVR.ModConfig;

namespace NomaiVR
{
    public enum MessageType
    {
        Message,
        Info,
        Success,
        Warning,
        Error,
    }

    public static class Logs
    {
        public static void Write(string message, MessageType messageType = MessageType.Message, bool debugOnly = true)
        {
            if (debugOnly && !ModSettings.DebugMode) return;
            switch (messageType)
            {
                case MessageType.Error:
                    UnityEngine.Debug.LogError(message);
                    break;

                case MessageType.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;

                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
        }

        public static void WriteInfo(string message)
        {
            Write(message, MessageType.Info);
        }

        public static void WriteSuccess(string message)
        {
            Write(message, MessageType.Success);
        }

        public static void WriteWarning(string message)
        {
            Write(message, MessageType.Warning);
        }

        public static void WriteError(string message)
        {
            Write(message, MessageType.Error, false);
        }
    }
}
