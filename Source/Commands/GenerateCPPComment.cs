using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CPPUtility
{

    class CodeElementComparer : IEqualityComparer<CodeElement>
    {
        public bool Equals(CodeElement x, CodeElement y)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            return x.FullName == y.FullName &&
                x.StartPoint.AbsoluteCharOffset == y.StartPoint.AbsoluteCharOffset;
        }

        public int GetHashCode(CodeElement product)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (product is null) return 0;

            return product.FullName.GetHashCode() ^ product.StartPoint.AbsoluteCharOffset.GetHashCode();
        }
    }

    [Command(PackageIds.GenerateCPPComment)]
    internal sealed class GenerateCPPComment : BaseCommand<GenerateCPPComment>, ICancellableCommand
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandManager.Instance.ExecuteWithCancellationAsync(this);
        }

        DTE2 dte;
        DocumentUtility.DocumentPair documentPair;


        readonly InsertTextManager insertManager = new InsertTextManager();

        async Task<bool> InitAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            dte = await VS.GetServiceAsync<DTE, DTE2>();

            if (dte == null)
            {
                return false;
            }


            // ヘッダーとCPPのDocument取得
            documentPair = await DocumentUtility.GetActiveDocumentPairAsync(DocumentUtility.DocumentSwitchType.AlwaysCPP);

            if (documentPair.header == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: Header document is not found.");
                return false;
            }
            if (documentPair.cpp == null)
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: CPP document is not found.");
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

            dte.UndoContext.Open($"Generate CPP Comment.");

            try
            {
                // ヘッダーをもとにCPPの一番上のコメントを作る
                await ErrorHandlingUtility.TryCatchTaskFuncAsync(GenerateCPPDocmentTopCommentAsync);


                // ヘッダーをもとにCPPの関数のコメントを作る
                var option = await CPPUtilityOption.GetLiveInstanceAsync();
                if (option.IsUseGenerateCPPFunctionComment)
                {
                    await ErrorHandlingUtility.TryCatchTaskFuncAsync(GenerateCPPFunctionCommentAllAsync);
                }


                var editPoints = insertManager.ExecuteInsertAndFindEditPoints();


                EditSnippetManager.Instance.ExecuteEdit(editPoints,
                    EditSnippetManager.EndType.Success);
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }


        async Task GenerateCPPFunctionCommentAllAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var codeElements = GetCodeElements();

            var option = await CPPUtilityOption.GetLiveInstanceAsync();

            foreach (CodeElement element in codeElements)
            {
                if (element.Kind != vsCMElement.vsCMElementFunction)
                    continue;

                var codeFunction = element as CodeFunction;

                var name = codeFunction.FullName;

                await ErrorHandlingUtility.TryCatchTaskFuncAsync(async () =>
                 {
                     if (Generate(codeFunction, option.CPPFunctionCommentSnippet))
                     {
                         await OutputWindow.Instance.WriteLineAsync("Add CPP Comment " + name);
                     }
                 }, false);
            }
        }
        CodeElement[] GetCodeElements()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<CodeElement> returns = new List<CodeElement>();

            var headerProjectItem = documentPair.header.Parent?.ProjectItem;

            if (headerProjectItem?.FileCodeModel != null)
            {
                returns.AddRange(CodeModelUtility.CodeElementsRecursively(headerProjectItem.FileCodeModel.CodeElements));
            }

            var cppProjectItem = documentPair.cpp.Parent?.ProjectItem;

            if (cppProjectItem?.FileCodeModel != null)
            {
                returns.AddRange(CodeModelUtility.CodeElementsRecursively(cppProjectItem.FileCodeModel.CodeElements));
            }

            return returns.Distinct(new CodeElementComparer()).ToArray();
        }


        bool Generate(CodeFunction codeFunction, string snippet)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (codeFunction.StartPoint.Parent != documentPair.cpp)
            {
                return false;
            }
            var headerComment = GetHeaderFunctionComment(codeFunction);

            if (string.IsNullOrEmpty(headerComment))
                return false;

            snippet = SingletonHelper<CPPFunctionCommentLiteralFormatter>.Instance
                .FormatLiteral(snippet, new CPPFunctionCommentLiteralData(headerComment));

            var cppFunctionStartPoint = codeFunction.StartPoint.CreateEditPoint();
            return insertManager.InsertReservationFunctionComment(cppFunctionStartPoint, snippet);
        }

        string GetHeaderFunctionComment(CodeFunction codeFunction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var headerComment = codeFunction.Comment;

            if (!string.IsNullOrEmpty(headerComment))
            {
                return headerComment;
            }

            var startPoint = CodeModelUtility.GetHeaderFunctionStartPoint(codeFunction);

            if (startPoint.Parent == documentPair.cpp)
            {
                return BasicLiteralFormatter.EDIT_POINT_LITERAL;
            }


            var beforeLine = startPoint.GetLines(startPoint.Line - 1, startPoint.Line);

            var match = Regex.Match(beforeLine, @"^\s*//\s*(.+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        async Task GenerateCPPDocmentTopCommentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (documentPair.cpp.ExistsDocumentComment())
            {
                await OutputWindow.Instance.WriteLineAsync("WARNING: File top comment already exists in CPP.");
                return;
            }

            var documentTopCommentCPP = HeaderToCPPDocumentTopComment();

            if (string.IsNullOrEmpty(documentTopCommentCPP))
            {
                await OutputWindow.Instance.WriteLineAsync("ERROR: Failed to parse file top comment in header.");
                return;
            }

            insertManager.InsertReservation(new InsertInfo(documentPair.cpp.StartPoint.CreateEditPoint(), documentTopCommentCPP));
        }


        string HeaderToCPPDocumentTopComment()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var comment = documentPair.header.GetTopComment();

            if (string.IsNullOrEmpty(comment))
                return comment;

            // file.h を file.cpp にする
            comment = Regex.Replace(comment,
                documentPair.header.Parent.Name,
                documentPair.cpp.Parent.Name);

            if (!comment.EndsWith("\n"))
            {
                comment += "\n";
            }
            return comment;
        }


    }
}
