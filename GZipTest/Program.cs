using System;
using System.Diagnostics;
using System.IO;

namespace GZipTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("The format of the arguments should be: [command (compress/decompress)] [source file name] [destination file name]");
                return;
            }
            var command = args[0];
            var inputFile = new FileInfo(args[1]);
            var ouputFile = new FileInfo(args[2]);

            if (string.Equals(command, "compress", StringComparison.InvariantCultureIgnoreCase))
            {
                var compressor = new Compressor();
                compressor.ParallelCompress(inputFile, ouputFile, 2);
            }
            else if (string.Equals(command, "decompress", StringComparison.InvariantCultureIgnoreCase))
            {
                var decompressor = new Decompressor();
                decompressor.ParallelDecompress(inputFile, ouputFile, 2);
            }
            else
            {
                Console.WriteLine("Command is not recongized, please enter command 'compress' or 'decompress'.");
                return;
            }
            Console.ReadLine();
        }
    }
}