using System.IO;
using System.IO.Compression;
using System.Web;

namespace GZipTest
{
    public class ThreadWithCompressionState
    {
        private readonly FileStream inputStream;

        private readonly GZipStream gZipStream;

        private readonly int bufferLength;

        private readonly int counter;

        public ThreadWithCompressionState(FileStream input, GZipStream gzip, int bufferLength, int counter)
        {
            this.inputStream = input;
            this.gZipStream = gzip;
            this.bufferLength = bufferLength;
            this.counter = counter;
        }

        public void Compress()
        {
            byte[] buffer = new byte[this.bufferLength];
            this.inputStream.Read(buffer, this.counter * this.bufferLength, this.bufferLength);
            this.gZipStream.Write(buffer, 0, this.bufferLength);
        }
    }
}