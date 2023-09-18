using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Threading.Tasks;

namespace CPPUtility
{
    public static class ErrorHandlingUtility{
        public static async Task TryCatchTaskFuncAsync(Func<Task> task, bool isUseErrorLog = true)
        {
            try
            {
                await task();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                if (isUseErrorLog)
                {
                    await OutputWindow.Instance.WriteLineAsync(ex.ToString());
                }
            }
        }
        public static void TryCatchAction(Action task, bool isUseErrorLog = true)
        {
            try
            {
                task();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                if (isUseErrorLog)
                {
                    OutputWindow.Instance.WriteLine(ex.ToString());
                }
            }
        }
    }
}