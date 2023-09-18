using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPPUtility
{
    internal struct EditInfo
    {
        public EditPoint editPoint;
        public Action editAction;
        public string insertText;

        public EditInfo(EditPoint editPoint, string insertText)
        {
            this.editPoint = editPoint;
            this.insertText = insertText;
            editAction = null;
        }
        public EditInfo(EditPoint editPoint, Action editAction)
        {
            this.editPoint = editPoint;
            this.editAction = editAction;
            insertText = "";
        }

        public void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            editAction?.Invoke();
            editPoint.Insert(insertText);
        }
    }


    internal class EditTextManager
    {
        readonly List<EditInfo> insertInfos = new List<EditInfo>();

        public bool InsertReservationFunctionComment(EditPoint functionStartPoint, string comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            var lineText = functionStartPoint.GetLines(functionStartPoint.Line, functionStartPoint.Line + 1);

            var spaceMatch = Regex.Match(lineText, @"^(\s*)");

            var space = spaceMatch.Groups[1].Value;

            functionStartPoint.StartOfLine();


            var beforeLineText = functionStartPoint.GetLines(functionStartPoint.Line - 1, functionStartPoint.Line);

            // 前の行が既にコメント行ならreturn
            if (Regex.IsMatch(beforeLineText, @"^\s*(//|/\*)"))
            {
                return false;
            }

            // インデント合わせる
            var split = comment.Split('\n');

            string insertText = "";
            for (int i = 0; i < split.Length - 1; i++)
            {
                insertText += space + split[i] + "\n";
            }
            insertText += split[split.Length - 1];


            //  前の行が空行でないとき改行を追加
            if (!Regex.IsMatch(beforeLineText, @"^\s*$"))
            {
                insertText = "\n" + insertText;
            }
            if (!insertText.EndsWith("\n"))
            {
                insertText += "\n";
            }

            EditReservation(new EditInfo(functionStartPoint, insertText));

            return true;
        }

        public void EditReservation(EditInfo insertInfo)
        {
            insertInfos.Add(insertInfo);
        }

        public int GetEditReservationCount()
        {
            return insertInfos.Count;
        }


        public List<EditSnippetInfo> ExecuteEditAndFindEditPoints()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<EditSnippetInfo> editSnippetPoints = new List<EditSnippetInfo>();

            foreach (var info in insertInfos)
            {
                var offset = info.editPoint.AbsoluteCharOffset;

                info.Execute();

                List<EditPoint> editPoints = new List<EditPoint>();

                var mathes = Regex.Matches(info.insertText.Replace("\r\n", "\n"), BasicLiteralFormatter.EDIT_POINT_LITERAL);
                foreach (Match match in mathes)
                {
                    var edit = info.editPoint.CreateEditPoint();
                    edit.MoveToAbsoluteOffset(offset + match.Index);
                    editPoints.Add(edit);
                }

                editSnippetPoints.Add(new EditSnippetInfo(info, editPoints));
            }

            insertInfos.Clear();

            return editSnippetPoints;
        }
        public void ExecuteEdit()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var info in insertInfos)
            {
                info.Execute();
            }

            insertInfos.Clear();
        }
    }
}
