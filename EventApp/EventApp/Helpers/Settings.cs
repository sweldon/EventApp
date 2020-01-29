// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace EventApp
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string AppInfoKey = "app_info_key";
        private static readonly string AppInfoKeyDefault = "DVNT Applications - Beta 0.1.10";

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = "no";

        private const string DevicePushIdKey = "device_push_id_key";
        private static readonly string DevicePushIdDefault = "none";

        private const string IsLoggedInKey = "is_logged_in_key";
        private static readonly bool IsLoggedInDefault;

        private const string CurrentUserKey = "current_user_key";
        private static readonly string CurrentUserDefault = "none";

        private const string IsActiveKey = "is_active_key";
        private static readonly bool IsActiveDefault;

        private const string IsPremiumKey = "is_premium_key";
        private static readonly bool IsPremiumDefault;

        private const string ConfettiCountKey = "confetti_key";
        private static readonly string ConfettiCountDefault = "none";

        #endregion

        public static bool IsPremium
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsPremiumKey, IsPremiumDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsPremiumKey, value);
            }
        }


        public static bool IsActive
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsActiveKey, IsActiveDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsActiveKey, value);
            }
        }

        public static string AppInfo
        {
            get
            {
                return AppSettings.GetValueOrDefault(AppInfoKey, AppInfoKeyDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppInfoKey, value);
            }
        }

        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

        public static string DevicePushId
        {
            get
            {
                return AppSettings.GetValueOrDefault(DevicePushIdKey, DevicePushIdDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(DevicePushIdKey, value);
            }
        }

        public static bool IsLoggedIn
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsLoggedInKey, IsLoggedInDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsLoggedInKey, value);
            }
        }

        public static string CurrentUser
        {
            get
            {
                return AppSettings.GetValueOrDefault(CurrentUserKey, CurrentUserDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CurrentUserKey, value);
            }
        }

        public static string ConfettiCount
        {
            get
            {
                return AppSettings.GetValueOrDefault(ConfettiCountKey, ConfettiCountDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ConfettiCountKey, value);
            }
        }

    }
}
