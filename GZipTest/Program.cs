using System.IO;

namespace GZipTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //TODO: input parameters validation
            var command = args[0];
            var inputFile = new FileInfo(args[1]);
            var ouputFile = new FileInfo(args[2]);
        }
    }
}