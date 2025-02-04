﻿namespace NomaiVR.Helpers
{
    public static class ToolHelper
    {
        public static ToolModeSwapper Swapper => Locator.GetToolModeSwapper();

        public static bool IsUsingAnyTool()
        {
            if (Swapper == null)
            {
                return false;
            }

            return Swapper.IsInToolMode(ToolMode.Probe) || Swapper.IsInToolMode(ToolMode.Translator) || Swapper.IsInToolMode(ToolMode.SignalScope);
        }

        public static bool IsUsingAnyTool(ToolGroup group)
        {
            if (Swapper == null)
            {
                return false;
            }

            return Swapper.IsInToolMode(ToolMode.Probe, group) || Swapper.IsInToolMode(ToolMode.Translator, group) || Swapper.IsInToolMode(ToolMode.SignalScope, group);
        }

        public static bool IsUsingNoTools()
        {
            if (Swapper == null)
            {
                return true;
            }

            return Swapper.IsInToolMode(ToolMode.None) || Swapper.IsInToolMode(ToolMode.Item);
        }

        public static bool IsInToolMode(ToolMode mode, ToolGroup group)
        {
            return Swapper && Swapper.IsInToolMode(mode, group);
        }

        public static bool IsInToolMode(ToolMode mode)
        {
            return Swapper && Swapper.IsInToolMode(mode);
        }
    }
}
