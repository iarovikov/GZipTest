using System;
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

            var gZipWorker = new GZipWorker();
//            if (string.Equals(command, "compress", StringComparison.InvariantCultureIgnoreCase))
//            {
//                ValidateFileToCompress(inputFile);
//                gZipWorker.Compress(inputFile);
//            }

            if (string.Equals(command, "compress", StringComparison.InvariantCultureIgnoreCase))
            {
                gZipWorker.ParallelCompress(inputFile, new FileInfo("compress.gz"));
            }

            if (string.Equals(command, "decompress", StringComparison.InvariantCultureIgnoreCase))
            {
                gZipWorker.ParallelDecompress(new FileInfo("compress_1.gz"), new FileInfo("uncompressed.txt"));
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