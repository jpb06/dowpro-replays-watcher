using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Helpers
{
    public class CryptoHelper
    {
        public static void EncryptFile(
            string filePath, 
            string destPath)
        {
            try
            {
                byte[] iv;

                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = System.Convert.FromBase64String(LogicSettings.Default.SymmetricKey);
                    aes.GenerateIV();
                    iv = aes.IV;

                    using (FileStream outputFileStream = new FileStream(destPath, FileMode.Create))
                    using (CryptoStream cs = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[1];
                        int read;

                        try
                        {
                            while ((read = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                                cs.Write(buffer, 0, read);

                            cs.Close();
                            outputFileStream.Close();
                            inputFileStream.Close();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                Prepend(destPath, iv);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void Prepend(string filePath, byte[] data)
        {
            char[] buffer = new char[1024];

            string renamedFile = $"{filePath}.orig";
            File.Move(filePath, renamedFile);

            File.WriteAllBytes(filePath, data);

            using (StreamReader sr = new StreamReader(renamedFile, Encoding.GetEncoding(1252)))
            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.GetEncoding(1252)))
            {
                int read;
                while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
                    sw.Write(buffer, 0, read);
            }

            File.Delete(renamedFile);
        }

        public static string GetChecksum(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
