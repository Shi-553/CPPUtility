using System.Collections.Generic;

namespace CPPUtility
{
    public class VariableFormatHelper
    {
        List<string> variables = new List<string>();
        int originalVariableIndex = 0;

        public VariableFormatHelper(string originalVariable)
        {
            variables.Add(originalVariable);
        }
        public string Variable
        {
            get => variables[originalVariableIndex];
            set => variables[originalVariableIndex] = value;
        }

        public enum InsertType
        {
            BeforeBegin,
            AfterBegin,
            BeforeEnd,
            AfterEnd
        }

        int GetInsertIndex(InsertType insertType)
        {
            switch (insertType)
            {
                case InsertType.BeforeBegin:
                    return 0;

                case InsertType.BeforeEnd:
                    return originalVariableIndex;


                case InsertType.AfterBegin:
                    return originalVariableIndex + 1;


                case InsertType.AfterEnd:
                    return variables.Count - 1;

                default:
                    return 0;
            }
        }
        public void InsertVariable(string inserting, InsertType insertType)
        {
            variables.Insert(GetInsertIndex(insertType), inserting);

            if (insertType == InsertType.BeforeBegin || insertType == InsertType.BeforeEnd)
            {
                originalVariableIndex++;
            }
        }

        public string GetCombinedVariable(VariableDelimiterType delimiterType)
        {
            string beforeString = "";
            for (int i = 0; i < originalVariableIndex; i++)
            {
                beforeString += variables[i];
            }

            string afterString = "";
            for (int i = originalVariableIndex + 1; i < variables.Count; i++)
            {
                afterString += variables[i];
            }


            return delimiterType.Combine(beforeString, Variable, afterString);
        }
    }
}
