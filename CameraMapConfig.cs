using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RtMap
{
    public class CameraMapConfig
    {
        public string Version { get; set; }
        public List<CameraMapping> Mappings { get; set; } = new List<CameraMapping>();

        private static CameraMapConfig _instance;
        private static readonly object _lock = new object();

        // 默认配置文件路径
        public static string DefaultConfigPath => @"D:\testdata\camera_paras\camera_map.json";

        public static CameraMapConfig Load(string configPath = null)
        {
            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                string path = configPath ?? DefaultConfigPath;
                if (!File.Exists(path))
                {
                    // 自动生成模板
                    var template = new CameraMapConfig
                    {
                        Version = "1.0",
                        Mappings = new List<CameraMapping>
                        {
                            new CameraMapping
                            {
                                Id = "M3M",
                                ParamDir = "M3M",
                                MatchRules = new MatchRules
                                {
                                    MakeKeywords = new List<string> { "DJI" },
                                    ModelKeywords = new List<string> { "M3M", "MAVIC 3 MULTISPECTRAL", "M30" }
                                }
                            },
                            new CameraMapping
                            {
                                Id = "P4M",
                                ParamDir = "P4M",
                                MatchRules = new MatchRules
                                {
                                    MakeKeywords = new List<string> { "DJI" },
                                    ModelKeywords = new List<string> { "P4M", "PHANTOM 4 MULTISPECTRAL" }
                                }
                            },
                            new CameraMapping
                            {
                                Id = "S6100_X_40",
                                ParamDir = "S6100_X_40",
                                MatchRules = new MatchRules
                                {
                                    MakeKeywords = new List<string> { "SHARE" },
                                    ModelKeywords = new List<string> { "S6100", "X_" }
                                }
                            }
                        }
                    };
                    string dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(path, JsonConvert.SerializeObject(template, Formatting.Indented));
                    _instance = template;
                    return _instance;
                }

                string json = File.ReadAllText(path);
                _instance = JsonConvert.DeserializeObject<CameraMapConfig>(json);
                return _instance;
            }
        }

        public static void Reload(string configPath = null)
        {
            lock (_lock)
            {
                _instance = null;
                Load(configPath);
            }
        }

        // 根据EXIF的Make和Model查找映射
        public CameraMapping FindMapping(string make, string model)
        {
            if (string.IsNullOrEmpty(model)) return null;

            string rawMake = (make ?? "").ToUpperInvariant();
            string rawModel = model.ToUpperInvariant();

            foreach (var mapping in Mappings)
            {
                if (mapping.MatchRules == null) continue;

                if (mapping.MatchRules.MakeKeywords != null && mapping.MatchRules.MakeKeywords.Count > 0)
                {
                    bool makeHit = mapping.MatchRules.MakeKeywords.Any(k => rawMake.Contains(k.ToUpperInvariant()));
                    if (!makeHit) continue;
                }

                if (mapping.MatchRules.ModelKeywords != null && mapping.MatchRules.ModelKeywords.Count > 0)
                {
                    bool modelHit = mapping.MatchRules.ModelKeywords.Any(k => rawModel.Contains(k.ToUpperInvariant()));
                    if (modelHit) return mapping;
                }
            }
            return null;
        }
    }

    public class CameraMapping
    {
        public string Id { get; set; }
        public string ParamDir { get; set; }
        public MatchRules MatchRules { get; set; }
    }

    public class MatchRules
    {
        public List<string> MakeKeywords { get; set; }
        public List<string> ModelKeywords { get; set; }
        public string Note { get; set; }
    }
}