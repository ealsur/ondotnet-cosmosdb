using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace episode2
{
    public class Show
    {
        [JsonProperty("id")]
        internal string Id => $"Show_{Name}";

        [JsonProperty("Show")]
        public string Name {get;set;} 

        public DateTime LastUpdated {get;set;}

        public static Show Create(string name) => new Show(){ Name = name, LastUpdated = DateTime.UtcNow };
    }

    public class Episode
    {
        [JsonProperty("id")]
        internal string Id => $"Episode_{Show}_{Name}";

        public string Show {get;set;} 

        public string Name {get;set;} 

        public DateTime AirDate {get;set;}

        public static Episode Create(string show, string name) => new Episode() { Show = show, Name = name, AirDate = DateTime.MinValue };
    }
}