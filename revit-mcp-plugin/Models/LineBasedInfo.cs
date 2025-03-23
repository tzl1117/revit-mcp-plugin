using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Models
{
    /// <summary>
    /// 基于线的构件 (Line-Based Components)
    /// </summary>
    public class LineBasedComponent
    {
        /// <summary>
        /// 构件类型名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 路径曲线
        /// </summary>
        public JZLine LocationLine { get; set; }
        /// <summary>
        /// 厚度
        /// </summary>
        public double Thickness { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }
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

        public LineBasedComponent()
        {
            Parameters = new Dictionary<string, double>();
        }
    }
}
