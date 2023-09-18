using EnvDTE;

namespace CPPUtility
{
    public class NameRegexMatchVariableSelector : VariableSelectorBase<RegexMatchInputInfo>
    {
        public override string SelectorName => "Name";

        public override bool CanSelect(ICodeVariableLike codeVariableLike)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return InputInfo.MatchRegex.IsMatch(codeVariableLike.Name);
        }
    }
}
