using Microsoft.Build.Framework;
using Microsoft.ServiceHub.Resources;
using Microsoft.VisualStudio.Package;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPPUtility
{
    internal interface ICancellableCommand
    {
        Task ExecuteWithCancellationAsync(CancellationToken token);
    }

    internal class CommandManager : Singleton<CommandManager>
    {
        readonly HashSet<Task> tasks = new HashSet<Task>();

        CancellationTokenSource baseSource = new CancellationTokenSource();

        public async Task ExecuteWithCancellationAsync(ICancellableCommand command, CancellationToken cancellationToken)
        {
            var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(baseSource.Token, cancellationToken);

            await ExecuteWithCancellationInternalAsync(command, linkedSource);
        }
        public async Task ExecuteWithCancellationAsync(ICancellableCommand command)
        {
            await ExecuteWithCancellationInternalAsync(command, baseSource);
        }

        async Task ExecuteWithCancellationInternalAsync(ICancellableCommand command, CancellationTokenSource source)
        {
            try
            {
                var task = command.ExecuteWithCancellationAsync(source.Token);
                tasks.Add(task);

                await task;

                tasks.Remove(task);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                await OutputWindow.Instance.WriteLineAsync(ex.ToString());
            }
        }

        public async Task EscapeAsync()
        {
            try
            {
                baseSource.Cancel();

                await Task.WhenAll(tasks);

            }
            catch (OperationCanceledException)
            {
                await OutputWindow.Instance.WriteLineAsync("Cancel All");
            }
            finally
            {
                baseSource = new CancellationTokenSource();
                tasks.Clear();
            }
        }

    }
}
