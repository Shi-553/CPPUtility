namespace CPPUtility
{
    public abstract class IFrameworkElementInputInfo
    {
        public IFrameworkElementInputInfo()
        {
            Name = GetType().Name;
        }
        public string Name { get; set; } 
    }
}
