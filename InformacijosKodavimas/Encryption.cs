using System.Security.Cryptography;
using System.Text;

namespace InformacijosKodavimas
{
    public enum EncryptionType
    {
        E_AES = 0,
        E_3DES = 1
    }

    public class Encryption : AbstractCrypt
    {
        public Encryption(string key) : base(key) { }

        public static string Encrypt(string key, string text, EncryptionType type) 
        {
            return new Encryption(key).Encrypt(text, type);
        }

        public string Encrypt(string text, EncryptionType type)
        {
            return type switch
            {
                EncryptionType.E_AES => EncryptAES(text),
                EncryptionType.E_3DES => Encrypt3DES(text),
                _ => throw new ArgumentException("Nežinomas užkodavimo tipas"),
            };
        }

        public string EncryptAES(string text)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.ASCII.GetBytes(Key);
            aes.IV = Encoding.ASCII.GetBytes(Key[..16]);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var stream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(text);
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Encrypt3DES(string plainText)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.ASCII.GetBytes(Key.PadRight(24, '0'));
            tripleDes.IV = Encoding.ASCII.GetBytes(Key[..8]);
            using var encryptor = tripleDes.CreateEncryptor(tripleDes.Key, tripleDes.IV);
            using var ms = new MemoryStream();
            using (var stream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(plainText);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
