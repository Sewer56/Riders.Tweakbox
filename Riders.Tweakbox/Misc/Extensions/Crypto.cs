using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SimpleBase;

namespace Riders.Tweakbox.Misc.Extensions
{
    /// <summary>
    /// Provides support for very basic encryption.
    /// </summary>
    public static class Crypto
    {
        /*
            Encrypted Message Format:
        
            // [32 bytes of Salt] + [32 bytes of IV] + [x bytes of CipherText]
            // Encoded as Base85
        */

        /// <summary>
        /// The keysize of the encryption algorithm in bits.
        /// </summary>
        private const int KeyBytes = 16;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 128;

        public static string Encrypt(byte[] plainBytes, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltBytes  = GenerateRandomKey();
            var ivBytes    = GenerateRandomKey();

            using var password  = new Rfc2898DeriveBytes(passPhrase, saltBytes, DerivationIterations);
            var keyBytes        = password.GetBytes(KeyBytes);

            using var symmetricKey = GetSymmetricKey();
            using var encryptor    = symmetricKey.CreateEncryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            
            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            var cipherTextBytes = saltBytes.Concat(ivBytes).Concat(memoryStream.ToArray()).ToArray();
            return Base85.Ascii85.Encode(cipherTextBytes);
        }

        public static byte[] Decrypt(string cipherText, string passPhrase)
        {
            var saltIvAndCypher  = Base85.Ascii85.Decode(cipherText);

            var saltStringBytes = saltIvAndCypher.Slice(0, KeyBytes).ToArray(); 
            var ivStringBytes   = saltIvAndCypher.Slice(KeyBytes, KeyBytes).ToArray();
            var cipherTextBytes = saltIvAndCypher.Slice(KeyBytes * 2).ToArray();

            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            var keyBytes = password.GetBytes(KeyBytes);

            using var symmetricKey = GetSymmetricKey();
            using var decryptor    = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes     = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            return plainTextBytes;
        }

        private static AesManaged GetSymmetricKey()
        {
            return new AesManaged()
            {
                BlockSize = KeyBytes * 8,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }

        private static byte[] GenerateRandomKey()
        {
            // Fill the array with cryptographically secure random bytes.
            var randomBytes  = new byte[KeyBytes]; 
            using var rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}
