using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.API.Models
{
    /// <summary>
    /// 命令执行结果
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 错误消息，如果有的话
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static CommandResult CreateSuccess(object data = null)
        {
            return new CommandResult
            {
                Success = true,
                Data = data,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static CommandResult CreateError(string errorMessage, object data = null)
        {
            return new CommandResult
            {
                Success = false,
                Data = data,
                ErrorMessage = errorMessage
            };
        }
    }
}
