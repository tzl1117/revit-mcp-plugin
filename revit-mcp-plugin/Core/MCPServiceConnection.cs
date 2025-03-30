using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace revit_mcp_plugin.Core
{
    [Transaction(TransactionMode.Manual)]
    public class MCPServiceConnection : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //// 初始化版本适配器
                //RevitVersionAdapter.Initialize(commandData.Application);

                //// 获取程序集所在目录，用于配置文件路径
                //string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                //string configDir = Path.GetDirectoryName(assemblyLocation);
                //string configPath = Path.Combine(configDir, "commandsets.json");

                // 获取socket服务
                SocketService service = SocketService.Instance;

                if (service.IsRunning)
                {
                    service.Stop();
                    TaskDialog.Show("revitMCP", "Close Server");
                }
                else
                {
                    service.Initialize(commandData.Application);
                    service.Start();
                    TaskDialog.Show("revitMCP", "Open Server");
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
