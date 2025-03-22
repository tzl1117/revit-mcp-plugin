using Autodesk.Revit.UI;

namespace revit_mcp_plugin.Commands.Interfaces
{
    public interface IWaitableExternalEventHandler : IExternalEventHandler
    {
        bool WaitForCompletion(int timeoutMs);
    }
}
