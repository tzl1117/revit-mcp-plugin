using System;

namespace revit_mcp_plugin.Exceptions
{
    namespace revit_mcp_framework.Exceptions
    {
        public class ConfigurationException : Exception
        {
            public ConfigurationException(string message)
                : base(message)
            {
            }

            public ConfigurationException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}
