using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.API.Interfaces
{
    /// <summary>
    /// 命令注册接口
    /// </summary>
    public interface ICommandRegistry
    {
        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="command">要注册的命令</param>
        void RegisterCommand(IRevitCommand command);
        /// <summary>
        /// 尝试获取命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <param name="command">找到的命令</param>
        /// <returns>是否找到命令</returns>
        bool TryGetCommand(string commandName, out IRevitCommand command);
    }
}
