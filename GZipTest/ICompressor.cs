using System.IO;

namespace GZipTest
{
    public interface ICompressor
    {
        void Compress(FileInfo fileToCompress);
        void Compress(FileInfo fileToCompress, FileInfo compressedFile);
    }
}