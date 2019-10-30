using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace episode2
{
    public class Model
    {
        public string id {get;set;}
        public string userName {get;set;}
        public string email {get;set;}

        public int age {get;set;}

        [JsonConverter(typeof(StringEnumConverter))]
        public Language favLanguage {get;set;}

        public override string ToString() => $"{id} - UserName {userName} || Email {email} || Age {age} || Favorite language {favLanguage}";

    }

    public enum Language
    {
        CSharp,
        FSharp,
        Go,
        Java,
        Python,
        Javascript,
        VisualBasic,
        Ruby,
        PHP,
        CPlusPlus
    }
}