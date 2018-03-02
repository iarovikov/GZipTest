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

        public void EnqueueNull()
        {
            this.Enqueue((Chunk)null);
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
                //Check for valid chunk order
                if (chunk != null && chunk.Id != this.chunkId)
                {
                    Monitor.Wait(this.@lock);
                }

                this.chunkQueue.Enqueue(chunk);
                this.chunkId++;
                Monitor.PulseAll(this.@lock);
            }
        }

        public Chunk Dequeue()
        {
            lock (this.@lock)
            {
                while (this.chunkQueue.Count == 0)
                {
                    if (this.chunkId == 0)
                    {
                        return null;
                    }

                    Monitor.Wait(this.@lock);
                }

                var chunk = this.chunkQueue.Dequeue();
                this.chunkId--;
                return chunk;
            }
        }
    }
}