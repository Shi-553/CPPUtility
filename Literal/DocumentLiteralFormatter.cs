using EnvDTE;
using System.Collections.Generic;

namespace CPPUtility
{
    internal class DocumentLiteralData : BasicLiteralData
    {
        public TextDocument document;

        public DocumentLiteralData(TextDocument document)
        {
            this.document = document;
        }
    }
    internal class DocumentLiteralFormatter : BasicLiteralFormatter, ILiteralFormatter<DocumentLiteralData>
    {
        public const string FILENAME_LITERAL = "{file}";

        public string FormatLiteral(string snippet, DocumentLiteralData data)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            snippet = base.FormatLiteral(snippet, data);

            if (data.document != null)
                snippet = snippet.Replace(FILENAME_LITERAL, data.document.Parent.Name);

            return snippet;
        }

        protected override void LoadLiteralSamplesData(List<LiteralSampleData> samples)
        {
            base.LoadLiteralSamplesData(samples);

            samples.Add(new LiteralSampleData(FILENAME_LITERAL, "Filename"));
        }
    }
}
