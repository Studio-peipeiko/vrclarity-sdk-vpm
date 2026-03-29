using System;
using System.Security.Cryptography;
using System.Text;

namespace StudioPeipeiko.VRClarity.Editor
{
    public static class VRClarityEncryption
    {
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string.");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// AES-256-GCM encrypt a plaintext string.
        /// Returns base64url-encoded: nonce(12) || ciphertext(N) || tag(16)
        /// Uses a manual AES-CTR + GHASH implementation for Mono/Unity compatibility.
        /// </summary>
        public static string EncryptPayload(string plaintext, byte[] key)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 256 bits (32 bytes).");

            byte[] nonce = new byte[12];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(nonce);

            byte[] pt = Encoding.UTF8.GetBytes(plaintext);
            byte[] ct = new byte[pt.Length];
            byte[] tag = new byte[16];

            AesGcmEncrypt(key, nonce, pt, ct, tag);

            // nonce(12) || ciphertext(N) || tag(16)
            byte[] result = new byte[12 + ct.Length + 16];
            Buffer.BlockCopy(nonce, 0, result, 0, 12);
            Buffer.BlockCopy(ct, 0, result, 12, ct.Length);
            Buffer.BlockCopy(tag, 0, result, 12 + ct.Length, 16);

            return Base64UrlEncode(result);
        }

        private static void AesGcmEncrypt(byte[] key, byte[] nonce, byte[] plaintext, byte[] ciphertext, byte[] tag)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                using (var encryptor = aes.CreateEncryptor())
                {
                    // H = AES_K(0^128)
                    byte[] h = EncryptBlock(encryptor, new byte[16]);

                    // J0 = nonce || 0x00000001 (for 12-byte nonce)
                    byte[] j0 = new byte[16];
                    Buffer.BlockCopy(nonce, 0, j0, 0, 12);
                    j0[15] = 1;

                    // Encrypt with AES-CTR starting from inc32(J0)
                    byte[] counter = (byte[])j0.Clone();
                    IncrementCounter(counter);

                    for (int i = 0; i < plaintext.Length; i += 16)
                    {
                        byte[] encryptedCounter = EncryptBlock(encryptor, counter);
                        int blockLen = Math.Min(16, plaintext.Length - i);
                        for (int j = 0; j < blockLen; j++)
                            ciphertext[i + j] = (byte)(plaintext[i + j] ^ encryptedCounter[j]);
                        IncrementCounter(counter);
                    }

                    // GHASH(H, {}, C)
                    byte[] ghash = GHash(h, ciphertext);

                    // Tag = AES_K(J0) XOR GHASH
                    byte[] encJ0 = EncryptBlock(encryptor, j0);
                    for (int i = 0; i < 16; i++)
                        tag[i] = (byte)(encJ0[i] ^ ghash[i]);
                }
            }
        }

        private static byte[] EncryptBlock(ICryptoTransform encryptor, byte[] block)
        {
            byte[] output = new byte[16];
            encryptor.TransformBlock(block, 0, 16, output, 0);
            return output;
        }

        private static void IncrementCounter(byte[] counter)
        {
            for (int i = 15; i >= 12; i--)
            {
                if (++counter[i] != 0) break;
            }
        }

        /// <summary>
        /// GHASH with no AAD. Processes ciphertext blocks then the length block.
        /// </summary>
        private static byte[] GHash(byte[] h, byte[] ciphertext)
        {
            byte[] y = new byte[16];

            // Process ciphertext blocks: Y = (Y XOR X_i) * H
            int fullBlocks = ciphertext.Length / 16;
            for (int i = 0; i < fullBlocks; i++)
            {
                for (int j = 0; j < 16; j++)
                    y[j] ^= ciphertext[i * 16 + j];
                y = GfMul(y, h);
            }
            int remaining = ciphertext.Length % 16;
            if (remaining > 0)
            {
                for (int j = 0; j < remaining; j++)
                    y[j] ^= ciphertext[fullBlocks * 16 + j];
                y = GfMul(y, h);
            }

            // Length block: len(A)=0 || len(C) in bits, both 64-bit big-endian
            long ctBits = (long)ciphertext.Length * 8;
            byte[] lenBlock = new byte[16];
            for (int i = 0; i < 8; i++)
                lenBlock[15 - i] = (byte)(ctBits >> (i * 8));
            for (int j = 0; j < 16; j++)
                y[j] ^= lenBlock[j];
            y = GfMul(y, h);

            return y;
        }

        /// <summary>
        /// GF(2^128) multiplication with the GCM reduction polynomial x^128 + x^7 + x^2 + x + 1.
        /// </summary>
        private static byte[] GfMul(byte[] x, byte[] y)
        {
            byte[] z = new byte[16];
            byte[] v = (byte[])y.Clone();

            for (int i = 0; i < 128; i++)
            {
                if ((x[i / 8] & (1 << (7 - (i % 8)))) != 0)
                {
                    for (int j = 0; j < 16; j++)
                        z[j] ^= v[j];
                }

                bool lsb = (v[15] & 1) != 0;
                for (int j = 15; j > 0; j--)
                    v[j] = (byte)((v[j] >> 1) | ((v[j - 1] & 1) << 7));
                v[0] >>= 1;

                if (lsb)
                    v[0] ^= 0xE1;
            }

            return z;
        }

        /// <summary>
        /// RFC 4648 section 5 base64url encoding (no padding).
        /// </summary>
        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Validate that a key ID matches the expected format: sk_ + 24 hex chars.
        /// </summary>
        public static bool IsValidKeyId(string keyId)
        {
            if (string.IsNullOrEmpty(keyId)) return false;
            if (!keyId.StartsWith("sk_")) return false;
            if (keyId.Length != 27) return false; // "sk_" (3) + 24 hex chars

            for (int i = 3; i < keyId.Length; i++)
            {
                char c = keyId[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Validate that an encryption key is a valid 64-character hex string (256-bit).
        /// </summary>
        public static bool IsValidEncryptionKey(string encKey)
        {
            if (string.IsNullOrEmpty(encKey)) return false;
            if (encKey.Length != 64) return false;

            for (int i = 0; i < encKey.Length; i++)
            {
                char c = encKey[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return true;
        }
    }
}
