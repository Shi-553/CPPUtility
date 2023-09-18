using EnvDTE;

namespace CPPUtility
{
    public enum ScopeType
    {
        Argument,
        Member,
        Local,
        Global
    }
    public class ScopeVariableSelector : VariableSelectorBase<EnumInputInfo<ScopeType>>
    {
        public override string SelectorName => "Scope";

        public override bool CanSelect(ICodeVariableLike codeVariableLike)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (InputInfo.EnumValue == ScopeType.Argument)
                return codeVariableLike.Kind == vsCMElement.vsCMElementParameter;


            if (!(codeVariableLike is CodeVariable codeVariable))
            {
                return false;
            }

            if (codeVariable.Parent is CodeClass || codeVariable.Parent is CodeStruct)
                return InputInfo.EnumValue == ScopeType.Member;


            if (codeVariable.Parent is CodeFunction)
                return InputInfo.EnumValue == ScopeType.Local;


            // TODO: Globalってどうすればいい？

            return InputInfo.EnumValue == ScopeType.Global;
        }
    }
}
