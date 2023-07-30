using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CPPUtility
{
    internal class BasicLiteralData
    {
        public bool isAppendNewLineAtEnd = true;
    }
    internal class BasicLiteralFormatter : SingletonBase, ILiteralFormatter<BasicLiteralData>
    {
        public const string EDIT_POINT_LITERAL = "{edit}";

        public const string DATE_LITERAL = "{date=yyyy/M/d}";
        public const string DATE_REGEX = "{date=(.+?)}";


        public virtual string FormatLiteral(string snippet, BasicLiteralData data)
        {
            snippet = Regex.Replace(snippet, DATE_REGEX, (Match match) => { return DateTime.Now.ToString(match.Groups[1].Value); });

            if (data.isAppendNewLineAtEnd && !snippet.EndsWith("\n"))
            {
                snippet += "\n";
            }

            return snippet;
        }

        protected virtual void LoadLiteralSamplesData(List<LiteralSampleData> samples)
        {
            samples.Add(new LiteralSampleData(EDIT_POINT_LITERAL, "Edit every time"));
            samples.Add(new LiteralSampleData(DATE_LITERAL, "DateTime.Now.ToString(\"yyyy/M/d\")"));
        }

        public List<LiteralSampleData> GetLiteralSamplesData()
        {
            List<LiteralSampleData> datas = new List<LiteralSampleData>();
            LoadLiteralSamplesData(datas);
            return datas;
        }
    }

    public class LiteralSampleData
    {
        public LiteralSampleData(string literal, string description)
        {
            Literal = literal;
            Description = description;
        }

        public string Literal { get; set; }
        public string Description { get; set; }
    }
}
