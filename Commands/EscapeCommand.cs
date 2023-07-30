using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Community.VisualStudio.Toolkit;
using System;
using Task = System.Threading.Tasks.Task;

namespace CPPUtility.Commands
{
    [Command(PackageIds.EscapeCommand)]
    internal sealed class EscapeCommand : BaseCommand<EscapeCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandManager.Instance.EscapeAsync();
        }
    }
}
