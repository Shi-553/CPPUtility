using EnvDTE;

namespace CPPUtility
{
    public class TypeRegexMatchVariableSelector : VariableSelectorBase<RegexMatchInputInfo>
    {
        public override string SelectorName => "Type";

        public override bool CanSelect(ICodeVariableLike codeVariableLike)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return InputInfo.MatchRegex.IsMatch(codeVariableLike.Type.AsString);
        }
    }
}
