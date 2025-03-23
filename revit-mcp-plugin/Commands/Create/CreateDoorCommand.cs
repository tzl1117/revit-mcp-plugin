using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Commands.Base;
using revit_mcp_plugin.Models;

namespace revit_mcp_plugin.Commands.Create
{
    public class CreateDoorCommand : ExternalEventCommandBase
    {
        private CreateDoorEventHandler _handler => (CreateDoorEventHandler)Handler;

        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "create_point_based_element";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiApp">Revit UIApplication</param>
        public CreateDoorCommand(UIApplication uiApp)
            : base(new CreateDoorEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            try
            {
                List<PointBasedComponent> data = new List<PointBasedComponent>();
                // 解析参数
                data = parameters["data"].ToObject<List<PointBasedComponent>>();
                if (data == null)
                    throw new Exception("创建门操作超时");

                // 设置墙体参数
                _handler.SetParameters(data);

                // 触发外部事件并等待完成
                if (RaiseAndWaitForCompletion(10000))
                {
                    return _handler.CreatedInfo;
                }
                else
                {
                    throw new TimeoutException("创建门操作超时");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"创建门失败: {ex.Message}");
            }
        }
    }
}
