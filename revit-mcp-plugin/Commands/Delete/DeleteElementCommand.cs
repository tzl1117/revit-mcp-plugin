using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Commands.Base;
using System;

namespace revit_mcp_plugin.Commands.Delete
{
    /// <summary>
    /// 删除指定元素的命令
    /// </summary>
    public class DeleteElementCommand : ExternalEventCommandBase
    {
        private DeleteElementEventHandler _handler => (DeleteElementEventHandler)Handler;

        public override string CommandName => "delete_element";

        public DeleteElementCommand(UIApplication uiApp)
            : base(new DeleteElementEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            try
            {
                // 解析参数
                string elementId = parameters?["elementId"]?.Value<string>();

                if (string.IsNullOrEmpty(elementId))
                {
                    throw new ArgumentException("元素ID不能为空");
                }

                // 设置要删除的元素ID
                _handler.ElementId = elementId;

                // 触发外部事件并等待完成
                if (RaiseAndWaitForCompletion(15000))
                {
                    return new { success = _handler.IsSuccess };
                }
                else
                {
                    throw new TimeoutException("删除元素超时");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"删除元素失败: {ex.Message}");
            }
        }
    }
}