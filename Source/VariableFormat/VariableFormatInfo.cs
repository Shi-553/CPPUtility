using System.Runtime.Serialization;

namespace CPPUtility
{
    public class VariableFormatInfo
    {
        public VariableSelectorBase Selector { get; set; }
        public VariableFormatterBase Formatter { get; set; }

    }
}