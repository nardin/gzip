using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gzip
{
    public class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (args.Length == 0)
            { 
                PrintHelp();
                return;
            }

            Console.CancelKeyPress += CancelHandler;

            try
            {
                switch (args[0])
                {
                    case "compress":
                        var param = ParseParams(args);
                        Lib.Gzip.Compress(
                            param["inFile"],
                            param["outFile"],
                            param.ContainsKey("size") ? int.Parse(param["size"]) : 1,
                            CancellationTokenSource);
                        break;
                    case "decompress":
                        Lib.Gzip.Decompress(
                            args[1],
                            args[2],
                            CancellationTokenSource);
                        break;
                    default:
                        PrintHelp();
                        break;
                }
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error: ");
                foreach (var innerException in ex.InnerExceptions)
                {
                    Console.WriteLine(innerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: ");
                Console.WriteLine(ex.Message);
            }

            if (CancellationTokenSource.IsCancellationRequested)
            {
                Environment.Exit(1);
                return;
            }

            watch.Stop();
            Console.WriteLine("Total time: " + watch.ElapsedMilliseconds + "ms");
            Environment.Exit(0);
        }


        protected static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            CancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Parse params.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ParseParams(string[] args)
        {
            var param = new Dictionary<string, string>();
            var options = args.Where(x => x.StartsWith("-"));

            var sizeString = options.FirstOrDefault(x => x.StartsWith("-size=", StringComparison.OrdinalIgnoreCase));
            if (sizeString != null)
            {
                var parts = sizeString.Split("=");
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int size))
                    {
                        param["size"] = size.ToString();
                    }
                    else
                    {
                        throw new ArgumentException("Parameter size should be number");
                    }
                }
                else
                {
                    throw new ArgumentException("Parameter size have incorrect format");
                }
            }

            var fileParams = args.Where(x => !x.StartsWith("-")).ToList();
            if (fileParams.Count != 3)
            {
                throw new ArgumentException("Incorrect format.");
            }

            param["inFile"] = fileParams[1];
            param["outFile"] = fileParams[2];

            return param;
        }

        /// <summary>
        /// Print help to console.
        /// </summary>
        private static void PrintHelp()
        {
            var helpMessage =
@"
Usage: gzip [compress|decompress] <inFile> <outFile>
       gzip [help]

compression/decompression tool using Lempel-Ziv coding (LZ77)                    

Options:
    -s, -size   file chunk size (MB)
";

            Console.WriteLine(helpMessage);
        }
    }
}
