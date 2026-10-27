using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RtMap
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                AppConfig.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RtMap 配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new RtMap());
        }
    }
}
