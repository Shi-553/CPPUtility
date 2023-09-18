using Newtonsoft.Json;
using System;

namespace CPPUtility
{
    public abstract class EnumInputInfoBase : IFrameworkElementInputInfo
    {
        [JsonIgnore]
        public abstract Type EnumType { get; }
        [JsonIgnore]
        public abstract string EnumString { get; }
        [JsonIgnore]
        public string[] EnumAllStrings => Enum.GetNames(EnumType);

        public abstract void SetValue(string newEnumString);
    }
    public class EnumInputInfo<T> : EnumInputInfoBase where T : struct, Enum
    {
        public T EnumValue { get; set; }

        [JsonIgnore]
        public override string EnumString => Enum.GetName(EnumType, EnumValue);


        [JsonIgnore]
        public override Type EnumType => typeof(T);

        public override void SetValue(string newEnumString)
        {
            if (Enum.TryParse<T>(newEnumString, true, out var result))
            {
                EnumValue = result;
            }
        }
    }
}
