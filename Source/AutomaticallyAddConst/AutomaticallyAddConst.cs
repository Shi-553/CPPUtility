using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Shell.TableControl;

namespace CPPUtility
{
    internal enum AutomaticallyAddConstOption
    {
        Default,
        IncludeTemplate
    }

    internal class AutomaticallyAddConst : ICancellableCommand
    {
        private const string EC_ERROR = "EC_ERROR";

        readonly AutomaticallyAddConstOption option;

        CancellationToken token;

        EnvDTE.Project selectedProject;

        public AutomaticallyAddConst(AutomaticallyAddConstOption option)
        {
            this.option = option;
        }



        public async Task ExecuteWithCancellationAsync(CancellationToken token)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            this.token = token;

            var dte = await VS.GetServiceAsync<DTE, DTE2>();

            dte.UndoContext.Open("Add Const");

            await OutputWindow.Instance.WriteLineAsync("Start...");

            var iErrorList = dte.ToolWindows.ErrorList as IErrorList;
            iErrorList.AreErrorsShown = true;
            iErrorList.AreBuildErrorSourceEntriesShown = true;


            selectedProject = dte.ActiveDocument.ProjectItem.ContainingProject;


            try
            {
                await BuildAsync();

                token.ThrowIfCancellationRequested();
                await Task.Delay(2000);
                token.ThrowIfCancellationRequested();

                if (await CheckAnyErrorAsync())
                {
                    await VS.MessageBox.ShowAsync("Cannot be executed if there is an error.",
                        "Resolve the error.",
                         OLEMSGICON.OLEMSGICON_CRITICAL,
                         OLEMSGBUTTON.OLEMSGBUTTON_OK,
                         OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }



                var allProjectItems = CodeModelUtility.GetProjectItemsRecursively(selectedProject.ProjectItems);


                foreach (ProjectItem item in allProjectItems)
                {
                    if (item.ContainingProject != selectedProject)
                        continue;
                    if (item.FileCodeModel == null)
                        continue;

                    foreach (CodeElement element in CodeModelUtility.CodeElementsRecursively(item.FileCodeModel.CodeElements))
                    {
                        try
                        {
                            if (!AddConst(element))
                                continue;

                            var filePath = item.GetFilePath();
                            var startPoint = element.GetStartPoint();

                            await OutputWindow.Instance.WriteLineAsync($"{filePath}({startPoint.Line}):Add Const {element.Name};");
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                        }
                    }

                    token.ThrowIfCancellationRequested();
                }


                await FixErrorAsync();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                await OutputWindow.Instance.WriteLineAsync(ex.ToString());
            }
            catch (OperationCanceledException ex)
            {
                await OutputWindow.Instance.WriteLineAsync("cancel");
                throw ex;
            }
            finally
            {
                dte.UndoContext.Close();
                await OutputWindow.Instance.WriteLineAsync("Finished");
            }
        }

        bool AddConst(CodeElement element)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (element.Kind != vsCMElement.vsCMElementFunction)
                {
                    return false;
                }

                var function = element as VCCodeFunction;

                if (!(function.Parent is CodeElement parent))
                {
                    return false;
                }

                if (parent.Kind != vsCMElement.vsCMElementClass &&
                    parent.Kind != vsCMElement.vsCMElementStruct)
                {
                    return false;
                }

                if (function.IsConstant)
                {
                    return false;
                }
                if (option != AutomaticallyAddConstOption.IncludeTemplate && function.IsTemplate)
                {
                    return false;
                }
                if (function.FunctionKind.HasFlag(vsCMFunction.vsCMFunctionConstructor))
                {
                    return false;
                }

                function.IsConstant = true;
                function.IsConstant = true;

            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return false;
            }
            return true;
        }
        bool RemoveConst(CodeElement element)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (element == null || element.Kind != vsCMElement.vsCMElementFunction)
                {
                    return false;
                }
                if (element is VCCodeFunction func)
                {
                    func.IsConstant = false;
                    func.IsConstant = false;
                    return true;
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
            }
            return false;
        }


        private async Task FixErrorAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await VS.GetServiceAsync<DTE, DTE2>();

            int errorCount = await GetErrorCountAsync();

            int retryMax = 2;
            for (int i = 0; i < retryMax; i++)
            {
                await Task.Delay(1000);


                await BuildAsync();



                int waitCount = 0;
                while (true)
                {
                    await Task.Delay(1000);
                    token.ThrowIfCancellationRequested();

                    if (waitCount > 5)
                    {
                        await OutputWindow.Instance.WriteLineAsync("Complete!!! No Error ");

                        await VS.MessageBox.ShowAsync("Complete!!",
                            "No Error ",
                             OLEMSGICON.OLEMSGICON_INFO,
                             OLEMSGBUTTON.OLEMSGBUTTON_OK,
                             OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                        return;
                    }
                    waitCount++;

                    if (await CheckAnyErrorAsync())
                    {
                        break;
                    }
                }


                var iErrorList = dte.ToolWindows.ErrorList as IErrorList;
                var errorList = dte.ToolWindows.ErrorList;

                int index = 1;
                foreach (var entry in iErrorList.TableControl.Entries)
                {
                    var errorItem = errorList.ErrorItems.Item(index);

                    try
                    {
                        if (!CheckError(entry))
                        {
                            continue;
                        }

                        var projectItem = dte.Solution.FindProjectItem(errorItem.FileName);

                        if (selectedProject != projectItem.ContainingProject)
                        {
                            await OutputWindow.Instance.WriteLineAsync($"Other Project ->{errorItem.FileName}");
                            continue;
                        }

                        if (!(projectItem.Document.Selection is TextSelection selection))
                        {
                            await OutputWindow.Instance.WriteLineAsync($"docment is not TextDocument.->{errorItem.FileName}");
                            continue;
                        }

                        await OutputWindow.Instance.WriteLineAsync($"{errorItem.Description} {errorItem.FileName}");

                        var editPoint = selection.ActivePoint.CreateEditPoint();

                        editPoint.MoveToLineAndOffset(errorItem.Line, errorItem.Column);
                        RemoveConst(editPoint.CodeElement[vsCMElement.vsCMElementFunction]);


                        token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                    }
                    index++;
                }



                int newErrorCount = await GetErrorCountAsync();

                if (newErrorCount != errorCount)
                {
                    i = 0;
                }
                errorCount = newErrorCount;
            }


            await OutputWindow.Instance.WriteLineAsync("Can not fix errors...");

            await VS.MessageBox.ShowAsync("Can not fix errors...",
                "",
                 OLEMSGICON.OLEMSGICON_CRITICAL,
                 OLEMSGBUTTON.OLEMSGBUTTON_OK,
                 OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }


        async Task BuildAsync()
        {
            token.ThrowIfCancellationRequested();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            try
            {
                var project = await VS.Solutions.FindProjectsAsync(selectedProject.Name);
                await VS.Build.BuildProjectAsync(project);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException)) { }

            token.ThrowIfCancellationRequested();
        }



        async Task<bool> CheckAnyErrorAsync()
        {
            var dte = await VS.GetServiceAsync<DTE, DTE2>();
            var iErrorList = dte.ToolWindows.ErrorList as IErrorList;

            return iErrorList.TableControl.Entries.Any(h => CheckError(h));
        }
        async Task<int> GetErrorCountAsync()
        {
            var dte = await VS.GetServiceAsync<DTE, DTE2>();
            var iErrorList = dte.ToolWindows.ErrorList as IErrorList;
            return iErrorList.TableControl.Entries.Count(h => CheckError(h));
        }

        bool CheckError(ITableEntryHandle handle)
        {
            if (handle.TryGetValue("errorseverity", out var c))
            {

                return c.ToString() == EC_ERROR;
            }
            return false;
        }



    }
}
