using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LIB_RDP.Models
{
    public enum CredentialEncryptionMode
    {
        DPAPI,
        AES
    }

    internal static class CryptoKeys
    {
        // 你想混淆的 XOR Key
        private const byte XorKey = 0x5B;

        // AES KeySize（可改 128 或 256）
        public static int KeySizeBits = 256; // 128 or 256

        private static string A1 = Xor("S0x2");
        private static string A2 = Xor("bk5Z");
        private static string A3 = Xor("dUtK");
        private static string A4 = Xor("WnZz");
        private static string A5 = Xor("aWZB");
        private static string A6 = Xor("QllV");
        private static string A7 = Xor("c3RT");
        private static string A8 = Xor("ZFdB");

        private static string I1 = Xor("QUxH");
        private static string I2 = Xor("c2Zq");
        private static string I3 = Xor("T2t6");
        private static string I4 = Xor("bll3");

        // Base64 片段（這裡你可以填任意字串，系統會自動補齊）
        private static readonly string[] KeyParts = new[]
        {
            A1,A2,A3,A4,A5,A6,A7,A8
        };

        private static readonly string[] IVParts = new[]
        {
           I1,I2,I3,I4
        };

        // XOR
        private static string Xor(string s)
        {
            return new string(s.Select(c => (char)(c ^ XorKey)).ToArray());
        }

        // 將多個 Base64 片段併起來並 XOR 解回
        private static string RestoreBase64(string[] parts)
        {
            return string.Concat(parts.Select(p => Xor(p))); // XOR 還原
        }

        // 若不足長度，自動補齊
        private static byte[] EnsureLength(byte[] input, int requiredLength)
        {
            if (input.Length == requiredLength)
                return input;

            byte[] output = new byte[requiredLength];

            // 循環複製直到補滿
            for (int i = 0; i < requiredLength; i++)
                output[i] = input[i % input.Length];

            return output;
        }

        public static byte[] GetKey()
        {
            // 1. XOR → 還原 Base64 → 解碼
            byte[] raw = Convert.FromBase64String(RestoreBase64(KeyParts));

            int required = KeySizeBits / 8; // 16 或 32 bytes

            // 2. 若 bytes 不足，補滿
            return EnsureLength(raw, required);
        }

        public static byte[] GetIV()
        {
            byte[] raw = Convert.FromBase64String(RestoreBase64(IVParts));

            // AES IV **永遠固定為 16 bytes**
            return EnsureLength(raw, 16);
        }
    }
}