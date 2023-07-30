using Microsoft.Build.Framework;
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

        CancellationTokenSource source = new CancellationTokenSource();

        public async Task ExecuteWithCancellationAsync(ICancellableCommand command)
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
                source.Cancel();

                await Task.WhenAll(tasks);

            }
            catch (OperationCanceledException)
            {
                await OutputWindow.Instance.WriteLineAsync("Cancel All");
            }
            finally
            {
                source = new CancellationTokenSource();
                tasks.Clear();
            }
        }

    }
}
