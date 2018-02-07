using System.IO;

namespace GZipTest
{
    public interface ICompressor
    {
        void Compress(FileInfo fileToCompress);
    }
}