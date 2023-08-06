using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace CPPUtility
{
    internal static class DocumentUtility
    {
        public static Task<bool> ToggleHeaderCPPFileAsync()
        {
            return VS.Commands.ExecuteAsync("EditorContextMenus.CodeWindow.ToggleHeaderCodeFile");
        }

        public static string GetAllText(this TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var startPoint = document.StartPoint;
            var endPoint = document.EndPoint;

            var editPoint = startPoint.CreateEditPoint();

            return editPoint.GetText(endPoint.AbsoluteCharOffset);
        }

        public static bool ExistsDocumentComment(this TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var editPoint = document.StartPoint.CreateEditPoint();

            string lineText;
            while (true)
            {
                lineText = editPoint.GetLines(editPoint.Line, editPoint.Line + 1);
                if (!string.IsNullOrWhiteSpace(lineText))
                    break;
                editPoint.LineDown();

                if (document.EndPoint.Line <= editPoint.Line)
                {
                    return false;
                }
            }

            return Regex.IsMatch(lineText, @"^\s*(//)|(/\*)", RegexOptions.ExplicitCapture);
        }
        public static async Task<TextDocument> GetActiveTextDocumentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await VS.GetServiceAsync<DTE, DTE2>();
            if (dte.ActiveWindow.DocumentData is TextDocument textDocument)
                return textDocument;

            foreach (Window window in dte.Windows)
            {
                if (window.LinkedWindowFrame != dte.ActiveWindow.LinkedWindowFrame)
                    continue;

                if (window.DocumentData is TextDocument textDocument1)
                {
                    window.Activate();
                    return textDocument1;
                }
            }
            return null;
        }

        public enum DocumentSwitchType
        {
            KeepCurrent,

            AlwaysHeader,
            AlwaysCPP,
        }

        public static async Task<DocumentPair> GetActiveDocumentPairAsync(DocumentSwitchType switchType)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var firstDocument = await GetActiveTextDocumentAsync();

            await ToggleHeaderCPPFileAsync();

            var secondDocument = await GetActiveTextDocumentAsync();

            if (firstDocument == null && secondDocument == null)
                return new DocumentPair();


            var pair = new DocumentPair(firstDocument, secondDocument);

            if (firstDocument == secondDocument)
                return pair;

            switch (switchType)
            {
                case DocumentSwitchType.KeepCurrent:
                    await ToggleHeaderCPPFileAsync();
                    break;

                case DocumentSwitchType.AlwaysHeader:
                    if (firstDocument.IsHeaderDocument())
                        await ToggleHeaderCPPFileAsync();
                    break;

                case DocumentSwitchType.AlwaysCPP:
                    if (firstDocument.IsCPPDocument())
                        await ToggleHeaderCPPFileAsync();
                    break;

                default:
                    break;
            }

            return pair;
        }

        public static bool IsHeaderDocument(this TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var name = document.Parent.Name;
            return name.EndsWith("h") || name.EndsWith("hpp");
        }
        public static bool IsCPPDocument(this TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var name = document.Parent.Name;
            return name.EndsWith("cpp");
        }

        internal struct DocumentPair
        {
            public TextDocument header;
            public TextDocument cpp;

            public DocumentPair(TextDocument firstDocument, TextDocument secondDocument)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                header = null;
                cpp = null;

                SetDocument(firstDocument);
                SetDocument(secondDocument);
            }
            void SetDocument(TextDocument document)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (document.IsHeaderDocument())
                {
                    header = document;
                    return;
                }
                if (document.IsCPPDocument())
                {
                    cpp = document;
                    return;
                }

            }
        }


        public static string GetTopComment(this TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            const string beginLineComment = "//";
            const string beginComment = "/*";
            const string endComment = "*/";

            // $"{}" を使いたいけど使うとシンタックスハイライトが消える
            var matchPattern = new Regex(@"\G(//)|(/\*)|(\*/)|(\s+)|(\S)", RegexOptions.ExplicitCapture);


            var editPoint = document.StartPoint.CreateEditPoint();
            int line = 0;
            int finalCommentLine = 0;

            bool isCommentNow = false;
            while (true)
            {
                line++;

                // 最終行を超えた
                if (document.EndPoint.Line <= line)
                {
                    if (isCommentNow)
                    {
                        return document.GetAllText();
                    }
                    if (finalCommentLine == 0)
                    {
                        return string.Empty;
                    }
                    return editPoint.GetLines(1, finalCommentLine + 1);
                }

                var lineText = editPoint.GetLines(line, line + 1);


                // 空白行
                if (Regex.IsMatch(lineText, @"^\s*$"))
                {
                    continue;
                }

                finalCommentLine = line;

                var mathes = matchPattern.Matches(lineText);

                foreach (Match match in mathes)
                {
                    // コメント状態
                    if (isCommentNow)
                    {
                        // */ ならコメント状態を終わらせる
                        if (match.Value == endComment)
                        {
                            isCommentNow = false;
                        }
                        continue;
                    }


                    // 一行コメントなら次の行へ
                    if (match.Value == beginLineComment)
                    {
                        break;
                    }

                    // /* ならコメント開始
                    if (match.Value == beginComment)
                    {
                        isCommentNow = true;
                        continue;
                    }

                    // どれでもなく、空白でもないときコメント確定
                    if (!string.IsNullOrWhiteSpace(match.Value))
                    {
                        editPoint.MoveToLineAndOffset(line, match.Index + 1);

                        var result= editPoint.GetText(-editPoint.AbsoluteCharOffset);

                        if (string.IsNullOrWhiteSpace(result))
                            return string.Empty;

                        return result;
                    }
                }

            }
        }
    }
}
