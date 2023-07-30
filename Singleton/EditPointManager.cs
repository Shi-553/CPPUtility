using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using static Microsoft.VisualStudio.VSConstants;

namespace CPPUtility
{
    internal class EditPointManager : Singleton<EditPointManager>
    {
        public EditPointManager()
        {
            CommandFilter.CommandEvent += CommandFilter_CommandEvent;
        }
        ~EditPointManager()
        {
            CommandFilter.CommandEvent -= CommandFilter_CommandEvent;
        }

        Queue<EditPoint> editPoints;
        public enum EndType
        {
            Success,
            SuccessInOtherLine,
            Failure,
        }
        EndType allowEndType;
        Action<EndType> onEndEdit;
        bool isEditing = false;

        EditPoint currentEditPoint;
        public bool ExecuteEdit(List<EditPoint> editPoints, EndType allowEndType, Action<EndType> onEndEdit = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (editPoints == null || editPoints.Count == 0)
            {
                return false;
            }

            isEditing = true;
            this.editPoints = new Queue<EditPoint>(editPoints);
            this.onEndEdit = onEndEdit;
            this.allowEndType = allowEndType;
            currentEditPoint = null;


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

            editPoints.Clear();

            bool isAllow = currentEditPoint != null && ((int)type) <= ((int)allowEndType);
            if (isAllow)
            {
                var endPoint = currentEditPoint.Parent.Selection.ActivePoint.CreateEditPoint();
                endPoint.LineDown();
                endPoint.EndOfLine();
                endPoint.TryToShow(vsPaneShowHow.vsPaneShowCentered);
                currentEditPoint.Parent.Selection.MoveToAbsoluteOffset(endPoint.AbsoluteCharOffset);
                onEndEdit?.Invoke(type);
            }

            isEditing = false;
            return isAllow;
        }

        bool Next()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!isEditing)
                return false;

            if (editPoints == null || editPoints.Count == 0)
            {
                if (currentEditPoint != null && currentEditPoint.Parent.Selection.CurrentLine != currentEditPoint.Line)
                {
                    return End(EndType.SuccessInOtherLine);
                }

                return End(EndType.Success);
            }

            currentEditPoint = editPoints.Dequeue();

            if (currentEditPoint.DTE.ActiveDocument != currentEditPoint.Parent.Parent)
            {
                End(EndType.Failure);
                return false;
            }

            bool isLeteral = currentEditPoint.GetText(BasicLiteralFormatter.EDIT_POINT_LITERAL.Length) == BasicLiteralFormatter.EDIT_POINT_LITERAL;

            if (!isLeteral)
            {
                End(EndType.Failure);
                return false;
            }

            currentEditPoint.TryToShow(vsPaneShowHow.vsPaneShowCentered);


            currentEditPoint.Parent.Selection.MoveToAbsoluteOffset(currentEditPoint.AbsoluteCharOffset);

            if (isLeteral)
                currentEditPoint.Parent.Selection.CharRight(true, BasicLiteralFormatter.EDIT_POINT_LITERAL.Length);

            return true;
        }
    }
}
