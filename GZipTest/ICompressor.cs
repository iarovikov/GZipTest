using System.IO;

namespace GZipTest
{
    public interface ICompressor
    {
        void Compress(FileInfo fileToCompress);
        void Compress(FileInfo fileToCompress, FileInfo compressedFile);
        void ParallelCompress(FileInfo fileToCompress, FileInfo compressedFile, int numberOfWorkers);
    }
}