namespace CPPUtility
{
    // クラス化したほうがよさそう
    public enum VariableDelimiterType
    {
        UpperCase,
        UnderScore
    }
    public static class VariableDelimiterTypeExtension
    {
        public static string GetDelimiterRegexString(this VariableDelimiterType type)
        {
            switch (type)
            {
                case VariableDelimiterType.UpperCase:
                    return "[A-Z]";

                case VariableDelimiterType.UnderScore:
                    return "_";

                default:
                    return "";
            }
        }
        public static string Combine(this VariableDelimiterType type, string before, string variable, string after)
        {
            switch (type)
            {
                case VariableDelimiterType.UpperCase:
                    {
                        var result = variable;

                        if (!string.IsNullOrEmpty(before))
                        {
                            result = before + result[0].ToString().ToUpper() + result.Substring(1);
                        }
                        if (!string.IsNullOrEmpty(after))
                        {
                            result = result + after;
                        }

                        return result;
                    }

                case VariableDelimiterType.UnderScore:
                    {
                        var result = variable;

                        if (!string.IsNullOrEmpty(before))
                        {
                            result = before + "_" + result;
                        }
                        if (!string.IsNullOrEmpty(after))
                        {
                            result = result + "_" + after;
                        }

                        return result;
                    }


                default:
                    return before + variable + after;
            }
        }
    }
}
