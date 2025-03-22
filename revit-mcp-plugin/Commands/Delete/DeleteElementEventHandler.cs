using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using revit_mcp_plugin.Commands.Interfaces;
using System;
using System.Threading;

namespace revit_mcp_plugin.Commands.Delete
{
    public class DeleteElementEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        // 执行结果
        public bool IsSuccess { get; private set; }

        // 状态同步对象
        public bool TaskCompleted { get; private set; }
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        // 要删除的元素ID
        public string ElementId { get; set; }

        // 实现IWaitableExternalEventHandler接口
        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument.Document;

                // 将字符串ID转换为ElementId
                if (ElementId != null && int.TryParse(ElementId, out int elementIdValue))
                {
                    var elementId = new ElementId(elementIdValue);

                    // 查找元素
                    var element = doc.GetElement(elementId);
                    if (element != null)
                    {
                        // 删除元素
                        using (var transaction = new Transaction(doc, "Delete Element"))
                        {
                            transaction.Start();
                            doc.Delete(elementId);
                            transaction.Commit();
                        }

                        IsSuccess = true;
                    }
                    else
                    {
                        TaskDialog.Show("Error", "未找到指定的元素");
                        IsSuccess = false;
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "无效的元素ID");
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "删除元素失败: " + ex.Message);
                IsSuccess = false;
            }
            finally
            {
                TaskCompleted = true;
                _resetEvent.Set();
            }
        }

        public string GetName()
        {
            return "删除元素";
        }
    }
}