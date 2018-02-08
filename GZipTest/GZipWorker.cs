using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class GZipWorker : ICompressor, IDecompressor
    {
        public void Compress(FileInfo fileToCompress)
        {
            this.Compress(fileToCompress, new FileInfo(fileToCompress.FullName + ".gz"));
        }

        public void Compress(FileInfo fileToCompress, FileInfo compressedFile)
        {
            using (FileStream inputStream = fileToCompress.OpenRead())
            {
                ValidateInputFile(fileToCompress);
                using (FileStream outFile = File.Create(compressedFile.FullName))
                {
                    using (GZipStream gZipStream = new GZipStream(outFile, CompressionMode.Compress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            gZipStream.Write(buffer, 0, numRead);
                        }

                        Console.WriteLine(
                            "Compressed {0} from {1} to {2} bytes.",
                            fileToCompress.Name,
                            fileToCompress.Length,
                            outFile.Length);
                    }
                }
            }
        }

        public void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile)
        {
            int numberOfThreads = 32;
            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                var thread = new Thread(Compress);
                thread.Start(i);
                threads.Add(thread);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private static void Compress(object i)
        {
            Console.WriteLine(i);
        }

        public void Decompress(FileInfo fileToDecompress)
        {
            throw new System.NotImplementedException();
        }

        private static void ValidateInputFile(FileInfo fileToCompress)
        {
            if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden
                || fileToCompress.Extension == ".gz")
            {
                throw new InvalidOperationException("File is hidden or already compressed");
            }
        }
    }
}