using Newtonsoft.Json.Linq;

namespace revit_mcp_plugin.Commands.Interfaces
{
    public interface IRevitCommand
    {
        string CommandName { get; }
        object Execute(JObject parameters, string requestId);
    }
}
