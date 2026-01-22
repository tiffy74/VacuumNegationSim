using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Viable.Core.Unity.Export
{
    /// <summary>
    /// Writes checksums.txt with SHA-256 hashes for all exported files.
    /// Stage 9: Reproducibility verification via checksums.
    /// </summary>
    public class ChecksumWriter
    {
        /// <summary>
        /// Write checksums file.
        /// Computes SHA-256 hash for every file in the export directory.
        /// </summary>
        public void Write(string checksumsPath, string exportDir)
        {
            var sb = new StringBuilder();
            var dirInfo = new DirectoryInfo(exportDir);

            foreach (var fileInfo in dirInfo.GetFiles())
            {
                // Skip checksums.txt itself (we're creating it)
                if (fileInfo.Name == "checksums.txt")
                    continue;

                string hash = ComputeSha256(fileInfo.FullName);
                sb.AppendLine($"{hash}  {fileInfo.Name}");
            }

            File.WriteAllText(checksumsPath, sb.ToString());
        }

        /// <summary>
        /// Compute SHA-256 hash of a file.
        /// </summary>
        private string ComputeSha256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BytesToHex(hash);
                }
            }
        }

        /// <summary>
        /// Convert byte array to lowercase hex string.
        /// </summary>
        private string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
