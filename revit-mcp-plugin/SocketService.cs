using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace revit_mcp_plugin
{
    public class SocketService
    {
        private static SocketService _instance;
        private TcpListener _listener;
        private Thread _listenerThread;
        private bool _isRunning;
        private int _port = 8080;

        private ExternalEvent _createWallEvent;
        private CreateWallEventHandler _createWallHandler;

        // UIApplication
        private UIApplication _uiApp;

        private readonly Dictionary<string, Func<JObject, string, object>> _commandHandlers =
            new Dictionary<string, Func<JObject, string, object>>();

        public static SocketService Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new SocketService();
                return _instance;
            }
        }

        private SocketService() { _commandHandlers["createWall"] = HandleCreateWallCommand; }

        public bool IsRunning => _isRunning;

        public int Port
        {
            get => _port;
            set => _port = value;
        }

        // 初始化外部事件和UIApplication
        public void Initialize(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _createWallHandler = new CreateWallEventHandler();
            _createWallEvent = ExternalEvent.Create(_createWallHandler);
        }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _isRunning = true;
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                _listenerThread = new Thread(ListenForClients)
                {
                    IsBackground = true
                };
                _listenerThread.Start();              
            }
            catch (Exception ex)
            {
                _isRunning = false;
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            try
            {
                _isRunning = false;

                _listener?.Stop();
                _listener = null;

                if(_listenerThread!=null && _listenerThread.IsAlive)
                {
                    _listenerThread.Join(1000);
                }
            }
            catch(Exception ex)
            {
                // log error
            }
        }

        private void ListenForClients()
        {
            try
            {
                while (_isRunning)
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    Thread clientThread = new Thread(HandleClientCommunication)
                    {
                        IsBackground = true
                    };
                    clientThread.Start(client);
                }
            }
            catch (SocketException)
            {
                
            }
            catch(Exception ex)
            {
                // log
            }
        }

        private void HandleClientCommunication(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream stream = tcpClient.GetStream();

            try
            {
                byte[] buffer = new byte[8192];

                while (_isRunning && tcpClient.Connected)
                {
                    // 读取客户端消息
                    int bytesRead = 0;

                    try
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                    catch (IOException)
                    {
                        // 客户端断开连接
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        // 客户端断开连接
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"收到消息: {message}");

                    string response = ProcessJsonRPCRequest(message);

                    // 发送响应
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            catch(Exception ex)
            {
                // log
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private string ProcessJsonRPCRequest(string requestJson)
        {
            JsonRPCRequest request;

            try
            {
                // 解析JSON-RPC请求
                request = JsonConvert.DeserializeObject<JsonRPCRequest>(requestJson);

                // 验证请求格式是否有效
                if (request == null || !request.IsValid())
                {
                    return CreateErrorResponse(
                        null,
                        JsonRPCErrorCodes.InvalidRequest,
                        "Invalid JSON-RPC request"
                    );
                }

                // 查找方法处理程序
                if (!_commandHandlers.TryGetValue(request.Method, out var handler))
                {
                    return CreateErrorResponse(
                        request.Id,
                        JsonRPCErrorCodes.MethodNotFound,
                        $"Method '{request.Method}' not found"
                    );
                }

                try
                {
                    // 执行命令处理程序
                    object result = handler(request.Params, request.Id);

                    // 创建成功响应
                    return CreateSuccessResponse(request.Id, result);
                }
                catch (Exception ex)
                {
                    // 创建错误响应
                    return CreateErrorResponse(
                        request.Id,
                        JsonRPCErrorCodes.InternalError,
                        ex.Message
                    );
                }
            }
            catch (JsonException)
            {
                // JSON解析错误
                return CreateErrorResponse(
                    null,
                    JsonRPCErrorCodes.ParseError,
                    "Invalid JSON"
                );
            }
            catch (Exception ex)
            {
                // 处理请求时的其他错误
                return CreateErrorResponse(
                    null,
                    JsonRPCErrorCodes.InternalError,
                    $"Internal error: {ex.Message}"
                );
            }
        }

        private string CreateSuccessResponse(string id, object result)
        {
            var response = new JsonRPCSuccessResponse
            {
                Id = id,
                Result = result is JToken jToken ? jToken : JToken.FromObject(result)
            };

            return response.ToJson();
        }

        private string CreateErrorResponse(string id, int code, string message, object data = null)
        {
            var response = new JsonRPCErrorResponse
            {
                Id = id,
                Error = new JsonRPCError
                {
                    Code = code,
                    Message = message,
                    Data = data != null ? JToken.FromObject(data) : null
                }
            };

            return response.ToJson();
        }

        private object HandleCreateWallCommand(JObject parameters, string requestId)
        {
            try
            {
                // 解析墙参数
                double startX = parameters["startX"].Value<double>();
                double startY = parameters["startY"].Value<double>();
                double endX = parameters["endX"].Value<double>();
                double endY = parameters["endY"].Value<double>();
                double height = parameters["height"].Value<double>();
                double width = parameters.ContainsKey("width") ? parameters["width"].Value<double>() : 0.3;

                // 设置墙体参数
                _createWallHandler.SetWallParameters(startX, startY, endX, endY, height, width);

                // 触发外部事件
                _createWallEvent.Raise();

                // 等待墙体创建完成
                if (_createWallHandler.WaitForCompletion(10000))
                {
                    // 返回创建的墙体信息
                    return _createWallHandler.CreatedWallInfo;
                }
                else
                {
                    throw new TimeoutException("Operation timed out");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create wall: {ex.Message}");
            }
        }
    }
}
