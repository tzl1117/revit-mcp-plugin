using Autodesk.Revit.UI;
using System.Reflection;

namespace revit_mcp_plugin.Core
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("revit-mcp");
            PushButtonData pushButtonData = new PushButtonData("ID_EXCMD_TOGGLE_REVIT_REVIT_MCP", "mcp服务\n开关", Assembly.GetExecutingAssembly().Location, "revit_mcp_plugin.Core.MCPServiceConnection");
            pushButtonData.ToolTip = "open/close mcp server";
            ribbonPanel.AddItem(pushButtonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                if (SocketService.Instance.IsRunning)
                {
                    SocketService.Instance.Stop();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}
