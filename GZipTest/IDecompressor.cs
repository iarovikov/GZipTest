using System.IO;

namespace GZipTest
{
    public interface IDecompressor
    {
        void Decompress(FileInfo fileToDecompress, FileInfo decompressedFile);
        void ParallelDecompress(FileInfo fileToDecompress, FileInfo decompressedFile);
    }
}