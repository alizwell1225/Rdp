using System;
using System.Net;
using System.Text.RegularExpressions;
using LIB_RDP.Models;

namespace LIB_RDP.Validators
{
    /// <summary>
    /// RDP連線參數驗證器
    /// </summary>
    public static class RdpValidator
    {
        // 有效的顏色深度值
        private static readonly int[] ValidColorDepths = { 8, 15, 16, 24, 32, 64 };
        
        // 主機名稱的正則表達式（DNS名稱或IP位址）
        private static readonly Regex HostNameRegex = new Regex(
            @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$",
            RegexOptions.Compiled);
        
        /// <summary>
        /// 驗證主機名稱或IP位址
        /// </summary>
        /// <param name="hostName">主機名稱</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateHostName(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                return ValidationResult.Failure("主機名稱不能為空");
            
            // 檢查是否為有效的IP位址
            if (IPAddress.TryParse(hostName, out _))
                return ValidationResult.Success();
            
            // 檢查是否為有效的主機名稱
            if (HostNameRegex.IsMatch(hostName))
                return ValidationResult.Success();
            
            return ValidationResult.Failure($"無效的主機名稱或IP位址: {hostName}");
        }
        
        /// <summary>
        /// 驗證使用者名稱
        /// </summary>
        /// <param name="userName">使用者名稱</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ValidationResult.Failure("使用者名稱不能為空");
            
            if (userName.Length > 256)
                return ValidationResult.Failure("使用者名稱長度不能超過256個字元");
            
            // 檢查是否包含非法字元
            if (userName.Contains("\0") || userName.Contains("\r") || userName.Contains("\n"))
                return ValidationResult.Failure("使用者名稱包含非法字元");
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// 驗證畫面解析度
        /// </summary>
        /// <param name="width">寬度</param>
        /// <param name="height">高度</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateResolution(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return ValidationResult.Failure("解析度必須大於0");
            
            if (width > 4096)
                return ValidationResult.Failure($"寬度不能超過4096像素（目前: {width}）");
            
            if (height > 2160)
                return ValidationResult.Failure($"高度不能超過2160像素（目前: {height}）");
            
            // 警告：非標準解析度
            if (width % 4 != 0 || height % 4 != 0)
            {
                return ValidationResult.SuccessWithWarning(
                    "建議使用4的倍數作為解析度以獲得最佳效能");
            }
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// 驗證顏色深度
        /// </summary>
        /// <param name="colorDepth">顏色深度</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateColorDepth(int colorDepth)
        {
            if (Array.IndexOf(ValidColorDepths, colorDepth) == -1)
            {
                return ValidationResult.Failure(
                    $"無效的顏色深度: {colorDepth}。有效值: {string.Join(", ", ValidColorDepths)}");
            }
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// 驗證連線超時設定
        /// </summary>
        /// <param name="timeoutSeconds">超時秒數</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds < 5)
                return ValidationResult.Failure("連線超時不能小於5秒");
            
            if (timeoutSeconds > 300)
                return ValidationResult.Failure("連線超時不能大於300秒（5分鐘）");
            
            if (timeoutSeconds < 10)
            {
                return ValidationResult.SuccessWithWarning(
                    "連線超時時間較短，可能導致連線失敗，建議至少設定10秒");
            }
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// 驗證RDP配置
        /// </summary>
        /// <param name="config">RDP配置</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateConfig(RdpConfig config)
        {
            if (config == null)
                return ValidationResult.Failure("配置物件不能為null");
            
            var resolutionResult = ValidateResolution(config.ScreenWidth, config.ScreenHeight);
            if (!resolutionResult.IsValid)
                return resolutionResult;
            
            var colorDepthResult = ValidateColorDepth(config.ColorDepth);
            if (!colorDepthResult.IsValid)
                return colorDepthResult;
            
            return ValidationResult.Success();
        }
        
        /// <summary>
        /// 驗證連線配置檔案
        /// </summary>
        /// <param name="profile">連線配置檔案</param>
        /// <returns>驗證結果</returns>
        public static ValidationResult ValidateProfile(RdpConnectionProfile profile)
        {
            if (profile == null)
                return ValidationResult.Failure("配置檔案不能為null");
            
            var hostResult = ValidateHostName(profile.HostName);
            if (!hostResult.IsValid)
                return hostResult;
            
            var userResult = ValidateUserName(profile.UserName);
            if (!userResult.IsValid)
                return userResult;
            
            if (profile.Config != null)
            {
                var configResult = ValidateConfig(profile.Config);
                if (!configResult.IsValid)
                    return configResult;
            }
            
            return ValidationResult.Success();
        }
    }
    
    /// <summary>
    /// 驗證結果
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否驗證通過
        /// </summary>
        public bool IsValid { get; private set; }
        
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; private set; }
        
        /// <summary>
        /// 警告訊息
        /// </summary>
        public string WarningMessage { get; private set; }
        
        /// <summary>
        /// 是否有警告
        /// </summary>
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        
        private ValidationResult(bool isValid, string errorMessage = null, string warningMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            WarningMessage = warningMessage;
        }
        
        /// <summary>
        /// 建立成功的驗證結果
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }
        
        /// <summary>
        /// 建立帶警告的成功結果
        /// </summary>
        public static ValidationResult SuccessWithWarning(string warningMessage)
        {
            return new ValidationResult(true, null, warningMessage);
        }
        
        /// <summary>
        /// 建立失敗的驗證結果
        /// </summary>
        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult(false, errorMessage);
        }
        
        public override string ToString()
        {
            if (IsValid)
            {
                return HasWarning ? $"成功（警告: {WarningMessage}）" : "成功";
            }
            return $"失敗: {ErrorMessage}";
        }
    }
}
