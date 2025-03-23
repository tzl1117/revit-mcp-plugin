using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace revit_mcp_plugin.Models
{
    /// <summary>
    /// 壳形构件 (Shell Components)
    /// </summary>
    public class ShellComponent
    {
        /// <summary>
        /// 构件类型名称
        /// </summary>
        
        public string Name { get; set; }
        /// <summary>
        /// 类型Id
        /// </summary>
        public int TypeId { get; set; } = -1;
        /// <summary>
        /// 壳形轮廓边界
        /// </summary>
        public JZFace Boundary { get; set; }
        /// <summary>
        /// 厚度
        /// </summary>
        public double Thickness { get; set; }
        /// <summary>
        /// 底部标高
        /// </summary>
        public double BaseLevel { get; set; }
        /// <summary>
        /// 底部偏移
        /// </summary>
        public double BaseOffset { get; set; }
        /// <summary>
        /// 参数化属性
        /// </summary>
        public Dictionary<string, double> Parameters { get; set; }

        public ShellComponent()
        {
            Parameters = new Dictionary<string, double>();
        }

    }
}
