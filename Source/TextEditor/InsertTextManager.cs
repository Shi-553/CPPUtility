using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CPPUtility
{
    internal struct InsertInfo
    {
        public string insertText;
        public EditPoint editPoint;

        public InsertInfo(EditPoint editPoint, string insertText)
        {
            this.editPoint = editPoint;
            this.insertText = insertText;
        }
    }
    

    internal class InsertTextManager
    {
        readonly List<InsertInfo> insertInfos = new List<InsertInfo>();

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

            InsertReservation(new InsertInfo(functionStartPoint, insertText));

            return true;
        }

        public void InsertReservation(EditPoint editPoint, string comment)
        {
            insertInfos.Add(new InsertInfo(editPoint, comment));
        }
        public void InsertReservation(InsertInfo insertInfo)
        {
            insertInfos.Add(insertInfo);
        }

        public int GetInsertReservationCount()
        {
            return insertInfos.Count;
        }


        public List<EditSnippetInfo> ExecuteInsertAndFindEditPoints()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<EditSnippetInfo> editSnippetPoints = new List<EditSnippetInfo>();

            foreach (var info in insertInfos)
            {
                var offset = info.editPoint.AbsoluteCharOffset;
                info.editPoint.Insert(info.insertText);


                List<EditPoint> editPoints = new List<EditPoint>();

                var mathes = Regex.Matches(info.insertText.Replace("\r\n", "\n"), BasicLiteralFormatter.EDIT_POINT_LITERAL);
                foreach (Match match in mathes)
                {
                    var edit = info.editPoint.CreateEditPoint();
                    edit.MoveToAbsoluteOffset(offset + match.Index);
                    editPoints.Add(edit);
                }

                editSnippetPoints.Add(new EditSnippetInfo(info,editPoints));
            }

            insertInfos.Clear();

            return editSnippetPoints;
        }
        public void ExecuteInsert()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var info in insertInfos)
            {
                info.editPoint.Insert(info.insertText);
            }

            insertInfos.Clear();
        }
    }
}
