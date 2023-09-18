using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CPPUtility
{
    public class RegexReplaceInputInfo : IFrameworkElementInputInfo
    {
        public string MatchText { get; set; } = "";

        [JsonIgnore]
        public Regex MatchRegex => new Regex(MatchText);

        public string ReplacementText { get; set; } = "";
    }
}
