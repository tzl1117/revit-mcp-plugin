using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using revit_mcp_plugin.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Commands.Base
{
    public abstract class ExternalEventCommandBase : IRevitCommand
    {
        protected ExternalEvent Event { get; private set; }
        protected IWaitableExternalEventHandler Handler { get; private set; }
        protected UIApplication UiApp { get; private set; }

        public abstract string CommandName { get; }

        public ExternalEventCommandBase(IWaitableExternalEventHandler handler, UIApplication uiApp)
        {
            Handler = handler;
            Event = ExternalEvent.Create(handler);
            UiApp = uiApp;
        }

        public abstract object Execute(JObject parameters, string requestId);

        protected bool RaiseAndWaitForCompletion(int timeoutMs = 10000)
        {
            Event.Raise();
            return Handler.WaitForCompletion(timeoutMs);
        }
    }

}
