using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace revit_mcp_plugin.Utils
{
    public static class ProjectUtils
    {
        /// <summary>
        /// 创建或获取指定高度的标高
        /// </summary>
        /// <param name="doc">revit文档</param>
        /// <param name="elevation">标高高度（ft）</param>
        /// <param name="levelName">标高名称</param>
        /// <returns></returns>
        public static Level CreateOrGetLevel(this Document doc, double elevation, string levelName)
        {
            // 先查找是否存在指定高度的标高
            Level existingLevel = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(l => Math.Abs(l.Elevation - elevation) < 0.1 / 304.8);

            if (existingLevel != null)
                return existingLevel;

            // 创建新标高
            Level newLevel = Level.Create(doc, elevation);
            // 设置标高名称
            Level namesakeLevel = new FilteredElementCollector(doc)
                 .OfClass(typeof(Level))
                 .Cast<Level>()
                 .FirstOrDefault(l => l.Name == levelName);
            if (namesakeLevel != null)
            {
                levelName = $"{levelName}_{newLevel.Id.IntegerValue.ToString()}";
            }
            newLevel.Name = levelName;

            return newLevel;
        }

        /// <summary>
        /// 刷新视图并添加延迟
        /// </summary>
        public static void Refresh(this Document doc, int waitingTime = 0, bool allowOperation = true)
        {
            UIApplication uiApp = new UIApplication(doc.Application);
            UIDocument uiDoc = uiApp.ActiveUIDocument;

            // 检查文档是否可修改
            if (uiDoc.Document.IsModifiable)
            {
                // 更新模型
                uiDoc.Document.Regenerate();
            }
            // 更新界面
            uiDoc.RefreshActiveView();

            // 延迟等待
            if (waitingTime != 0)
            {
                System.Threading.Thread.Sleep(waitingTime);
            }

            // 允许用户进行非安全操作
            if (allowOperation)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        /// <summary>
        /// 将指定的消息保存到桌面的指定文件中（默认覆盖文件）
        /// </summary>
        /// <param name="message">要保存的消息内容</param>
        /// <param name="fileName">目标文件名</param>
        public static void SaveToDesktop(this string message, string fileName = "temp.json", bool isAppend = false)
        {
            // 确保 logName 包含后缀
            if (!Path.HasExtension(fileName))
            {
                fileName += ".txt"; // 默认添加 .txt 后缀
            }

            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            // 组合完整的文件路径
            string filePath = Path.Combine(desktopPath, fileName);

            // 写入文件（覆盖模式）
            using (StreamWriter sw = new StreamWriter(filePath, isAppend))
            {
                sw.WriteLine($"{message}");
            }
        }


    }
}
