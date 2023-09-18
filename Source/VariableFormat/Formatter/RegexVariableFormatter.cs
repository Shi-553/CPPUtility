using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{
    public class RegexVariableFormatter : VariableFormatterBase<RegexReplaceInputInfo>
    {
        public override void FormatVariable(VariableFormatHelper variableFormatHelper, ICodeVariableLike codeVariable)
        {
            variableFormatHelper.Variable = InputInfo.MatchRegex.Replace(variableFormatHelper.Variable, InputInfo.ReplacementText);
        }

        public override string FormattertName => "Replace";
    }
}
