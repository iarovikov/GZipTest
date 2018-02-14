using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

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
                using (FileStream outFile = File.Create(compressedFile.FullName))
                {
                    using (GZipStream gZipStream = new GZipStream(outFile, CompressionMode.Compress))
                    {
                        byte[] buffer = new byte[64 * 1024];
                        int numRead;
                        while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
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
            const int BUFFER_SIZE = 64 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = fileToCompress.OpenRead())
            {
                using (FileStream outFile = File.Create(compressedFile.FullName))
                {
                    // Producer-consumers
                    // Producer reads file by chunks and saves them to queue.
                    // Consumers take chunsk from queue and perform compression
                    var result = new Queue<byte[]>();
                    using (var chunkProducerConsumer = new ChunkProducerConsumer(1, CompressionMode.Compress, result))
                    {
                        while (inputStream.Read(buffer, 0, buffer.Length) > 0)
                        {
                            chunkProducerConsumer.Enqueue(buffer);
                            buffer = new byte[BUFFER_SIZE];
                        }
                    }

                    WriteOutFile(result, outFile);
                }
            }
        }

        public void Decompress(FileInfo fileToDecompress, FileInfo decompressedFile)
        {
            using (FileStream inputStream = fileToDecompress.OpenRead())
            {
                using (FileStream outFile = File.Create(decompressedFile.FullName))
                {
                    using (var gZipStream = new GZipStream(outFile, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[64 * 1024];
                        int numRead;
                        while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            gZipStream.Write(buffer, 0, numRead);
                        }

                        Console.WriteLine(
                            "Decompressed {0} from {1} to {2} bytes.",
                            fileToDecompress.Name,
                            fileToDecompress.Length,
                            outFile.Length);
                    }
                }
            }
        }

        public void ParallelDecompress(FileInfo fileToDecompress, FileInfo decompressedFile)
        {
            const int BUFFER_SIZE = 64 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = fileToDecompress.OpenRead())
            {
                using (FileStream outFile = File.Create(decompressedFile.FullName))
                {
                    // Producer-consumers
                    // Producer reads file by chunks and saves them to queue.
                    // Consumers take chunsk from queue and perform compression
                    var result = new Queue<byte[]>();
                    using (var chunkProducerConsumer = new ChunkProducerConsumer(1, CompressionMode.Decompress, result))
                    {
                        while (inputStream.Read(buffer, 0, buffer.Length) > 0)
                        {
                            chunkProducerConsumer.Enqueue(buffer);
                            buffer = new byte[BUFFER_SIZE];
                        }
                    }

                    WriteOutFile(result, outFile);
                }
            }
        }

        private static void WriteOutFile(Queue<byte[]> result, FileStream outFile)
        {
            while (result.Count > 0)
            {
                var chunk = result.Dequeue();
                outFile.Write(chunk, 0, chunk.Length);
            }
        }
    }
}