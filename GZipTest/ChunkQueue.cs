using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class ChunkQueue
    {
        private readonly object @lock = new object();

        private readonly Queue<Chunk> chunkQueue = new Queue<Chunk>();

        private int chunkId;

        public void Finish()
        {
            lock (this.@lock)
            {
                this.Enqueue((Chunk)null);
            }
        }

        public void Enqueue(byte[] byteChunk)
        {
            lock (this.@lock)
            {
                var chunk = new Chunk(this.chunkId, byteChunk);
                this.chunkQueue.Enqueue(chunk);
                this.chunkId++;
                Monitor.PulseAll(this.@lock);
            }
        }

        public void Enqueue(Chunk chunk)
        {
            lock (this.@lock)
            {
                this.chunkQueue.Enqueue(chunk);
                Monitor.PulseAll(this.@lock);
            }
        }

        public Chunk Dequeue()
        {
            lock (this.@lock)
            {
                while (this.chunkQueue.Count == 0)
                {
                    Monitor.Wait(this.@lock);
                }

                var chunk = this.chunkQueue.Dequeue();
                return chunk;
            }
        }

        //        private void Consume()
        //        {
        //            int index = 0;
        //            while (true)
        //            {
        //                byte[] chunk;
        //                lock (this.@lock)
        //                {
        //                    while (this.chunkQueue.Count == 0)
        //                    {
        //                        Monitor.Wait(this.@lock);
        //                    }
        //
        //                    chunk = this.chunkQueue.Dequeue();
        //                    Interlocked.Increment(ref index);
        //                }
        //
        //                if (chunk == null)
        //                    return;
        //
        //                if (this._compressionMode == CompressionMode.Compress)
        //                {
        //                    byte[] size = new byte[4];
        //                    using (var memoryStream = new MemoryStream())
        //                    {
        //                        using (var zipStream = new GZipStream(memoryStream, this._compressionMode))
        //                        {
        //                            zipStream.Write(chunk, 0, chunk.Length);
        //                        }
        //
        //                        byte[] data = memoryStream.ToArray();
        //                        size = BitConverter.GetBytes(data.Length);
        //                        byte[] result = new byte[size.Length + data.Length];
        //                        Buffer.BlockCopy(size, 0, result, 0, size.Length);
        //                        Buffer.BlockCopy(data, 0, result, size.Length, data.Length);
        //                        this._result.Add(new Chunk(index, result));
        //                    }
        //                }
        //                else if (this._compressionMode == CompressionMode.Decompress)
        //                {
        //                    using (var inStream = new MemoryStream(chunk))
        //                    {
        //                        using (var outStream = new MemoryStream())
        //                        {
        //                            using (var zipStream = new GZipStream(inStream, this._compressionMode))
        //                            {
        //                                byte[] buffer = new byte[chunk.Length];
        //                                int numRead;
        //                                while ((numRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
        //                                {
        //                                    outStream.Write(buffer, 0, numRead);
        //                                }
        //                            }
        //
        //                            this._result.Add(new Chunk(index, outStream.ToArray()));
        //                        }
        //                    }
        //                }
        //
        ////                }
        //            }
        //        }
    }
}