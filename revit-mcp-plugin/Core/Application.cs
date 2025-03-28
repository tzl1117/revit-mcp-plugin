using System;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;



namespace revit_mcp_plugin.Core
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Revit MCP Plugin");
            PushButtonData pushButtonData = new PushButtonData("ID_EXCMD_TOGGLE_REVIT_REVIT_MCP", "Claude MCP Switch",
                Assembly.GetExecutingAssembly().Location, "revit_mcp_plugin.Core.MCPServiceConnection");
            pushButtonData.ToolTip = "Open / Close mcp server";
            pushButtonData.Image = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources-icon-16.png", UriKind.RelativeOrAbsolute));
            pushButtonData.LargeImage = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources/icon-32.png", UriKind.RelativeOrAbsolute));
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
