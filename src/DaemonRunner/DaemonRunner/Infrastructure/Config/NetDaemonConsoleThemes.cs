using System;
using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace NetDaemon.Infrastructure.Config
{
    public static class NetDaemonConsoleThemes
    {
        private static SystemConsoleTheme SystemTheme { get; } = new(new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Gray},
            [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.DarkGray},
            [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.DarkGray},
            [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Yellow},
            [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Green},
            [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Gray},
            [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.DarkYellow},
            [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.DarkGreen},
            [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Yellow},
            [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.Red},
            [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle {Foreground = ConsoleColor.DarkRed},
        });

        private static AnsiConsoleTheme AnsiTheme { get; } = new(new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[38;5;0253m",
            [ConsoleThemeStyle.SecondaryText] = "\x1b[38;5;0246m",
            [ConsoleThemeStyle.TertiaryText] = "\x1b[38;5;0242m",
            [ConsoleThemeStyle.Invalid] = "\x1b[33;1m",
            [ConsoleThemeStyle.Null] = "\x1b[38;5;0038m",
            [ConsoleThemeStyle.Number] = "\x1b[38;5;151m",
            [ConsoleThemeStyle.Boolean] = "\x1b[38;5;0038m",
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[37m",
            [ConsoleThemeStyle.LevelError] = "\u001b[0;31m",
            [ConsoleThemeStyle.Name] = "\u001b[1;34m",
            [ConsoleThemeStyle.LevelInformation] = "\u001b[0;36m",
            [ConsoleThemeStyle.LevelWarning] = "\u001b[1;33m",
            [ConsoleThemeStyle.LevelFatal] = "\u001b[0;31m",
            [ConsoleThemeStyle.LevelDebug] = "\u001b[0;37m",
            [ConsoleThemeStyle.Scalar] = "\u001b[1;34m",
            [ConsoleThemeStyle.String] = "\u001b[0;36m",
        });

        public static ConsoleTheme GetThemeByType(string type)
        {
            return string.Equals(type, "system", StringComparison.InvariantCultureIgnoreCase) ? SystemTheme : AnsiTheme;
        }
    }
}