using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

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

        public void ParallelDecompress(FileInfo fileToDecompress, FileInfo decompressedFile)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            using (FileStream inputStream = fileToDecompress.OpenRead())
            {

                    // Producer-consumers
                    // Producer reads file by chunks and saves them to queue.
                    // Consumers take chunsk from queue and perform compression
                    var result = new List<Chunk>();
                    using (var chunkProducerConsumer = new ChunkProducerConsumer(8, CompressionMode.Decompress, result))
                    {
                        byte[] size = new byte[4];
                        //                        inputStream.Read(buffer, 0, 4);
                        //                        int sizeOfFirstChunk = BitConverter.ToInt32(buffer, 0);
                        //                        buffer = new byte[sizeOfFirstChunk + 4];
                        //                        inputStream.Position = 0;
                        while (inputStream.Read(size, 0, size.Length) > 0)
                        {
                            int s = BitConverter.ToInt32(size, 0);
                            byte[] data = new byte[s];
                            inputStream.Read(data, 0, s);
                            chunkProducerConsumer.Enqueue(data);
                        }
                    }

                WriteOutputFile(decompressedFile, result);
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