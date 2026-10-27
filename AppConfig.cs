using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RtMap
{
    public class AppConfig
    {
        public ServerSettings Server { get; set; }
        public UosSettings Uos { get; set; }
        public MonitorSettings Monitor { get; set; }
        public ToolPathSettings Paths { get; set; }
        public WslSettings Wsl { get; set; }

        private static AppConfig _instance;
        private static string _configPath;
        private static readonly object _lock = new object();
        private const string ConfigFileName = "AppSettings.json";

        // 从 EXE 所在目录向上搜索，不依赖启动时的工作目录。
        public static string ConfigPath
        {
            get
            {
                lock (_lock)
                {
                    if (_configPath == null) _configPath = FindConfigPath(AppDomain.CurrentDomain.BaseDirectory);
                    return _configPath;
                }
            }
        }

        public static string ConfigFolder => Path.GetDirectoryName(ConfigPath);

        public static string FindConfigPath(string startDirectory)
        {
            for (var dir = new DirectoryInfo(startDirectory); dir != null; dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, ConfigFileName);
                if (File.Exists(candidate)) return candidate;
            }

            throw new FileNotFoundException(
                $"未找到 {ConfigFileName}。已从程序目录 {startDirectory} 逐级向上查找。\n" +
                "请将完整的本机配置放在 RtMap 文件夹的同级目录；可参考 config/AppSettings.example.json。");
        }

        public static AppConfig Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) Load();
                    return _instance;
                }
            }
        }

        public static void Load()
        {
            lock (_lock)
            {
                string path = ConfigPath;
                try
                {
                    var config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(path));
                    if (config == null) throw new InvalidDataException("配置内容不能为空或 null。");
                    config.Validate();
                    config.ResolveLocalPaths(Path.GetDirectoryName(path));
                    _instance = config;
                }
                catch (Exception ex) when (ex is JsonException || ex is InvalidDataException || ex is IOException ||
                    ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
                {
                    throw new InvalidDataException($"无法加载配置文件：{path}\n{ex.Message}", ex);
                }
            }
        }

        public static void Reload() => Load();

        public static void Save()
        {
            lock (_lock)
            {
                if (_instance == null) throw new InvalidOperationException("尚未成功加载配置，无法保存。");
                _instance.Validate();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_instance, Formatting.Indented));
            }
        }

        private void Validate()
        {
            var errors = new List<string>();
            ValidateSection(Server, nameof(Server), errors);
            ValidateSection(Uos, nameof(Uos), errors);
            ValidateSection(Monitor, nameof(Monitor), errors);
            ValidateSection(Paths, nameof(Paths), errors);
            ValidateSection(Wsl, nameof(Wsl), errors);
            if (Server != null && (Server.Port < 1 || Server.Port > 65535)) errors.Add("Server.Port 必须为 1–65535。");
            if (Uos != null && (!Uri.TryCreate(Uos.ApiUrl, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))) errors.Add("Uos.ApiUrl 必须是 HTTP/HTTPS 地址。");
            if (Wsl != null && !string.IsNullOrWhiteSpace(Wsl.MountRoot) && !Wsl.MountRoot.StartsWith("/"))
                errors.Add("Wsl.MountRoot 必须为 Linux 绝对路径。");
            if (Wsl != null && !string.IsNullOrWhiteSpace(Wsl.CodeDirectory) && !Wsl.CodeDirectory.StartsWith("/"))
                errors.Add("Wsl.CodeDirectory 必须为 Linux 绝对路径。");
            if (errors.Count > 0) throw new InvalidDataException(string.Join("\n", errors));
        }

        private static void ValidateSection(object section, string name, List<string> errors)
        {
            if (section == null)
            {
                errors.Add($"缺少配置节 {name}。");
                return;
            }
            foreach (var property in section.GetType().GetProperties())
            {
                if (property.PropertyType != typeof(string)) continue;
                string value = (string)property.GetValue(section);
                // 空前缀表示不剥离回调路径前缀；空密码允许无密码的服务器。
                bool allowEmpty = name == nameof(Uos) && property.Name == nameof(UosSettings.PathPrefixToRemove) ||
                    name == nameof(Server) && property.Name == nameof(ServerSettings.Password);
                if (value == null || (!allowEmpty && string.IsNullOrWhiteSpace(value)))
                    errors.Add($"{name}.{property.Name} 未填写。");
            }
        }

        private void ResolveLocalPaths(string configDirectory)
        {
            // Paths 均为本机路径，支持环境变量；相对路径以配置文件目录为基准。
            foreach (var property in typeof(ToolPathSettings).GetProperties())
                property.SetValue(Paths, ResolveLocalPath((string)property.GetValue(Paths), configDirectory));
            Monitor.DefaultLocalInfoDir = ResolveLocalPath(Monitor.DefaultLocalInfoDir, configDirectory);
            Monitor.DefaultLocalInfoDir3D = ResolveLocalPath(Monitor.DefaultLocalInfoDir3D, configDirectory);
        }

        private static string ResolveLocalPath(string value, string configDirectory)
        {
            string expanded = Environment.ExpandEnvironmentVariables(value);
            return Path.GetFullPath(Path.IsPathRooted(expanded) ? expanded : Path.Combine(configDirectory, expanded));
        }
    }

    public class ServerSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UosSettings
    {
        public string ApiUrl { get; set; }
        public string PathPrefixToRemove { get; set; }
    }

    public class MonitorSettings
    {
        public string DefaultRemoteInfoDir { get; set; }
        public string DefaultLocalInfoDir { get; set; }
        public string DefaultRemotePictureDir { get; set; }
        public string DefaultRemoteInfoDir3D { get; set; }
        public string DefaultLocalInfoDir3D { get; set; }
    }

    public class ToolPathSettings
    {
        public string VideoSamplerExe { get; set; }
        public string OtFRunnerExe { get; set; }
        public string PythonExe { get; set; }
        public string Gdal2TilesPy { get; set; }
        public string RtScenExe { get; set; }
        public string Obj2TilesExe { get; set; }
        public string ViewerExe { get; set; }
        public string SfmUiExe { get; set; }
        public string Mip2DExe { get; set; }
        public string Mip3DExe { get; set; }
        public string Mip2DConfig { get; set; }
        public string Mip3DConfig { get; set; }
        public string LasModuleDirectoryA { get; set; }
        public string LasModuleDirectoryB { get; set; }
        public string WslExe { get; set; }
        public string CameraParametersRoot { get; set; }
        public string CameraMapFile { get; set; }
        public string ImageRoot { get; set; }
        public string SfmWorkspaceRoot { get; set; }
        public string TileCacheRoot { get; set; }
        public string PointCloudRoot { get; set; }
        public string Workspace3DRoot { get; set; }
        public string StateDirectory { get; set; }
        public string LocalInputRoot { get; set; }
        public string SharedOutputRoot { get; set; }
    }

    public class WslSettings
    {
        public string Distribution { get; set; }
        public string CodeDirectory { get; set; }
        public string PythonCommand { get; set; }
        public string MountRoot { get; set; }

        public string ConvertWindowsPath(string winPath)
        {
            if (string.IsNullOrWhiteSpace(winPath)) return string.Empty;
            if (winPath.Length < 3 || !char.IsLetter(winPath[0]) || winPath[1] != ':' ||
                (winPath[2] != '\\' && winPath[2] != '/'))
                throw new ArgumentException("WSL 路径转换需要带盘符的 Windows 绝对路径：" + winPath);
            return MountRoot.TrimEnd('/') + "/" + char.ToLowerInvariant(winPath[0]) + "/" +
                winPath.Substring(3).Replace("\\", "/");
        }
    }
}
