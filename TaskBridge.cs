using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace RtMap
{
    public class TaskInfo
    {
        public string action { get; set; }
        public string nest_uuid { get; set; }
        public string exec_id { get; set; }
        public string image_path { get; set; }
        public string result_path { get; set; }
        public long timestamp { get; set; }
        public string local_result_path { get; set; }
        public string dom_path { get; set; }
        public double? centm { get; set; }

        // 覆盖ToString()，这样在下拉框中能直接显示exec_id
        public override string ToString()
        {
            return exec_id;
        }
    }

    public static class TaskBridge
    {
        private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "active_tasks.json");
        private static object _lock = new object(); // 防止多线程同时读写冲突

        // 获取本地记录的所有任务
        public static List<TaskInfo> GetAllTasks()
        {
            lock (_lock)
            {
                if (!File.Exists(dbPath)) return new List<TaskInfo>();
                string json = File.ReadAllText(dbPath);
                return JsonConvert.DeserializeObject<List<TaskInfo>>(json) ?? new List<TaskInfo>();
            }
        }

        // 下传工具开始时，将任务存入本地
        public static void SaveOrUpdateTask(TaskInfo newTask)
        {
            lock (_lock)
            {
                var tasks = GetAllTasks();
                int index = tasks.FindIndex(t => t.exec_id == newTask.exec_id);
                if (index >= 0)
                {
                    tasks[index] = newTask; // 更新
                }
                else
                {
                    tasks.Add(newTask); // 新增
                }
                File.WriteAllText(dbPath, JsonConvert.SerializeObject(tasks, Formatting.Indented));
            }
        }

        // STOP或完成时：将任务移出本地记录
        public static void RemoveTask(string execId)
        {
            lock (_lock)
            {
                var tasks = GetAllTasks();
                int removedCount = tasks.RemoveAll(t => t.exec_id == execId);
                if (removedCount > 0)
                {
                    File.WriteAllText(dbPath, JsonConvert.SerializeObject(tasks, Formatting.Indented));
                }
            }
        }
    }

    public static class UosApiClient
    {
        // 全局复用一个HttpClient，防止频繁请求导致端口耗尽
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task NotifyUosBuildComplete(string execId, string rawResultPath, string tileFolderName)
        {
            try
            {
                var uosCfg = AppConfig.Instance.Uos;
                // 路径清洗
                string cleanPath = rawResultPath.Replace(uosCfg.PathPrefixToRemove, "");
                if (!cleanPath.EndsWith("/")) cleanPath += "/";
                string finalResultPath = cleanPath + tileFolderName;

                // 构建JSON Payload
                var payload = new
                {
                    exec_id = execId,
                    result_path = finalResultPath
                };
                string jsonContent = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 发送POST请求
                //string apiUrl = "http://183.6.74.124:48585/gateway/geoai-uos-service/nest-api/v2/szlt/domBuildComplete";
                HttpResponseMessage response = await httpClient.PostAsync(uosCfg.ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"回调成功：{execId} -> {finalResultPath}");
                }
                else
                {
                    Console.WriteLine($"回调失败：{response.StatusCode}，返回内容：{await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"回调发生错误：{ex.Message}");
            }
        }
    }
}
