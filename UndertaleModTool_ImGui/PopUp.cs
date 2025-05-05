using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace UndertaleModTool_ImGui
{
    public static class PopUp
    {
        public static void CreatePopup(string message, string title)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var command = $"msg * {message}";
                ExecuteShellCommandWindows(command);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var escapedMessage = message.Replace("'", "'\"'\"'");
                var command = $"zenity --info --text='{escapedMessage}' --title='{title}'";
                ExecuteShellCommand(command);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var escapedMessage = message.Replace("\"", "\\\"");
                var command = $"osascript -e 'display dialog \"{escapedMessage}\" with title \"{title}\"'";
                ExecuteShellCommand(command);
            }
            else
            {
                Console.WriteLine("Sistema operacional não suportado.");
            }
        }

        static void ExecuteShellCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
        static void ExecuteShellCommandWindows(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
            }
            };
            process.Start();
        }
    }
}