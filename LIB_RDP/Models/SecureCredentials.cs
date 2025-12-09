using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace LIB_RDP.Models
{
    /// <summary>
    /// 安全憑證管理類別，提供密碼加密存儲功能
    /// </summary>
    public class SecureCredentials : IDisposable
    {
        private readonly byte[] _entropy;
        private SecureString _securePassword;

        public string UserName { get; private set; }
        public string Domain { get; private set; }

        public CredentialEncryptionMode EncryptionMode { get; } = CredentialEncryptionMode.AES;

        private static SecureCredentials Instance
        {
            get
            {
                if (credentials == null)
                {
                    credentials = new SecureCredentials();
                }
                return credentials;
            }
        }
        private static SecureCredentials credentials;



        public SecureCredentials()
        {
        }

        public SecureCredentials(
            string userName,
            string password,
            string domain = "",
            CredentialEncryptionMode mode = CredentialEncryptionMode.AES)
        {
            UserName = userName;
            Domain = domain;
            EncryptionMode = mode;

            _entropy = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(_entropy);
            SetPassword(password);
            credentials = this;
        }

        private void SetPassword(string password)
        {
            _securePassword = new SecureString();
            foreach (char c in password)
                _securePassword.AppendChar(c);
            _securePassword.MakeReadOnly();
        }

        public string GetPassword()
        {
            if (_securePassword == null || _securePassword.Length == 0)
                return string.Empty;

            IntPtr ptr = Marshal.SecureStringToBSTR(_securePassword);

            try
            {
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }


        /// <summary>
        ///  產生加密字串：依 EncryptionMode 決定使用 DPAPI 或 AES
        /// </summary>
        /// <returns></returns>

        public string ToEncryptedString()
        {
            return EncryptionMode switch
            {
                CredentialEncryptionMode.DPAPI => ToEncryptedString_DPAPI(),
                CredentialEncryptionMode.AES => ToEncryptedString_AES(),
                _ => string.Empty
            };
        }

        private string ToEncryptedString_DPAPI()
        {
            try
            {
                string credentials = $"{UserName}|{Domain}|{GetPassword()}";
                byte[] data = Encoding.UTF8.GetBytes(credentials);
                byte[] encryptedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedData) + "|" + Convert.ToBase64String(_entropy);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ToEncryptedString_AES()
        {
            try
            {
                string credentials = $"{UserName}|{Domain}|{GetPassword()}";
                return AesHelper.Encrypt(credentials, CryptoKeys.GetKey(), CryptoKeys.GetIV());
            }
            catch
            {
                return string.Empty;
            }
        }


        /// <summary>
        ///  從加密字串還原：自動偵測模式（DPAPI / AES）
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        public static SecureCredentials FromEncryptedString(string encrypted)
        {
            // 判斷是 DPAPI 還是 AES：  
            // DPAPI 格式為 "base64|entropy"
            // AES 則無 "|entropy"
            bool isDpapi = encrypted.Contains("|") && encrypted.Split('|').Length == 2;

            return Instance.EncryptionMode == CredentialEncryptionMode.DPAPI
                ? FromEncryptedString_DPAPI(encrypted)
                : FromEncryptedString_AES(encrypted);
        }

        private static SecureCredentials FromEncryptedString_DPAPI(string encrypted)
        {
            try
            {
                var parts = encrypted.Split('|');
                if (parts.Length != 2) return null;

                byte[] encryptedData = Convert.FromBase64String(parts[0]);
                byte[] entropy = Convert.FromBase64String(parts[1]);

                byte[] data = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
                string credentials = Encoding.UTF8.GetString(data);

                string[] credParts = credentials.Split('|');
                if (credParts.Length >= 3)
                {
                    return new SecureCredentials(credParts[0], credParts[2], credParts[1], CredentialEncryptionMode.DPAPI);
                }
            }
            catch { }

            return null;
        }

        private static SecureCredentials FromEncryptedString_AES(string encrypted)
        {
            try
            {
                string decrypted = AesHelper.Decrypt(encrypted, CryptoKeys.GetKey(), CryptoKeys.GetIV());
                var parts = decrypted.Split('|');
                if (parts.Length == 3)
                    return new SecureCredentials(parts[0], parts[2], parts[1], CredentialEncryptionMode.AES);
            }
            catch { }

            return null;
        }

        public void Dispose()
        {
            _securePassword?.Dispose();
            Array.Clear(_entropy, 0, _entropy.Length);
        }
    }

}