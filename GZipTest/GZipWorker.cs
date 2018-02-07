using System.IO;

namespace GZipTest
{
    public class GZipWorker : ICompressor, IDecompressor
    {
        public void Compress(FileInfo fileToCompress)
        {
            throw new System.NotImplementedException();
        }

        public void Decompress(FileInfo fileToDecompress)
        {
            throw new System.NotImplementedException();
        }
    }
}