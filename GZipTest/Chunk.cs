namespace GZipTest
{
    public class Chunk
    {
        public int Id { get; }
        public byte[] Data { get; }

        public Chunk(int id, byte[] data)
        {
            this.Id = id;
            this.Data = data;
        }
    }
}