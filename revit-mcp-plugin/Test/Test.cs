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
using static System.Windows.Forms.AxHost;

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

            try
            {
                TcpClient client = new TcpClient("localhost", 8080);
                NetworkStream stream = client.GetStream();

                // 创建符合JSON-RPC 2.0协议的墙体创建请求
                var json = new
                {
                    jsonrpc = "2.0",
                    method = "create_point_based_element",
                    //method = "create_line_based_element",
                    //method = "create_surface_based_element",
                    paramss = new
                    {
                        data = new List<PointBasedComponent>
                        {
                            new PointBasedComponent
                            {
                                Name = "",
                                TypeId = -1,
                                LocationPoint = new JZPoint(0,0),
                                Width = 500,
                                Height=2100,
                                BaseLevel = 0,
                                BaseOffset = 0,
                            }
                        }

                        //data= new List<LineBasedComponent>
                        //{
                        //    new LineBasedComponent
                        //    {
                        //        Name = "",
                        //        TypeId = 0,
                        //        LocationLine = new JZLine(0,0,5000,0),
                        //        Thickness = 250,
                        //        Height=2000,
                        //        BaseLevel = 0,
                        //        BaseOffset = 0,
                        //    }
                        //}


                        //data = new List<ShellComponent>
                        //{
                        //    new ShellComponent
                        //    {
                        //        Name = "楼板",
                        //        TypeId = 0,
                        //        Boundary = new JZFace()
                        //        {
                        //            OuterLoop = new List<JZLine>
                        //            {
                        //                new JZLine(0,0,10000,0),
                        //                new JZLine(10000,0,10000,10000),
                        //                new JZLine(10000,10000,0,10000),
                        //                new JZLine(0,10000,0,0),
                        //            }
                        //        },
                        //        Thickness = 250,
                        //        BaseLevel = 0,
                        //        BaseOffset = 0,
                        //    }
                        //}
                    },
                    id = 1
                };

                string jsonRpcRequest = JsonConvert.SerializeObject(json).Replace("paramss", "params");
                jsonRpcRequest.SaveToDesktop("createdInfo.json");


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


            //var createdFloorInfo = new List<ShellComponent>
            //{
            //    new ShellComponent
            //    {
            //        Name = "楼板",
            //        TypeId = 0,
            //        Boundary = new JZFace()
            //        {
            //            OuterLoop = new List<JZLine>
            //            {
            //                new JZLine(0,0,10000,0),
            //                new JZLine(10000,0,10000,10000),
            //                new JZLine(10000,10000,0,10000),
            //                new JZLine(0,10000,0,0),
            //            }
            //        },
            //        Thickness = 250,
            //        BaseLevel = 0,
            //        BaseOffset = 0,
            //    }
            //};
            //CreateFloorEventHandler createFloorEventHandler = new CreateFloorEventHandler();
            //createFloorEventHandler.SetParameters(createdFloorInfo);
            //createFloorEventHandler.Execute(uiApp);


            //var createdWallInfo = new List<LineBasedComponent>
            //{
            //    new LineBasedComponent
            //    {
            //        Name = "",
            //        TypeId = 0,
            //        LocationLine = new JZLine(0,0,5000,0),
            //        Thickness = 250,
            //        Height=2000,
            //        BaseLevel = 0,
            //        BaseOffset = 0,
            //    }
            //};
            //CreateWallEventHandler createWallEventHandler = new CreateWallEventHandler();
            //createWallEventHandler.SetParameters(createdWallInfo);
            //createWallEventHandler.Execute(uiApp);

            //double startX=-20;
            //double startY=0; 
            //double endX=20; 
            //double endY=0;
            //double height=10;
            //double thickness=0.3;
            //CreateWallEventHandler createWallEventHandler = new CreateWallEventHandler();
            //createWallEventHandler.SetWallParameters(startX, startY, endX, endY, height, thickness);
            //createWallEventHandler.Execute(uiApp);

            //var createdDoorInfo = new List<PointBasedComponent>
            //{
            //    new PointBasedComponent
            //    {
            //        Name = "",
            //        TypeId = -1,
            //        LocationPoint = new JZPoint(0,0),
            //        Width = 500,
            //        Height=2100,
            //        BaseLevel = 0,
            //        BaseOffset = 0,
            //    }
            //};
            //CreateDoorEventHandler createDoorEventHandler = new CreateDoorEventHandler();
            //createDoorEventHandler.SetParameters(createdDoorInfo);
            //createDoorEventHandler.Execute(uiApp);


            return Result.Succeeded;
        }
    }
}
