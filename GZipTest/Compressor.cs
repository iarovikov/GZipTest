using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipTest
{
    public class Compressor : GZipWorker, ICompressor
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
                        byte[] buffer = new byte[BUFFER_SIZE];
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

        private readonly int numberOfWorkers;
        public Compressor(int numberOfWorkers)
        {
            this.numberOfWorkers = numberOfWorkers;
        }

        private void Read(FileInfo inputFile)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = inputFile.OpenRead())
            {
                using (var chinkQueue = new ChunkQueue(this.numberOfWorkers, CompressionMode.Compress))
                {

                }
            }
        }

        public void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile)
        {

            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = fileToCompress.OpenRead())
            {
                // Producer-consumers
                // Producer reads file by chunks and saves them to queue.
                // Consumers take chunsk from queue and perform compression
                var result = new List<Chunk>();
                using (var chunkProducerConsumer = new ChunkQueue(2, CompressionMode.Compress, result))
                {
                    while (inputStream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        chunkProducerConsumer.Enqueue(buffer);
                        buffer = new byte[BUFFER_SIZE];
                    }
                }

                WriteOutputFile(compressedFile, result);
            }
        }

        private static void WriteOutputFile(FileInfo compressedFile, List<Chunk> result)
        {
            using (FileStream outFile = File.Create(compressedFile.FullName))
            {
                foreach (var chunk in result.OrderBy(x => x.Id))
                {
                    outFile.Write(chunk.Data, 0, chunk.Data.Length);
                }
            }
        }
    }
}