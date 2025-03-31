using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.API.Base;
using revit_mcp_plugin.API.Models;

namespace SampleCommandSet.Test
{
    public class SayHelloCommand : ExternalEventCommandBase
    {
        private SayHelloEventHandler _handler => (SayHelloEventHandler)Handler;

        public override string CommandName => "say_hello";

        public SayHelloCommand(UIApplication uiApp)
        : base(new SayHelloEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            RaiseAndWaitForCompletion(15000);
            return CommandResult.CreateSuccess(new { execute = true });
        }
    }
}
