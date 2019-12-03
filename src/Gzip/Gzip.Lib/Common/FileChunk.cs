namespace Gzip.Lib.Common
{
    /// <summary>
    /// Chunk data from file stream.
    /// </summary>
    public class FileChunk
    {
        /// <summary>
        /// Number of package.
        /// </summary>
        public int Number;
        /// <summary>
        /// Bytes data.
        /// </summary>
        public byte[] Data;
        /// <summary>
        /// Length data in data array.
        /// </summary>
        public int Length;
    }
}
