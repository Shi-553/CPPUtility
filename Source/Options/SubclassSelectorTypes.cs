using System.Collections.ObjectModel;

namespace CPPUtility
{
    public class SubclassSelectorTypes<T> : ObservableCollection<string> where T : class, ISubClassSelectable
    {
        public SubclassSelectorTypes()
        {
            foreach (var type in StaticClass.GetSubclasses<T>())
            {
                if (type is ISubClassSelectable subClassSelectable)
                {
                    Add(subClassSelectable.Name);
                }
            }
        }
    }
    public class VariableSelectorTypes : SubclassSelectorTypes<VariableSelectorBase>
    {
    }
    public class VariableFormatterTypes : SubclassSelectorTypes<VariableFormatterBase>
    {
    }
}
