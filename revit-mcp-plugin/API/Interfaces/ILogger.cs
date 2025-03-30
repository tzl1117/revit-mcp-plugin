using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.API.Interfaces
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        void Log(LogLevel level, string message, params object[] args);

        /// <summary>
        /// 记录调试日志
        /// </summary>
        void Debug(string message, params object[] args);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        void Info(string message, params object[] args);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        void Warning(string message, params object[] args);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        void Error(string message, params object[] args);
    }
}
