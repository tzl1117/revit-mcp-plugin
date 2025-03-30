using Newtonsoft.Json.Linq;

namespace revit_mcp_plugin.API.Interfaces
{
    /// <summary>
    /// 所有 Revit 命令必须实现的接口
    /// </summary>
    public interface IRevitCommand
    {
        /// <summary>
        /// 命令的唯一名称，用于在 JSON-RPC 请求中识别命令
        /// </summary>
        string CommandName { get; }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameters">JSON-RPC 参数</param>
        /// <param name="requestId">请求 ID</param>
        /// <returns>命令执行结果</returns>
        object Execute(JObject parameters, string requestId);
    }
}
