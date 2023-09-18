using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{
    public class PrefixVariableFormatter : VariableFormatterBase<TextInputInfo>
    {
        public override void FormatVariable(VariableFormatHelper variableFormatHelper, ICodeVariableLike codeVariable)
        {
            variableFormatHelper.InsertVariable(InputInfo.Text, VariableFormatHelper.InsertType.BeforeEnd);
        }
        public override string FormattertName => "Prefix";
    }
}
