using System;
using System.IO;
using Newtonsoft.Json;

namespace RtMap
{
    public class AppConfig
    {
        public ServerSettings Server { get; set; } = new ServerSettings();
        public UosSettings Uos { get; set; } = new UosSettings();
        public MonitorSettings Monitor { get; set; } = new MonitorSettings();
        public ToolPathSettings Paths { get; set; } = new ToolPathSettings();

        private static AppConfig _instance;
        private static readonly object _lock = new object();
        public static string ConfigFolder => @"D:\RtMap";
        public static string ConfigPath => Path.Combine(ConfigFolder, "AppSettings.json");

        public static AppConfig Instance
        {
            get
            {
                if (_instance == null) Load();
                return _instance;
            }
        }

        public static void Load()
        {
            lock (_lock)
            {
                if (File.Exists(ConfigPath))
                {
                    try
                    {
                        string json = File.ReadAllText(ConfigPath);
                        _instance = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                    }
                    catch
                    {
                        _instance = new AppConfig();
                    }
                }
                else
                {
                    _instance = new AppConfig();
                    Save(); // 首次运行自动导出默认JSON模板
                }
            }
        }

        public static void Reload() => Load();

        public static void Save()
        {
            lock (_lock)
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
        }
    }

    public class ServerSettings
    {
        public string Host { get; set; } = "183.6.74.124";
        public int Port { get; set; } = 26422;
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "Zkyt@2023";
    }

    public class UosSettings
    {
        public string ApiUrl { get; set; } = "http://183.6.74.124:48585/gateway/geoai-uos-service/nest-api/v2/szlt/domBuildComplete";
        public string PathPrefixToRemove { get; set; } = "/data/docker-shuzilvtu";
    }

    public class MonitorSettings
    {
        public string DefaultRemoteInfoDir { get; set; } = "/data/docker-shuzilvtu/szlt-wd-picture/start/info";
        public string DefaultLocalInfoDir { get; set; } = @"Z:\start\info";
        public string DefaultRemotePictureDir { get; set; } = "/data/docker-shuzilvtu/szlt-wd-picture";
        public string DefaultRemoteInfoDir3D { get; set; } = "/data/docker-shuzilvtu/szlt-wd-picture/start/info_3d";
        public string DefaultLocalInfoDir3D { get; set; } = @"Z:\start\info_3d";
    }

    public class ToolPathSettings
    {
        public string VideoSamplerExe { get; set; } = @"D:\software\video_samp\video_sampler.exe";
        public string OtFRunnerExe { get; set; } = @"D:\MSVCProj\sfm_onthefly\x64\Release\OtFRunner.exe";
        public string PythonExe { get; set; } = @"D:\software\anaconda\python.exe";
        public string Gdal2TilesPy { get; set; } = @"D:\software\anaconda\Scripts\gdal2tiles.py";
        public string RtScenExe { get; set; } = @"D:\RtScen\x64\Release\RtScen.exe";
        public string Obj2TilesExe { get; set; } = @"D:\RtScen\x64\Release\Obj2Tiles.exe";
    }
}
