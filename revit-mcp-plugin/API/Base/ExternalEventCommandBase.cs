using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.API.Interfaces;
using revit_mcp_plugin.Core;
using revit_mcp_plugin.Core.JsonRPC;
using revit_mcp_plugin.Exceptions;
using System;

namespace revit_mcp_plugin.API.Base
{
    public abstract class ExternalEventCommandBase : IRevitCommand
    {
        protected ExternalEvent Event { get; private set; }
        protected IWaitableExternalEventHandler Handler { get; private set; }
        protected UIApplication UiApp { get; private set; }

        public abstract string CommandName { get; }

        public ExternalEventCommandBase(IWaitableExternalEventHandler handler, UIApplication uiApp)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            UiApp = uiApp ?? throw new ArgumentNullException(nameof(uiApp));
            Event = ExternalEvent.Create(handler);
        }

        public abstract object Execute(JObject parameters, string requestId);

        protected bool RaiseAndWaitForCompletion(int timeoutMs = 10000)
        {
            Event.Raise();
            return Handler.WaitForCompletion(timeoutMs);
        }

        protected CommandExecutionException CreateTimeoutException(string commandName)
        {
            return new CommandExecutionException(
                $"命令 {commandName} 执行超时",
                JsonRPCErrorCodes.CommandExecutionTimeout);
        }
    }
}
