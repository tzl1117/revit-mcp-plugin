using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 类型Id
        /// </summary>
        [JsonProperty("typeId")]
        public int TypeId { get; set; } = -1;
        /// <summary>
        /// 定位点坐标
        /// </summary>
        [JsonProperty("locationPoint")]
        public JZPoint LocationPoint { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; set; } = -1;
        /// <summary>
        /// 深度
        /// </summary>
        [JsonProperty("depth")]
        public double Depth { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; set; }
        /// <summary>
        /// 底部标高
        /// </summary>
        [JsonProperty("baseLevel")]
        public double BaseLevel { get; set; }
        /// <summary>
        /// 底部偏移
        /// </summary>
        [JsonProperty("baseOffset")]
        public double BaseOffset { get; set; }
        /// <summary>
        /// 参数化属性
        /// </summary>
        [JsonProperty("parameters")]
        public Dictionary<string, double> Parameters { get; set; }

        public PointBasedComponent()
        {
            Parameters = new Dictionary<string, double>();
        }
    }
}
