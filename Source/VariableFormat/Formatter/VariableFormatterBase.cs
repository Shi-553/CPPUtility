using EnvDTE;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{

    public abstract class VariableFormatterBase : ISubClassSelectable
    {
        string ISubClassSelectable.Name => FormattertName;

        public abstract string FormattertName { get; }

        public abstract void FormatVariable(VariableFormatHelper variableFormatHelper, ICodeVariableLike codeVariable);

        public abstract IFrameworkElementInputInfo FrameworkElementInputInfo { get; }

    }
    public abstract class VariableFormatterBase<T> : VariableFormatterBase where T : IFrameworkElementInputInfo, new()
    {
        public override IFrameworkElementInputInfo FrameworkElementInputInfo => InputInfo;

        protected T InputInfo = new T();
    }
}
