using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.VCCodeModel;
using System.Text.RegularExpressions;
using System.Linq;

namespace CPPUtility
{
    [Command(PackageIds.CreateHeaderComment)]
    internal sealed class CreateHeaderComment : BaseCommand<CreateHeaderComment>, ICancellableCommand
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandManager.Instance.ExecuteWithCancellationAsync(this);
        }

        DTE2 dte;
        TextDocument headerDocument;

        readonly InsertTextManager insertManager = new InsertTextManager();

        async Task<bool> InitAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            dte = await VS.GetServiceAsync<DTE, DTE2>();

            if (dte == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: DTE is null.");
                return false;
            }

            // ヘッダーのDocument取得
            var documentPair = await DocumentUtility.GetActiveDocumentPairAsync(DocumentUtility.DocumentSwitchType.AlwaysHeader);
            headerDocument = documentPair.header;

            if (headerDocument == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: Header file is not found.");
                return false;
            }

            return true;
        }


        public async Task ExecuteWithCancellationAsync(CancellationToken token)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            if (!await InitAsync())
            {
                return;
            }

            dte.UndoContext.Open($"Create Header Comment.");

            List<EditSnippetInfo> editPoints;

            try
            {
                // ヘッダーの一番上のコメントを作る
                await ErrorHandlingUtility.TryCatchTaskFuncAsync(GenerateHeaderDocmentTopCommentAsync);


                // ヘッダーの関数コメントを作る
                var option = await CPPUtilityOption.GetLiveInstanceAsync();
                if (option.IsUseCreateHeaderFunctionComment)
                {
                    await ErrorHandlingUtility.TryCatchTaskFuncAsync(GenerateHeaderFunctionCommentAllAsync);
                }


                editPoints = insertManager.ExecuteInsertAndFindEditPoints();

            }
            finally
            {
                dte.UndoContext.Close();
            }

            if (editPoints.Count == 0)
            {
                await GenerateCPPCommentAsync();
                return;
            }

            EditSnippetManager.Instance.ExecuteEdit(editPoints,
                EditSnippetManager.EndType.SuccessInOtherLine,
                type =>
                {
                    _ = GenerateCPPCommentAsync();
                });
        }

        async Task GenerateHeaderFunctionCommentAllAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var headerProjectItem = headerDocument.Parent?.ProjectItem;

            if (headerProjectItem?.FileCodeModel == null)
                return;

            var headerCodeElements = CodeModelUtility.CodeElementsRecursively(headerProjectItem.FileCodeModel.CodeElements);


            foreach (CodeElement headerElement in headerCodeElements)
            {
                if (headerElement.Kind != vsCMElement.vsCMElementFunction)
                    continue;

                var headerFunction = headerElement as CodeFunction2;

                var insertText = $"// {BasicLiteralFormatter.EDIT_POINT_LITERAL}\n";


                ErrorHandlingUtility.TryCatchAction(() =>
                {
                    var functionStartPoint = CodeModelUtility.GetHeaderFunctionStartPoint(headerFunction);
                    insertManager.InsertReservationFunctionComment(functionStartPoint, insertText);
                },
                false);
            }

        }

        async Task GenerateHeaderDocmentTopCommentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (headerDocument.ExistsDocumentComment())
            {
                return;
            }

            var snippet = CPPUtilityOption.Instance.DocumentTopCommentSnippet;

            snippet = SingletonHelper<DocumentLiteralFormatter>.Instance
                .FormatLiteral(snippet, new DocumentLiteralData(headerDocument));

            insertManager.InsertReservation(new InsertInfo(headerDocument.StartPoint.CreateEditPoint(), snippet));
        }




        async Task GenerateCPPCommentAsync()
        {
            await Task.Delay(500);

            await VS.Commands.ExecuteAsync(PackageGuids.CPPUtility, PackageIds.GenerateCPPComment);
        }
    }
}
