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

        private readonly Queue<byte[]> _result = new Queue<byte[]>();

        public ChunkProducerConsumer(int workerCount, Queue<byte[]> result)
        {
            this._workers = new Thread[workerCount];
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
                }

                if (chunk == null)
                    return;

                using (var compressedStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        zipStream.Write(chunk, 0, chunk.Length);
                    }

                    this._result.Enqueue(compressedStream.ToArray());
                }
            }
        }
    }
}