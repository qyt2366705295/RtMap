using System;
using System.IO;
using Newtonsoft.Json.Linq;
using RtMap;

// 独立控制台检查，引用构建后的 RtMap.exe 和 Newtonsoft.Json.dll。
// 测试数据只写在传入的隔离目录，不启动界面、外部工具或网络连接。
internal static class ConfigChecks
{
    private static int checks;

    private static void Assert(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        checks++;
        Console.WriteLine("PASS " + description);
    }

    private static void Invalid(string path, string json, string expected)
    {
        File.WriteAllText(path, json);
        try
        {
            AppConfig.Reload();
            throw new Exception("Invalid configuration was accepted: " + expected);
        }
        catch (InvalidDataException ex)
        {
            Assert(ex.Message.Contains(path) && ex.Message.Contains(expected), "Configuration error: " + expected);
        }
    }

    private static int Main(string[] args)
    {
        try
        {
            string fixture = Path.GetFullPath(args[0]);
            string root = Path.GetFullPath(args[1]);
            string path = Path.Combine(root, "AppSettings.json");
            var json = JObject.Parse(File.ReadAllText(fixture));
            json["Paths"]["StateDirectory"] = "state";
            json["Paths"]["ImageRoot"] = "images";
            json["Paths"]["PythonExe"] = @"%RTMAP_CONFIG_CHECK_ROOT%\python.exe";
            Environment.SetEnvironmentVariable("RTMAP_CONFIG_CHECK_ROOT", root);
            File.WriteAllText(path, json.ToString());

            // 测试程序位于 root/child/bin；运行目录可以与配置目录完全不同。
            Directory.SetCurrentDirectory(Path.GetPathRoot(root));
            Assert(AppConfig.ConfigPath == path, "Find parent config independently of working directory");
            Assert(AppConfig.FindConfigPath(root) == path, "Find config in the starting directory");
            var nested = Path.Combine(root, "child", "AppSettings.json");
            File.WriteAllText(nested, "{}");
            Assert(AppConfig.FindConfigPath(Path.Combine(root, "child", "bin")) == nested, "Nearest config wins");
            File.Delete(nested);

            AppConfig.Load();
            Assert(AppConfig.Instance.Paths.ImageRoot == Path.Combine(root, "images"), "Relative paths use config directory");
            Assert(AppConfig.Instance.Paths.PythonExe == Path.Combine(root, "python.exe"), "Expand path environment variables");
            Assert(Path.IsPathRooted(AppConfig.Instance.Paths.Mip2DConfig), "Expand APPDATA paths");
            Assert(AppConfig.Instance.Paths.VideoSamplerExe == (string)json["Paths"]["VideoSamplerExe"], "Absolute tool paths preserved");
            Assert(AppConfig.Instance.Wsl.ConvertWindowsPath(@"E:\中文 data\image.jpg") == "/mnt/e/中文 data/image.jpg", "WSL conversion uses configured mount root");
            AppConfig.Instance.Wsl.MountRoot = "/drives";
            Assert(AppConfig.Instance.Wsl.ConvertWindowsPath(@"D:\data") == "/drives/d/data", "Custom WSL mount root");

            TaskBridge.SaveOrUpdateTask(new TaskInfo { exec_id = "check", local_result_path = "one" });
            Assert(File.Exists(Path.Combine(root, "state", "active_tasks.json")), "Task state uses configured directory");
            TaskBridge.SaveOrUpdateTask(new TaskInfo { exec_id = "check", local_result_path = "two" });
            Assert(TaskBridge.GetAllTasks().Count == 1 && TaskBridge.GetAllTasks()[0].local_result_path == "two", "Task update persists");
            TaskBridge.RemoveTask("check");
            Assert(TaskBridge.GetAllTasks().Count == 0, "Task removal persists");

            Invalid(path, "{", "配置");
            Invalid(path, "null", "null");
            var bad = (JObject)json.DeepClone();
            bad.Remove("Paths");
            Invalid(path, bad.ToString(), "Paths");
            bad = (JObject)json.DeepClone();
            bad["Paths"]["PythonExe"] = " ";
            Invalid(path, bad.ToString(), "Paths.PythonExe");
            bad = (JObject)json.DeepClone();
            bad["Monitor"]["DefaultLocalInfoDir"] = null;
            Invalid(path, bad.ToString(), "Monitor.DefaultLocalInfoDir");
            bad = (JObject)json.DeepClone();
            bad["Server"]["Port"] = 0;
            Invalid(path, bad.ToString(), "Server.Port");
            bad = (JObject)json.DeepClone();
            bad["Uos"]["ApiUrl"] = "not-a-url";
            Invalid(path, bad.ToString(), "Uos.ApiUrl");

            json["Server"]["Password"] = "";
            json["Uos"]["PathPrefixToRemove"] = "";
            File.WriteAllText(path, json.ToString());
            AppConfig.Reload();
            Assert(AppConfig.Instance.Server.Password == "" && AppConfig.Instance.Uos.PathPrefixToRemove == "", "Explicit empty password and prefix accepted");
            AppConfig.Save();
            AppConfig.Reload();
            Assert(AppConfig.ConfigPath == path && AppConfig.Instance.Paths.ImageRoot == Path.Combine(root, "images"), "Save and reload the selected config");

            File.Delete(path);
            try
            {
                AppConfig.Reload();
                throw new Exception("Missing file was accepted");
            }
            catch (InvalidDataException ex)
            {
                Assert(ex.Message.Contains(path) && !File.Exists(path), "Missing config is reported and never auto-created");
            }
            Console.WriteLine("All " + checks + " configuration checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
