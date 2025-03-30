using revit_mcp_plugin.Core.JsonRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace revit_mcp_plugin.Exceptions
{
    public class CommandExecutionException : Exception
    {
        public int ErrorCode { get; }
        public object ErrorData { get; }

        public CommandExecutionException(string message)
            : base(message)
        {
            ErrorCode = JsonRPCErrorCodes.InternalError;
        }

        public CommandExecutionException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public CommandExecutionException(string message, int errorCode, object errorData)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorData = errorData;
        }
    }
}
