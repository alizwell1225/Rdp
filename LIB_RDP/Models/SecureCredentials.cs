using System;
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
        
        public SecureCredentials(string userName, string password, string domain = "")
        {
            UserName = userName;
            Domain = domain;
            _entropy = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_entropy);
            }
            SetPassword(password);
        }
        
        private void SetPassword(string password)
        {
            _securePassword = new SecureString();
            foreach (char c in password)
            {
                _securePassword.AppendChar(c);
            }
            _securePassword.MakeReadOnly();
        }
        
        /// <summary>
        /// 獲取解密後的密碼
        /// </summary>
        /// <returns>明文密碼</returns>
        public string GetPassword()
        {
            if (_securePassword == null || _securePassword.Length == 0)
                return string.Empty;
                
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(_securePassword);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }
        
        /// <summary>
        /// 將憑證加密保存為Base64字符串
        /// </summary>
        /// <returns>加密後的憑證字符串</returns>
        public string ToEncryptedString()
        {
            try
            {
                string credentials = $"{UserName}|{Domain}|{GetPassword()}";
                byte[] data = Encoding.UTF8.GetBytes(credentials);
                byte[] encryptedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedData) + "|" + Convert.ToBase64String(_entropy);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// 從加密字符串還原憑證
        /// </summary>
        /// <param name="encryptedString">加密的憑證字符串</param>
        /// <returns>SecureCredentials實例</returns>
        public static SecureCredentials FromEncryptedString(string encryptedString)
        {
            try
            {
                string[] parts = encryptedString.Split('|');
                if (parts.Length != 2) return null;
                
                byte[] encryptedData = Convert.FromBase64String(parts[0]);
                byte[] entropy = Convert.FromBase64String(parts[1]);
                
                byte[] data = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
                string credentials = Encoding.UTF8.GetString(data);
                
                string[] credParts = credentials.Split('|');
                if (credParts.Length >= 3)
                {
                    return new SecureCredentials(credParts[0], credParts[2], credParts[1]);
                }
            }
            catch (Exception)
            {
                // 解密失敗，返回null
            }
            return null;
        }
        
        public void Dispose()
        {
            _securePassword?.Dispose();
            if (_entropy != null)
            {
                Array.Clear(_entropy, 0, _entropy.Length);
            }
        }
    }
}