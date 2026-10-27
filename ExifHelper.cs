using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace RtMap
{
    public static class ExifHelper
    {
        /// 从影像文件中读取EXIF，并匹配外置配置返回相机参数目录名
        /// <param name="imagePath">影像路径</param>
        /// <param name="configPath">可选：指定配置文件路径</param>
        /// <returns>匹配成功返回ParamDir（如"M3M"），失败返回 null</returns>
        public static string GetCameraParamDirFromExif(string imagePath, string configPath = null)
        {
            try
            {
                string ext = Path.GetExtension(imagePath).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".tif" && ext != ".tiff")
                    return null;

                using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image img = Image.FromStream(fs))
                {
                    // 0x0110 = Model, 0x010F = Make
                    var modelProp = img.PropertyItems.FirstOrDefault(p => p.Id == 0x0110);
                    var makeProp = img.PropertyItems.FirstOrDefault(p => p.Id == 0x010F);

                    string model = modelProp != null ? DecodeExifString(modelProp) : null;
                    string make = makeProp != null ? DecodeExifString(makeProp) : null;

                    if (string.IsNullOrEmpty(model)) return null;

                    var config = CameraMapConfig.Load(configPath);
                    var mapping = config.FindMapping(make, model);
                    return mapping?.ParamDir;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EXIF读取失败] {imagePath}：{ex.Message}");
            }
            return null;
        }

        private static string DecodeExifString(PropertyItem prop)
        {
            if (prop.Type == 2)
                return Encoding.ASCII.GetString(prop.Value).TrimEnd('\0').Trim();
            return Encoding.Default.GetString(prop.Value).TrimEnd('\0').Trim();
        }
    }
}
