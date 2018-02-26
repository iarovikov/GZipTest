using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

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

        private Thread[] _workers;
        private int _numberOfWorkers;

        public Compressor(int numberOfWorkers)
        {
        }

        public void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile, int numberOfWorkers)
        {
            // Create and start a separate thread for each worker
            for (var i = 0; i < numberOfWorkers; i++)
            {
                (this._workers[i] = new Thread(this.CompressChunk)).Start();
            }


            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = fileToCompress.OpenRead())
            {
                // Producer-consumers
                // Producer reads file by chunks and saves them to queue.
                // Consumers take chunsk from queue and perform compression
                var result = new List<Chunk>();
                using (var chunkProducerConsumer = new ChunkQueue())
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

        private void Read(FileInfo inputFile)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = inputFile.OpenRead())
            {
                using (var chinkQueue = new ChunkQueue())
                {
                    int numRead = 0;
                    while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        byte[] chunk = new byte[numRead];
                        Buffer.BlockCopy(buffer, 0, chunk, 0, numRead);
                        chinkQueue.Enqueue(chunk);
                    }
                }
            }
        }

        private void CompressChunk()
        {

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