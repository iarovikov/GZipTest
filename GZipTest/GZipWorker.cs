using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipTest
{
    public class GZipWorker
    {
        protected const int BUFFER_SIZE = 32 * 1024;

        protected static void WriteOutFile(IList<Chunk> result, FileStream outFile)
        {
            foreach (var chunk in result.OrderBy(x => x.Id))
            {
                outFile.Write(chunk.Data, 0, chunk.Data.Length);
            }
        }
    }
}