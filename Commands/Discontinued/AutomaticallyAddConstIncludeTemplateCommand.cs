using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;

namespace CPPUtility.Commands
{
    [Command(PackageIds.AutomaticallyAddConstIncludeTemplateCommand)]
    internal sealed class AutomaticallyAddConstIncludeTemplateCommand : BaseCommand<AutomaticallyAddConstIncludeTemplateCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var command = new AutomaticallyAddConst(AutomaticallyAddConstOption.IncludeTemplate);
            await CommandManager.Instance.ExecuteWithCancellationAsync(command);
        }
    }
}
