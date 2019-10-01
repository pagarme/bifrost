using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using PagarMe.Generic;

namespace PagarMe.Bifrost
{
    public class Options
    {
        [Option('p', "port", Required = false, HelpText = "Port to bind the server", Default = 2000)]
        public int BindPort { get; set; }

        [Option('b', "bind", Required = false, HelpText = "Address to bind the server", Default = "localhost")]
        public string BindAddress { get; set; }

        [Option('e', "endpoint", Required = false, HelpText = "Pagar.me's API endpoint", Default = "https://api.pagar.me/1/")]
        public string Endpoint { get; set; }

        [Option('d', "data-path", Required = false, HelpText = "Database path", Default = "<appdata>")]
        public string DataPath { get; set; }

        [Option('u', "update-address", Required = false, HelpText = "Address check for updates", Default = "http://localhost:2001")]
        public string UpdateAddress { get; set; }

        public Boolean Fail => Errors?.Any() ?? false;

        public IEnumerable<Error> Errors { get; private set; }

        public Options() { }

        public static Options Get(String[] args)
        {
            var options = new Options();

            var parser = Parser.Default.ParseArguments<Options>(args);

            parser.WithParsed(o =>
            {
                options = o;
            });

            parser.WithNotParsed(e =>
            {
                options.Errors = e;
            });

            if (options.Fail)
            {
                Log.Me.Warn("Could not get parameters. Verify parameters passed.");
            }

            options.EnsureDefaults();

            return options;
        }

        private void EnsureDefaults()
        {
            if (DataPath == null || DataPath == "<appdata>")
                DataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PagarMe.Bifrost"
                );
        }
    }
}
