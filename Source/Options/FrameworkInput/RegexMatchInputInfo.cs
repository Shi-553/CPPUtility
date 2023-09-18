using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CPPUtility
{
    public class RegexMatchInputInfo : IFrameworkElementInputInfo
    {
        public string MatchText { get; set; } = "";

        [JsonIgnore]
        public Regex MatchRegex => new Regex(MatchText);
    }
}
