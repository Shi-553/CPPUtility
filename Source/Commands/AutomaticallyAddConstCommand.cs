using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Community.VisualStudio.Toolkit;
using System;
using Task = System.Threading.Tasks.Task;

namespace CPPUtility
{
    [Command(PackageIds.AutomaticallyAddConstCommand)]
    internal sealed class AutomaticallyAddConstCommand : BaseCommand<AutomaticallyAddConstCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var command = new AutomaticallyAddConst(AutomaticallyAddConstOption.Default);
            await CommandManager.Instance.ExecuteWithCancellationAsync(command);
        }
    }
}
