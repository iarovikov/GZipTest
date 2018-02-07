using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    public class GZipWorker : ICompressor, IDecompressor
    {
        public void Compress(FileInfo fileToCompress)
        {
            using (FileStream inputStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden
                    || fileToCompress.Extension == ".gz")
                {
                    throw new InvalidOperationException("File is hidden or already compressed");
                }
                using (FileStream outFile = File.Create(fileToCompress.FullName + ".gz"))
                {
                    using (GZipStream gZipStream = new GZipStream(outFile,
                        CompressionMode.Compress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            gZipStream.Write(buffer, 0, numRead);
                        }
                        Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            fileToCompress.Name, fileToCompress.Length, outFile.Length);
                    }
                }
            }
        }

        public void Decompress(FileInfo fileToDecompress)
        {
            throw new System.NotImplementedException();
        }
    }
}