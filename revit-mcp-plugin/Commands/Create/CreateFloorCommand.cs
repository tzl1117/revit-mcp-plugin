using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Commands.Base;
using revit_mcp_plugin.Commands.Interfaces;
using revit_mcp_plugin.Commands.Wall;
using revit_mcp_plugin.Models;

namespace revit_mcp_plugin.Commands.Create
{
    public class CreateFloorCommand : ExternalEventCommandBase
    {
        private CreateFloorEventHandler _handler => (CreateFloorEventHandler)Handler;

        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "create_surface_based_element";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiApp">Revit UIApplication</param>
        public CreateFloorCommand(UIApplication uiApp)
            : base(new CreateFloorEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            try
            {
                List<ShellComponent> data = new List<ShellComponent>();
                // 解析参数
                data = parameters["Data"].ToObject<List<ShellComponent>>();
                if (data == null)
                    throw new Exception("创建楼板操作超时");

                // 设置墙体参数
                _handler.SetFloorParameters(data);

                // 触发外部事件并等待完成
                if (RaiseAndWaitForCompletion(10000))
                {
                    return _handler.CreatedFloorInfo;
                }
                else
                {
                    throw new TimeoutException("创建楼板操作超时");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"创建楼板失败: {ex.Message}");
            }
        }
    }
}
