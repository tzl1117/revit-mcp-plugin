using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Models
{
    /// <summary>
    /// 基于点的构件 (Point-Based Components)
    /// </summary>
    public class PointBasedComponent
    {
        /// <summary>
        /// 构件类型名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 定位点坐标
        /// </summary>
        public JZPoint LocationPoint { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
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

        public PointBasedComponent()
        {
            Parameters = new Dictionary<string, double>();
        }
    }
}
