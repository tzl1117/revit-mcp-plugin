using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Commands.Interfaces
{
    /// <summary>
    /// 可初始化的命令接口
    /// </summary>
    public interface IRevitCommandInitializable
    {
        /// <summary>
        /// 初始化命令
        /// </summary>
        /// <param name="uiApp">Revit UI应用程序</param>
        void Initialize(UIApplication uiApp);
    }
}
