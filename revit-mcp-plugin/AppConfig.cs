namespace revit_mcp_plugin
{
    public class AppConfig
    {
        public int Port { get; set; } = 8080; // 端口号
        public int DefaultTimeout { get; set; } = 10000; // 超时时间
        public bool EnableLogging { get; set; } = true;
    }
}
