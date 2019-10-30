using CommandLine;

namespace episode2
{
    public class CommandLineOptions
    {
        [Option('p', "processor", Required = false, HelpText = "Start a processor with some name")]
        public string Processor { get; set; }

        [Option('w', "writer", Required = false, HelpText = "Start a writer of documents")]
        public int DocumentWriter { get; set; }

        [Option('e', "estimator", Required = false, HelpText = "Starts an estimator")]
        public bool Estimator { get; set; }
    }
}