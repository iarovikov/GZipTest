using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipTest
{
    public abstract class GZipWorker
    {
        protected const int BUFFER_SIZE = 32 * 1024;

        protected abstract void Read(FileInfo inputFile);
        protected abstract void Write(object outputFile);
    }

}