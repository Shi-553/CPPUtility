using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{
    public abstract class VariableSelectorBase : ISubClassSelectable
    {
        string ISubClassSelectable.Name => SelectorName;

        public abstract string SelectorName { get; }

        public abstract bool CanSelect(ICodeVariableLike codeVariableLike);

        public abstract IFrameworkElementInputInfo FrameworkElementInputInfo { get; }
    }

    public abstract class VariableSelectorBase<T> : VariableSelectorBase where T : IFrameworkElementInputInfo, new()
    {
        public override IFrameworkElementInputInfo FrameworkElementInputInfo => InputInfo;

        protected T InputInfo = new T();
    }
}
