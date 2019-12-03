using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Gzip.Lib.Tests
{
    public class GzipTests
    {
        [Fact]
        public void CompressTest()
        {
            var rnd = new Random();

            var inputFile = Path.GetTempFileName();
            var compressedFile = Path.GetTempFileName();
            var decompressedFile = Path.GetTempFileName();

            try
            {

                using (var fileStream = new FileStream(inputFile, FileMode.Create))
                {
                    var buffer = new byte[3 * 1024 * 1024];
                    rnd.NextBytes(buffer);
                    fileStream.Write(buffer);
                }

                Gzip.Compress(inputFile, compressedFile, 1);
                Gzip.Decompress(compressedFile, decompressedFile);

                Assert.True(FileCompare(inputFile, decompressedFile));

            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (File.Exists(inputFile))
                {
                    File.Delete(inputFile);
                }

                if (File.Exists(compressedFile))
                {
                    File.Delete(compressedFile);
                }

                if (File.Exists(decompressedFile))
                {
                    File.Delete(decompressedFile);
                }
            }

        }


        [Fact]
        public void DecompressIncorrectFileTest()
        {
            var rnd = new Random();

            var inputFile = Path.GetTempFileName();
            var decompressedFile = Path.GetTempFileName();

            try
            {

                using (var fileStream = new FileStream(inputFile, FileMode.Create))
                {
                    var buffer = new byte[3 * 1024 * 1024];
                    rnd.NextBytes(buffer);
                    fileStream.Write(buffer);
                }

                var exceptions = Assert.Throws<AggregateException>(() => Gzip.Decompress(inputFile, decompressedFile));

                Assert.NotNull(exceptions.InnerExceptions);
                Assert.NotEmpty(exceptions.InnerExceptions);

                var exception = exceptions.InnerExceptions.FirstOrDefault(ex => ex is InvalidDataException);
                Assert.True(exception != null, "Gzip need throw InvalidDataException exception");
            }
            finally
            {
                if (File.Exists(inputFile))
                {
                    File.Delete(inputFile);
                }

                if (File.Exists(decompressedFile))
                {
                    File.Delete(decompressedFile);
                }
            }

        }

        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;

            if (file1 == file2)
            {
                return true;
            }

            using var fs1 = new FileStream(file1, FileMode.Open);
            using var fs2 = new FileStream(file2, FileMode.Open);
            if (fs1.Length != fs2.Length)
            { 
                return false;
            }

            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            } while ((file1byte == file2byte) && (file1byte != -1));
            

            return ((file1byte - file2byte) == 0);
        }
    }
}
