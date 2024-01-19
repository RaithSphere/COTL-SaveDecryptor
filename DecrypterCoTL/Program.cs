using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

class Decrypter_CoTL
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: DecryptFile.exe <input_file>");
            return;
        }

        string filePath = args[0];

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        byte[] encryptedBytes = File.ReadAllBytes(filePath);
        byte[] keyBytes = encryptedBytes.Skip(1).Take(16).ToArray();
        byte[] ivBytes = encryptedBytes.Skip(17).Take(16).ToArray();
        byte[] dataBytes = encryptedBytes.Skip(33).ToArray();

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = ivBytes;

            using (MemoryStream msDecrypt = new MemoryStream(dataBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        string decryptedText = srDecrypt.ReadToEnd();
                        string beautifiedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(decryptedText), Formatting.Indented);
                        Console.WriteLine(beautifiedJson);
                        string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                        string newFileName = $"{originalFileNameWithoutExtension}_decrypted.json";
                        File.WriteAllText(newFileName, beautifiedJson);
                    }
                }
            }
        }
    }
}
