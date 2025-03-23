using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Commands.Create;
using revit_mcp_plugin.Models;
using revit_mcp_plugin.Utils;

namespace revit_mcp_plugin.Test
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        private UIApplication uiApp;
        private UIDocument uiDoc => uiApp.ActiveUIDocument;
        private Document doc => uiDoc.Document;
        private Autodesk.Revit.ApplicationServices.Application app => uiApp.Application;

        // Revit外部事件对象，用于在主线程中触发操作
        private ExternalEvent _externalEvent;
        // 方案执行器
        private CreateFloorEventHandler _executeHandler;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiApp = commandData.Application;
            List<ShellComponent> createdFloorInfo = new List<ShellComponent>();
            createdFloorInfo.Add(new ShellComponent
            {
                Name = "楼板",
                TypeId = 0,
                Boundary = new JZFace()
                {
                    OuterLoop = new List<JZLine>
                    {
                        new JZLine(0,0,10000,0),
                        new JZLine(10000,0,10000,10000),
                        new JZLine(10000,10000,0,10000),
                        new JZLine(0,10000,0,0),
                    }
                },
                Thickness = 250,
                BaseLevel = 0,
                BaseOffset = 0,
            });

            try
            {
                TcpClient client = new TcpClient("localhost", 8080);
                NetworkStream stream = client.GetStream();

                // 创建符合JSON-RPC 2.0协议的墙体创建请求
                var json = new
                {
                    jsonrpc = "2.0",
                    method = "createFloor",
                    paramss = new { Data = createdFloorInfo },
                    id = 1
                };

                string jsonRpcRequest = JsonConvert.SerializeObject(json).Replace("paramss", "params");
                jsonRpcRequest.SaveToDesktop("createdFloorInfo.json");


                System.Diagnostics.Trace.WriteLine("发送请求: " + jsonRpcRequest);

                // 发送命令
                byte[] data = Encoding.UTF8.GetBytes(jsonRpcRequest);
                stream.Write(data, 0, data.Length);

                // 接收响应
                data = new byte[4096];
                int bytes = stream.Read(data, 0, data.Length);
                string response = Encoding.UTF8.GetString(data, 0, bytes);

                System.Diagnostics.Trace.WriteLine("服务器响应: " + response);

                // 关闭客户端
                client.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("错误: " + e.Message);
            }


            //// 设置墙体参数
            //_externalEvent = ExternalEvent.Create(_executeHandler);
            //_executeHandler.SetFloorParameters(createdFloorInfo);
            //_externalEvent.Raise();

            //CreateFloorEventHandler createFloorEventHandler = new CreateFloorEventHandler();
            //createFloorEventHandler.SetFloorParameters(createdFloorInfo);
            //createFloorEventHandler.Execute(uiApp);

            return Result.Succeeded;
        }
    }
}
