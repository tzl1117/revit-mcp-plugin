using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Trace.WriteLine("Revit Socket 客户端");

            try
            {
                TcpClient client = new TcpClient("localhost", 8080);
                NetworkStream stream = client.GetStream();

                // 创建符合JSON-RPC 2.0协议的墙体创建请求
                var json = new
                {
                    jsonrpc = "2.0",
                    method = "createFloor",
                    paramss = JObject.Parse(File.ReadAllText(@"C:\Users\huhaonan\Desktop\temp.json"))["Data"],
                    id = 1
                };

                string jsonRpcRequest = JsonConvert.SerializeObject(json).Replace("paramss", "params");

                //SaveToDesktop(jsonRpcRequest,"1.txt");


                //string jsonRpcRequest = @"{
                //    ""jsonrpc"": ""2.0"",
                //    ""method"": ""createFloor"",
                //    ""params"": {
                //        ""startX"": 0,
                //        ""startY"": 0,
                //        ""endX"": 20,
                //        ""endY"": 0,
                //        ""height"": 10,
                //        ""thickness"": 0.3
                //    },
                //    ""id"": 1
                //}";

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

            System.Diagnostics.Trace.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        /// <summary>
        /// 将指定的消息保存到桌面的指定文件中（默认覆盖文件）
        /// </summary>
        /// <param name="message">要保存的消息内容</param>
        /// <param name="fileName">目标文件名</param>
        public static void SaveToDesktop(string message, string fileName = "temp.json", bool isAppend = false)
        {
            // 确保 logName 包含后缀
            if (!Path.HasExtension(fileName))
            {
                fileName += ".txt"; // 默认添加 .txt 后缀
            }

            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            // 组合完整的文件路径
            string filePath = Path.Combine(desktopPath, fileName);

            // 写入文件（覆盖模式）
            using (StreamWriter sw = new StreamWriter(filePath, isAppend))
            {
                sw.WriteLine($"{message}");
            }
        }
    }
}
