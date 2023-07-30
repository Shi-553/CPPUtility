using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

namespace CPPUtility
{
    [Command(PackageIds.OpenOptionPageCommand)]
    internal sealed class OpenOptionPageCommand : BaseCommand<OpenOptionPageCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.Settings.OpenAsync<CPPUtilityOptionPage>();
        }
    }
}
