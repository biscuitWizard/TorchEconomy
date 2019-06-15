using System;
using System.IO;
using System.Reflection;

namespace TorchEconomy.Resources
{
    public class SQLiteInstaller
    {
        public static void CheckSQLiteInstalled()
        {
            var filePath = Environment.CurrentDirectory;
            var fileName = "";
            var platformVersion = "";
            
            var os = Environment.OSVersion;
            var pid = os.Platform;
            switch (pid) 
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    fileName = "sqlite3.dll";
                    platformVersion = "Windows";
                    break;
                case PlatformID.Unix:
                    fileName = "sqlite3";
                    platformVersion = "Linux";
                    break;
                case PlatformID.MacOSX:
                    fileName = "sqlite3";
                    platformVersion = "Mac";
                    break;
            }

            if (!File.Exists(Path.Combine(filePath, fileName)))
            {
                using (Stream stream = Assembly
                    .GetAssembly(typeof(EconomyPlugin))
                    .GetManifestResourceStream($"TorchEconomy.Resources.{platformVersion}.{fileName}"))
                {
                    using (var fileStream = File.Open(Path.Combine(filePath, fileName), FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                }
            }
        }
        
        public static bool IsLinux
        {
            get
            {
                int p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}