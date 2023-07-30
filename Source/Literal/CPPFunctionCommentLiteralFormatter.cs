using System.Collections.Generic;

namespace CPPUtility
{
    internal class CPPFunctionCommentLiteralData : BasicLiteralData
    {
        public string comment;
        public CPPFunctionCommentLiteralData(string comment)
        {
            this.comment = comment;
        }
    }
    internal class CPPFunctionCommentLiteralFormatter : BasicLiteralFormatter, ILiteralFormatter<CPPFunctionCommentLiteralData>
    {
        public const string COMMENT_LITERAL = "{comment}";

        public string FormatLiteral(string snippet, CPPFunctionCommentLiteralData data)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            snippet = base.FormatLiteral(snippet, data);

            snippet = snippet.Replace(COMMENT_LITERAL, data.comment);

            return snippet;
        }

        protected override void LoadLiteralSamplesData(List<LiteralSampleData> samples)
        {
            base.LoadLiteralSamplesData(samples);

            samples.Add(new LiteralSampleData(COMMENT_LITERAL, "Function Definition Comments"));
        }
    }
}
