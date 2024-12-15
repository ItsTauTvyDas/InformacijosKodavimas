using System.Security.Cryptography;
using System.Text;

namespace InformacijosKodavimas
{
    public class Decryption : AbstractCrypt
    {
        public Decryption(string key) : base(key) {}

        public static string Decrypt(string key, string encrypted, EncryptionType type)
        {
            return new Decryption(key).Decrypt(encrypted, type);
        }

        public string Decrypt(string encrypted, EncryptionType type)
        {
            return type switch
            {
                EncryptionType.E_AES => DecryptAES(encrypted),
                EncryptionType.E_3DES => Decrypt3DES(encrypted),
                _ => throw new ArgumentException("Nežinomas atkodavimo tipas"),
            };
        }

        public string DecryptAES(string encrypted)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.ASCII.GetBytes(Key);
            aes.IV = Encoding.ASCII.GetBytes(Key[..16]);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encrypted));
            using var stream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public string Decrypt3DES(string encrypted)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(Key.PadRight(24, '0'));
            tripleDes.IV = Encoding.UTF8.GetBytes(Key[..8]);
            using var decryptor = tripleDes.CreateDecryptor(tripleDes.Key, tripleDes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encrypted));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }
}
