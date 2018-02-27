using System;
using System.Diagnostics;
using System.IO;

namespace GZipTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //TODO: input parameters validation
            var command = args[0];
            var inputFile = new FileInfo(args[1]);
            //var ouputFile = new FileInfo(args[2]);


//            if (string.Equals(command, "compress", StringComparison.InvariantCultureIgnoreCase))
//            {
//                ValidateFileToCompress(inputFile);
//                gZipWorker.Compress(inputFile);
//            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (string.Equals(command, "compress", StringComparison.InvariantCultureIgnoreCase))
            {
                var compressor = new Compressor();
                compressor.ParallelCompress(inputFile, new FileInfo("compress.gz"), 1);
            }
            sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.Elapsed);


            if (string.Equals(command, "decompress", StringComparison.InvariantCultureIgnoreCase))
            {
                var decompressor = new Decompressor();
                decompressor.ParallelDecompress(new FileInfo("compress.gz"), new FileInfo("uncompressed.txt"));
            }

            Console.ReadLine();
        }

        private static void ValidateFileToCompress(FileInfo fileToCompress)
        {
            if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden
                || fileToCompress.Extension == ".gz")
            {
                throw new InvalidOperationException("File is hidden or already compressed");
            }
        }
    }
}