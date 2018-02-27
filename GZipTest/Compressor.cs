using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        private ChunkQueue inputQueue = new ChunkQueue();

        private ChunkQueue outputQueue = new ChunkQueue();

        public void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile, int numberOfWorkers)
        {
            this.Read(fileToCompress);

            Thread[] workers = new Thread[numberOfWorkers];
            // Create and start a separate thread for each worker
            for (var i = 0; i < numberOfWorkers; i++)
            {
                (workers[i] = new Thread(this.CompressChunk)).Start();
            }
            var writeThread = new Thread(new ParameterizedThreadStart(this.Write));
            writeThread.Start(compressedFile);
        }

        private void Read(FileInfo inputFile)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = inputFile.OpenRead())
            {
                int numRead = 0;
                while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] byteChunk = new byte[numRead];
                    Buffer.BlockCopy(buffer, 0, byteChunk, 0, numRead);
                    this.inputQueue.Enqueue(byteChunk);
                }

                this.inputQueue.EnqueueNull();
            }
        }

        private void CompressChunk()
        {
            Chunk inputChunk;
            while ((inputChunk = this.inputQueue.Dequeue()) != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    using (var binaryWriter = new BinaryWriter(zipStream))
                    {
                        binaryWriter.Write(inputChunk.Data, 0, inputChunk.Data.Length);
                    }

                    byte[] outputChunkData = memoryStream.ToArray();
                    var outputChunk = new Chunk(inputChunk.Id, outputChunkData);
                    this.outputQueue.Enqueue(outputChunk);
                }
            }
            this.outputQueue.EnqueueNull();
        }

        private void Write(object outputFileName)
        {
            var outputFile = (FileInfo)outputFileName;
            using (FileStream outFile = File.Create(outputFile.FullName))
            {
                var binaryFormatter = new BinaryFormatter();
                Chunk chunk;
                while ((chunk = this.outputQueue.Dequeue()) != null)
                {
                    binaryFormatter.Serialize(outFile, chunk);
                }
            }

            Console.WriteLine(
                "Compressed {0} to {1} bytes.",
                outputFile.Name,
                outputFile.Length);
            Console.ReadLine();
        }
    }
}