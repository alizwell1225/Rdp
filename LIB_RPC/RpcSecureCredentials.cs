using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace LIB_RPC
{
    /// <summary>
    /// 安全憑證管理類別，提供密碼加密存儲功能
    /// </summary>
    public class RpcSecureCredentials : IDisposable
    {
        private byte[] _entropy;
        private SecureString _securePassword;

        /// <summary>
        /// 新建立憑證（明文密碼）
        /// </summary>
        public RpcSecureCredentials(string password)
        {
            _entropy = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(_entropy);

            SetPassword(password);
        }

        /// <summary>
        /// 使用既有加密資料（解密時使用）
        /// </summary>
        private RpcSecureCredentials(string password, byte[] entropy)
        {
            _entropy = entropy;
            SetPassword(password);
        }

        private void SetPassword(string password)
        {
            _securePassword?.Dispose();
            _securePassword = new SecureString();
            foreach (char c in password)
                _securePassword.AppendChar(c);

            _securePassword.MakeReadOnly();
        }

        /// <summary>
        /// 取得解密後的密碼（明文）
        /// </summary>
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
        /// 將憑證加密保存為 Base64 字符串（格式： encrypted | entropy ）
        /// </summary>
        public string ToEncryptedString()
        {
            try
            {
                string plain = GetPassword();
                byte[] data = Encoding.UTF8.GetBytes(plain);
                byte[] encrypted = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);

                return Convert.ToBase64String(encrypted) + "|" + Convert.ToBase64String(_entropy);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 從加密字串還原憑證
        /// </summary>
        public static RpcSecureCredentials FromEncryptedString(string encryptedString)
        {
            try
            {
                string[] parts = encryptedString.Split('|');
                if (parts.Length != 2) return null;

                byte[] encryptedData = Convert.FromBase64String(parts[0]);
                byte[] entropy = Convert.FromBase64String(parts[1]);

                byte[] data = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
                string password = Encoding.UTF8.GetString(data);

                return new RpcSecureCredentials(password, entropy);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _securePassword?.Dispose();
            if (_entropy != null)
                Array.Clear(_entropy, 0, _entropy.Length);
        }
    }
}
