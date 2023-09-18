using System.Collections.Generic;

namespace CPPUtility
{

    internal interface ILiteralFormatter
    {
        string FormatLiteral(string snippet);
    }
    internal interface ILiteralFormatter<T>
    {
        string FormatLiteral(string snippet, T data);
    }
}
