using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class ChunkProducerConsumer : IDisposable
    {
        private readonly object @lock = new object();
        private Thread[] _workers;
        private Queue<byte[]> _chunkQueue = new Queue<byte[]>();

        public ChunkProducerConsumer(int workerCount)
        {
            this._workers = new Thread[workerCount];

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


                //TODO: implement compression

            }
        }
    }
}