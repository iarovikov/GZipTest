using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Schema;

namespace GZipTest
{
    public class ChunkProducerConsumer : IDisposable
    {
        private readonly object @lock = new object();

        private readonly Thread[] _workers;

        private readonly Queue<byte[]> _chunkQueue = new Queue<byte[]>();

        private readonly IList<Chunk> _result = new List<Chunk>();

        private readonly CompressionMode _compressionMode;

        public ChunkProducerConsumer(int workerCount, CompressionMode compressionMode, IList<Chunk> result)
        {
            this._workers = new Thread[workerCount];
            this._compressionMode = compressionMode;
            this._result = result;

            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
            {
                (this._workers[i] = new Thread(this.Consume)).Start();
            }
        }

        public void Dispose()
        {
            // Enqueue one null task per worker to make each exit.
            foreach (Thread worker in this._workers)
            {
                this.Enqueue(null);
            }

            foreach (Thread worker in this._workers)
            {
                worker.Join();
            }
        }

        public void Enqueue(byte[] chunk)
        {
            lock (this.@lock)
            {
                this._chunkQueue.Enqueue(chunk);
                Monitor.PulseAll(this.@lock);
            }
        }

        private void Consume()
        {
            int index = 0;
            while (true)
            {
                byte[] chunk;
                lock (this.@lock)
                {
                    while (this._chunkQueue.Count == 0)
                    {
                        Monitor.Wait(this.@lock);
                    }

                    chunk = this._chunkQueue.Dequeue();
                    Interlocked.Increment(ref index);
                }

                if (chunk == null)
                    return;

                if (this._compressionMode == CompressionMode.Compress)
                {
                    byte[] size = new byte[4];
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var zipStream = new GZipStream(memoryStream, this._compressionMode))
                        {
                            zipStream.Write(chunk, 0, chunk.Length);
                        }

                        byte[] data = memoryStream.ToArray();
                        size = BitConverter.GetBytes(data.Length);
                        byte[] result = new byte[size.Length + data.Length];
                        Buffer.BlockCopy(size, 0, result, 0, size.Length);
                        Buffer.BlockCopy(data, 0, result, size.Length, data.Length);
                        this._result.Add(new Chunk(index, result));
                    }
                }
                else if (this._compressionMode == CompressionMode.Decompress)
                {
                    //                    byte[] size = new byte[4];
                    //                    Buffer.BlockCopy(chunk, 0, size, 0, size.Length);
                    //                    int s = BitConverter.ToInt32(size, 0);
                    //                    byte[] data = new byte[s];
                    //                    Buffer.BlockCopy(chunk, 4, data, 0, s);
                    using (var inStream = new MemoryStream(chunk))
                    {
                        using (var outStream = new MemoryStream())
                        {
                            using (var zipStream = new GZipStream(inStream, this._compressionMode))
                            {
                                byte[] buffer = new byte[chunk.Length];
                                int numRead;
                                while ((numRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    outStream.Write(buffer, 0, numRead);
                                }
                            }

                            this._result.Add(new Chunk(index, outStream.ToArray()));
                        }
                    }
                }

//                }
            }
        }
    }
}