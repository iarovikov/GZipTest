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
        public void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile, int numberOfWorkers)
        {
            if (!fileToCompress.Exists)
            {
                Console.WriteLine("Source file does not exist. Please enter a valid file name.");
                return;
            }

            this.Read(fileToCompress);

            Thread[] workers = new Thread[numberOfWorkers];
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int toProcess = numberOfWorkers;
            // Create and start a separate thread for each worker
            for (var i = 0; i < numberOfWorkers; i++)
            {
                (workers[i] = new Thread(
                     () =>
                         {
                             this.CompressChunk();
                             if (Interlocked.Decrement(ref toProcess) == 0)
                                 resetEvent.Set();
                         })).Start();
            }

            // This is how to close output queue for writing file.
            // Othewise we somethimes get null queued before real data ends
            // And in that case not all file was written to disk.
            resetEvent.WaitOne();
            this.outputQueue.EnqueueNull();

            var writeThread = new Thread(new ParameterizedThreadStart(this.Write));
            writeThread.Start(compressedFile);
        }

        protected override void Read(FileInfo inputFile)
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
        }

        protected override void Write(object outputFileName)
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

            Console.WriteLine("File {0} created with {1} bytes length.", outputFile.Name, outputFile.Length);
            Console.ReadLine();
        }

        #region SingleThreaded

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

        #endregion
    }
}