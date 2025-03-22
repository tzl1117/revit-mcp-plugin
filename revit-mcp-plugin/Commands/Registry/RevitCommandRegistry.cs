using revit_mcp_plugin.Commands.Interfaces;
using System.Collections.Generic;

namespace revit_mcp_plugin.Commands.Registry
{
    public class RevitCommandRegistry
    {
        private readonly Dictionary<string, IRevitCommand> _commands = new Dictionary<string, IRevitCommand>();

        public void RegisterCommand(IRevitCommand command)
        {
            _commands[command.CommandName] = command;
        }

        public bool TryGetCommand(string commandName, out IRevitCommand command)
        {
            return _commands.TryGetValue(commandName, out command);
        }
    }
}
