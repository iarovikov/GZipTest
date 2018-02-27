using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace GZipTest
{
    public class Decompressor : GZipWorker, IDecompressor
    {
        public void Decompress(FileInfo fileToDecompress, FileInfo decompressedFile)
        {
            using (FileStream inputStream = fileToDecompress.OpenRead())
            {
                using (FileStream outFile = File.Create(decompressedFile.FullName))
                {
                    using (var gZipStream = new GZipStream(outFile, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
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

        private ChunkQueue inputQueue = new ChunkQueue();

        private ChunkQueue outputQueue = new ChunkQueue();

        public void ParallelDecompress(FileInfo fileToDecompress, FileInfo decompressedFile, int numberOfWorkers)
        {
            this.Read(fileToDecompress);
        }

        private void Read(FileInfo inputFile)
        {
            var formatter = new BinaryFormatter();
            using (FileStream inputStream = inputFile.OpenRead())
            {
                while (inputStream.Position < inputStream.Length)
                {
                    var chunk = (Chunk)formatter.Deserialize(inputStream);
                    this.inputQueue.Enqueue(chunk);
                }

                this.inputQueue.EnqueueNull();
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