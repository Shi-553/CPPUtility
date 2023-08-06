using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using static CPPUtility.EditSnippetManager;
using static Microsoft.VisualStudio.VSConstants;

namespace CPPUtility
{
    internal class EditSnippetInfo
    {
        public enum NextResultType
        {
            Running,
            Success,
            Failure,
        }
        InsertInfo insertInfo;
        readonly Queue<EditPoint> editSnippetPoints;

        public bool IsEmpty => !editSnippetPoints.Any();
        public bool IsSelectionSameLine
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CurrentEditPoint != null && CurrentEditPoint.Parent.Selection.CurrentLine == CurrentEditPoint.Line;
            }
        }

        public EditPoint CurrentEditPoint { private set; get; }


        public EditSnippetInfo(InsertInfo insertInfo, List<EditPoint> editSnippetPoints)
        {
            this.insertInfo = insertInfo;
            this.editSnippetPoints = new Queue<EditPoint>(editSnippetPoints);
        }

        public NextResultType Next()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!editSnippetPoints.Any())
            {
                var text = insertInfo.editPoint.GetText(-insertInfo.insertText.Length);
                if (text == insertInfo.insertText)
                {
                    insertInfo.editPoint.Delete(-insertInfo.insertText.Length);
                }

                return NextResultType.Success;
            }

            CurrentEditPoint = editSnippetPoints.Dequeue();

            if (CurrentEditPoint.DTE.ActiveDocument != CurrentEditPoint.Parent.Parent)
            {
                return NextResultType.Failure;
            }

            bool isLeteral = CurrentEditPoint.GetText(BasicLiteralFormatter.EDIT_POINT_LITERAL.Length) == BasicLiteralFormatter.EDIT_POINT_LITERAL;

            if (!isLeteral)
            {
                return NextResultType.Failure;
            }

            CurrentEditPoint.TryToShow(vsPaneShowHow.vsPaneShowCentered);


            CurrentEditPoint.Parent.Selection.MoveToAbsoluteOffset(CurrentEditPoint.AbsoluteCharOffset);

            if (isLeteral)
                CurrentEditPoint.Parent.Selection.CharRight(true, BasicLiteralFormatter.EDIT_POINT_LITERAL.Length);


            return NextResultType.Running;
        }
    }

    internal class EditSnippetManager : Singleton<EditSnippetManager>
    {
        public EditSnippetManager()
        {
            CommandFilter.CommandEvent += CommandFilter_CommandEvent;
        }
        ~EditSnippetManager()
        {
            CommandFilter.CommandEvent -= CommandFilter_CommandEvent;
        }

        Queue<EditSnippetInfo> snippetPoints;
        EditSnippetInfo currentSnippetPoint;

        public enum EndType
        {
            Success,
            SuccessInOtherLine,
            Failure,
        }
        EndType allowEndType;
        Action<EndType> onEndEdit;
        bool isEditing = false;

        public bool ExecuteEdit(List<EditSnippetInfo> editPoints, EndType allowEndType, Action<EndType> onEndEdit = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var queue = new Queue<EditSnippetInfo>(editPoints.Where(point => !point.IsEmpty));

            if (!queue.Any())
            {
                return false;
            }

            isEditing = true;
            this.snippetPoints = queue;
            this.onEndEdit = onEndEdit;
            this.allowEndType = allowEndType;
            currentSnippetPoint = snippetPoints.Dequeue();

            return Next();
        }

        private bool CommandFilter_CommandEvent(Guid arg1, uint arg2)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!isEditing)
                return false;

            if (arg2 == (uint)VSStd2KCmdID.RETURN)
            {
                return Next();
            }
            if (arg2 == (uint)VSStd2KCmdID.CANCEL)
            {
                End(EndType.Failure);
                return false;
            }

            return false;
        }

        bool End(EndType type)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!isEditing)
                return false;

            snippetPoints.Clear();

            bool isAllow = ((int)type) <= ((int)allowEndType);
            if (isAllow)
            {
                var endPoint = currentSnippetPoint.CurrentEditPoint.Parent.Selection.ActivePoint.CreateEditPoint();
                endPoint.LineDown();
                endPoint.EndOfLine();
                endPoint.TryToShow(vsPaneShowHow.vsPaneShowCentered);
                endPoint.Parent.Selection.MoveToAbsoluteOffset(endPoint.AbsoluteCharOffset);

                onEndEdit?.Invoke(type);
            }

            isEditing = false;
            return isAllow;
        }

        bool Next()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!isEditing || snippetPoints == null)
            {
                return false;
            }

            var result = currentSnippetPoint.Next();

            switch (result)
            {
                case EditSnippetInfo.NextResultType.Running:
                    return true;

                case EditSnippetInfo.NextResultType.Success:
                    if (snippetPoints.Any())
                    {
                        currentSnippetPoint = snippetPoints.Dequeue();
                        return Next();
                    }

                    return End(currentSnippetPoint.IsSelectionSameLine ? EndType.Success : EndType.SuccessInOtherLine);

                default:
                    return End(EndType.Failure);
            }
        }
    }
}
