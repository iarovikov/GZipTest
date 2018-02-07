using System.IO;

namespace GZipTest
{
    public interface IDecompressor
    {
        void Decompress(FileInfo fileToDecompress);
    }
}