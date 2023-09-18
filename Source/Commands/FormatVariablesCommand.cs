using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Shell.TableControl;
using System.Data.Common;
using System.Windows.Documents;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using TextSelection = EnvDTE.TextSelection;
using Microsoft.VisualStudio.VCCodeModel;
using System.Reflection;

namespace CPPUtility.Source
{

    [Command(PackageIds.FormatVariables)]
    internal sealed class FormatVariablesCommand : BaseCommand<FormatVariablesCommand>, ICancellableCommand
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandManager.Instance.ExecuteWithCancellationAsync(this);
        }

        const string FindAllReferenceKind = "{A80FEBB4-E7E0-4147-B476-21AAF2453969}";

        DTE2 dte;
        TextDocument activeDocument;
        EditTextManager editTextManager;

        async Task<bool> InitAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            dte = await VS.GetServiceAsync<DTE, DTE2>();

            if (dte == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: DTE is null.");
                return false;
            }

            // アクティブなDocument取得
            activeDocument = await DocumentUtility.GetActiveTextDocumentAsync();

            if (activeDocument == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: Active Text Document is null.");
                return false;
            }

            await InitFindAllReferenceWindowAsync();

            editTextManager = new EditTextManager();

            return true;
        }

        async Task InitFindAllReferenceWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var findAllreferenceService = await VS.GetServiceAsync<SVsFindAllReferences, IFindAllReferencesService>();
            findAllreferenceService.StartSearch("");

            await Task.Delay(100);
            token.ThrowIfCancellationRequested();

            var w = GetFindAllReferenceWindow();
            if (w == null)
                return;

            var findAllReferencesObject = w.Object;
            var objType = findAllReferencesObject.GetType();


            var filters = objType.GetMethod("get_VisibleFilters").Invoke(findAllReferencesObject, null) as IReadOnlyList<string>;
            var currentProjectFilter = filters[3];

            objType.GetMethod("set_CurrentFilterDisplayName").Invoke(findAllReferencesObject, new object[] { currentProjectFilter });
        }

        Window GetFindAllReferenceWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (Window w in dte.Windows)
            {
                if (w.ObjectKind == FindAllReferenceKind)
                {
                    return w;
                }
            }
            return null;
        }
        async Task<IWpfTableControl> GetFindAllReferenceTableControlAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var w = GetFindAllReferenceWindow();
            if (w == null)
                return null;

            var findAllReferencesObject = w.Object;
            var objType = findAllReferencesObject.GetType();

            var progress = objType.GetMethod("get_ProgressBar", RefrectionUtility.AllBindingFlags).Invoke(findAllReferencesObject, null);

            var isDetachedProperty = progress.GetType().GetProperty("IsDetached", RefrectionUtility.AllBindingFlags);

            int progressCount = 0;

            // プログレスバーが完全になくなるまで待つ
            while (true)
            {
                // プログレスバーがなくなるまで待つ
                while (isDetachedProperty.GetValue(progress) is bool b && !b)
                {
                    progressCount = 0;
                    await Task.Delay(100);
                    token.ThrowIfCancellationRequested();
                }
                await Task.Delay(1000);
                token.ThrowIfCancellationRequested();

                progressCount++;
                if (progressCount >= 3)
                {
                    break;
                }
            }

            return objType.GetProperty("TableControl").GetValue(findAllReferencesObject, null) as IWpfTableControl;
        }

        public async Task<List<EditPoint>> GetAllReferencesAsync(ICodeVariableLike codeVariable)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var defPoint = codeVariable.get_StartPointOf(vsCMPart.vsCMPartName, vsCMWhere.vsCMWhereDefinition);
            var declarPoint = codeVariable.get_StartPointOf(vsCMPart.vsCMPartName, vsCMWhere.vsCMWhereDeclaration);

            var point = defPoint ?? declarPoint;

            var selection = codeVariable.ProjectItem.Document.Selection as TextSelection;
            selection.MoveToPoint(point);
            codeVariable.ProjectItem.Document.Activate();


            await Task.Delay(1000);
            token.ThrowIfCancellationRequested();

            await VS.Commands.ExecuteAsync("Edit.FindAllReferences");

            var table = await GetFindAllReferenceTableControlAsync();

            List<EditPoint> results = new List<EditPoint>();

            foreach (var item in table.Entries.ToArray())
            {
                item.TryGetValue<string>("documentname", out var documentname);
                item.TryGetValue<int>("line", out var line);
                item.TryGetValue<int>("column", out var column);

                var projectItem = dte.Solution.FindProjectItem(documentname);

                if (!(projectItem?.Document?.Selection is EnvDTE.TextSelection itemSelection))
                {
                    continue;
                }

                var editPoint = itemSelection.Parent.CreateEditPoint();
                editPoint.MoveToLineAndOffset(line + 1, column + 1);

                {
                    var checkEditPoint = editPoint;
                    var charOffset = checkEditPoint.LineCharOffset;
                    editPoint.WordRight();
                    var text = editPoint.GetText(charOffset - editPoint.LineCharOffset);
                    if (text != codeVariable.Name)
                    {
                        continue;
                    }
                }

                results.Add(editPoint);
            }

            if (declarPoint != point)
            {
                results.Add(declarPoint.CreateEditPoint());
            }

            return results;
        }

        async Task FormatVariableAsync(ICodeVariableLike targetVariable)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (targetVariable == null)
            {
                return;
            }

            var delimiterRegexString = CPPUtilityOption.Instance.VariableDelimiterType.GetDelimiterRegexString();
            var testRegex = new Regex(CPPUtilityOption.Instance.VariableFormattedTestRegexText + delimiterRegexString);


            var name = targetVariable.Name;
            if (testRegex.IsMatch(name))
            {
                return;
            }
            var variableFormatHelper = new VariableFormatHelper(name);

            foreach (var formatInfo in CPPUtilityOption.Instance.VariableFormatInfos)
            {
                if (formatInfo.Selector.CanSelect(targetVariable))
                {
                    formatInfo.Formatter.FormatVariable(variableFormatHelper, targetVariable);
                }
            }

            var formatedName = variableFormatHelper.GetCombinedVariable(CPPUtilityOption.Instance.VariableDelimiterType);

            if (name == formatedName)
            {
                return;
            }


            var editPoints = await GetAllReferencesAsync(targetVariable);

            await OutputWindow.Instance.WriteLineAsync($"{targetVariable.Name}: {editPoints.Count}");

            foreach (var editPoint in editPoints)
            {

                editTextManager.EditReservation(
                    new EditInfo(editPoint,
                    () =>
                    {
                        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                        var charOffset = editPoint.LineCharOffset;
                        editPoint.WordRight();
                        editPoint.Delete(charOffset - editPoint.LineCharOffset);

                        editPoint.Insert(formatedName);
                    }));
            }
        }
        CancellationToken token;
        public async Task ExecuteWithCancellationAsync(CancellationToken token)
        {
            this.token = token;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            if (!await InitAsync())
            {
                return;
            }


            dte.UndoContext.Open($"Format Variables.");

            try
            {
                var activeDocuemntVariables = CodeModelUtility.GetCodeElementsChildrenRecursively(activeDocument.Parent?.ProjectItem?.FileCodeModel?.CodeElements);
                foreach (var codeElement in activeDocuemntVariables)
                {
                    if (codeElement.Kind == vsCMElement.vsCMElementFunction && codeElement is CodeFunction targetFunction)
                    {
                        foreach (var parameter in targetFunction.Parameters)
                        {
                            await ErrorHandlingUtility.TryCatchTaskFuncAsync(
                                () => FormatVariableAsync(CodeVariableLikeFactory.CreateCodeVariableLike(parameter as CodeParameter)));

                            token.ThrowIfCancellationRequested();
                        }
                    }
                    if (codeElement.Kind == vsCMElement.vsCMElementVariable)
                    {
                        await ErrorHandlingUtility.TryCatchTaskFuncAsync(
                            () => FormatVariableAsync(CodeVariableLikeFactory.CreateCodeVariableLike(codeElement as CodeVariable)));

                        token.ThrowIfCancellationRequested();
                    }

                }

                editTextManager.ExecuteEdit();

                await VS.MessageBox.ShowAsync(
                    "Format Success!",
                    "",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }

    }
}
