﻿using Autodesk.Revit.UI;
using revit_mcp_sdk.API.Interfaces;
using System.Threading;

namespace SampleCommandSet.Commands.Test
{
    public class SayHelloEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            TaskDialog.Show("revit-mcp", "hello MCP");
            _resetEvent.Set();
        }

        public string GetName()
        {
            return "say hello";
        }
    }
}
