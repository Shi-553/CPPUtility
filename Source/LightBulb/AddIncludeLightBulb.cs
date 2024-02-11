using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using TextSelection = EnvDTE.TextSelection;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace CPPUtility
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Test Suggested Actions")]
    [ContentType("text")]
    internal class AddIncludeSuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
    {
        [Import(typeof(ITextStructureNavigatorSelectorService))]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            if (textBuffer == null || textView == null)
            {
                return null;
            }
            return new AddIncludeSuggestedActionsSource(this, textView, textBuffer);
        }
    }

    internal class AddIncludeSuggestedActionsSource : ISuggestedActionsSource
    {
        private readonly AddIncludeSuggestedActionsSourceProvider m_factory;
        private readonly ITextBuffer m_textBuffer;
        private readonly ITextView m_textView;
        public AddIncludeSuggestedActionsSource(AddIncludeSuggestedActionsSourceProvider testSuggestedActionsSourceProvider, ITextView textView, ITextBuffer textBuffer)
        {
            m_factory = testSuggestedActionsSourceProvider;
            m_textBuffer = textBuffer;
            m_textView = textView;
        }
        private bool TryGetWordUnderCaret(out TextExtent wordExtent)
        {
            ITextCaret caret = m_textView.Caret;
            SnapshotPoint point;

            if (caret.Position.BufferPosition > 0)
            {
                point = caret.Position.BufferPosition - 1;
            }
            else
            {
                wordExtent = default;
                return false;
            }

            ITextStructureNavigator navigator = m_factory.NavigatorService.GetTextStructureNavigator(m_textBuffer);

            wordExtent = navigator.GetExtentOfWord(point);
            return true;
        }
        public async Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {

            TextExtent extent;
            if (TryGetWordUnderCaret(out extent))
            {
                // don't display the action if the extent has whitespace
                return extent.IsSignificant;
            }
            return false;
        }
        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            TextExtent extent;
            if (TryGetWordUnderCaret(out extent) && extent.IsSignificant)
            {
                ITrackingSpan trackingSpan = range.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
                var addIncludeAction = new AddIncludeSuggestedAction(trackingSpan);
                return new SuggestedActionSet[] { new SuggestedActionSet("Any", new ISuggestedAction[] { addIncludeAction }) };
            }
            return Enumerable.Empty<SuggestedActionSet>();
        }
        public event EventHandler<EventArgs> SuggestedActionsChanged;
        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }

    internal class AddIncludeSuggestedAction : ISuggestedAction, ICancellableCommand
    {
        private ITrackingSpan m_span;
        private string m_display;
        private ITextSnapshot m_snapshot;
        public AddIncludeSuggestedAction(ITrackingSpan span)
        {
            m_span = span;
            m_snapshot = span.TextBuffer.CurrentSnapshot;
            m_display = string.Format("Add '{0}' includes", span.GetText(m_snapshot));
        }
        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }
        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }
        public bool HasActionSets
        {
            get { return false; }
        }
        public string DisplayText
        {
            get { return m_display; }
        }
        public ImageMoniker IconMoniker
        {
            get { return default(ImageMoniker); }
        }
        public string IconAutomationText
        {
            get
            {
                return null;
            }
        }
        public string InputGestureText
        {
            get
            {
                return null;
            }
        }
        public bool HasPreview
        {
            get { return true; }
        }
        public void Invoke(CancellationToken cancellationToken)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await CommandManager.Instance.ExecuteWithCancellationAsync(this, cancellationToken);

            }).FireAndForget();
        }


        public int GetLastIncludeLine(TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var editPoint = document.StartPoint.CreateEditPoint();

            bool isIncludeLine = false;

            string lineText;
            while (true)
            {
                lineText = editPoint.GetLines(editPoint.Line, editPoint.Line + 1);

                bool isIncludeLineNow = lineText.StartsWith("#include");

                if (isIncludeLine && !isIncludeLineNow)
                {
                    return editPoint.Line;
                }

                if (isIncludeLineNow)
                {
                    isIncludeLine = true;
                }

                editPoint.LineDown();

                if (document.EndPoint.Line <= editPoint.Line)
                {
                    return 1;
                }
            }
        }


        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample action and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }

        public async Task ExecuteWithCancellationAsync(CancellationToken token)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var currnetDocument = await DocumentUtility.GetActiveTextDocumentAsync();
            if (currnetDocument == null)
            {
                return;
            }


            await VS.Commands.ExecuteAsync("Edit.GoToDefinition");

            await Task.Delay(100);


            TextDocument definitionDocument;

            while (true)
            {
                definitionDocument = await DocumentUtility.GetActiveTextDocumentAsync();

                if (definitionDocument != null && definitionDocument != currnetDocument)
                {
                    break;
                }

                await Task.Delay(1000);

                token.ThrowIfCancellationRequested();
            }

            await Task.Delay(100);

            await VS.Commands.ExecuteAsync("View.NavigateBackward");


            if (definitionDocument == null)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            var filePath = definitionDocument.Parent.FullName;


            if (await UnrealUtility.IsUnrealSolutionAsync())
            {
                filePath = UnrealUtility.GetShortestIncludePath(filePath);
            }


            int insertLine = GetLastIncludeLine(currnetDocument);

            var editPoint = currnetDocument.CreateEditPoint();
            editPoint.MoveToLineAndOffset(insertLine, 1);


            editPoint.Insert($"#include \"{filePath}\"{Environment.NewLine}");
        }
    }
}
