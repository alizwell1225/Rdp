using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIB_RDP.Models
{
    public enum CredentialEncryptionMode
    {
        DPAPI,
        AES
    }

    internal static class CryptoKeys
    {
        // 你可自行更換此 XOR 金鑰（0~255）
        private const byte XorKey = 0x5B;

        // ======== Key ========
        // 以下是 XOR 過後的字串片段
        private static string A1 = Xor("sHA", XorKey);
        private static string A2 = Xor("bcTyH", XorKey);
        private static string A3 = Xor("9KpIw", XorKey);
        private static string A4 = Xor("rA==", XorKey);

        // ======== IV ========
        private static string I1 = Xor("Qm9sZ", XorKey);
        private static string I2 = Xor("GVvbn", XorKey);
        private static string I3 = Xor("MhY==", XorKey);

        // XOR 加密/解密（同一函式）
        private static string Xor(string input, byte key)
        {
            return new string(input.Select(c => (char)(c ^ key)).ToArray());
        }

        public static byte[] GetKey()
        {
            // 在這裡做 XOR 還原
            string decoded =
                Xor(A1, XorKey) +
                Xor(A2, XorKey) +
                Xor(A3, XorKey) +
                Xor(A4, XorKey);

            return Convert.FromBase64String(decoded);
        }

        public static byte[] GetIV()
        {
            string decoded =
                Xor(I1, XorKey) +
                Xor(I2, XorKey) +
                Xor(I3, XorKey);

            return Convert.FromBase64String(decoded);
        }
    }


}
