using System;
using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace NetDaemon.Extensions.Logging.Internal;

internal static class NetDaemonLoggingThemes
{
    public static class NetDaemonConsoleThemes
    {
        private static SystemConsoleTheme SystemTheme { get; } = new(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new() {Foreground = ConsoleColor.Gray},
                [ConsoleThemeStyle.SecondaryText] = new() {Foreground = ConsoleColor.DarkGray},
                [ConsoleThemeStyle.TertiaryText] = new() {Foreground = ConsoleColor.DarkGray},
                [ConsoleThemeStyle.Invalid] = new() {Foreground = ConsoleColor.Yellow},
                [ConsoleThemeStyle.Null] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.Name] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.String] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.Number] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.Boolean] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.Scalar] = new() {Foreground = ConsoleColor.Green},
                [ConsoleThemeStyle.LevelVerbose] = new() {Foreground = ConsoleColor.Gray},
                [ConsoleThemeStyle.LevelDebug] = new() {Foreground = ConsoleColor.DarkYellow},
                [ConsoleThemeStyle.LevelInformation] =
                    new() {Foreground = ConsoleColor.DarkGreen},
                [ConsoleThemeStyle.LevelWarning] = new() {Foreground = ConsoleColor.Yellow},
                [ConsoleThemeStyle.LevelError] = new() {Foreground = ConsoleColor.Red},
                [ConsoleThemeStyle.LevelFatal] = new() {Foreground = ConsoleColor.DarkRed}
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
            [ConsoleThemeStyle.String] = "\u001b[0;36m"
        });

        public static ConsoleTheme GetThemeByType(string type)
        {
            return string.Equals(type, "system", StringComparison.OrdinalIgnoreCase) ? SystemTheme : AnsiTheme;
        }
    }
}