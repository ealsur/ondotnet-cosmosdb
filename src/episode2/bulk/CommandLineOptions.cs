using CommandLine;

namespace episode2
{
    public class CommandLineOptions
    {
        [Option('n', "nobulk", Required = false, HelpText = "Disables bulk support")]
        public bool NoBulk { get; set; }
    }
}