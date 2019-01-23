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

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = "no";

        private const string DevicePushIdKey = "device_push_id_key";
        private static readonly string DevicePushIdDefault = "none";

        private const string IsLoggedInKey = "is_logged_in_key";
        private static readonly string IsLoggedInDefault= "no";

        private const string CurrentUserKey = "current_user_key";
        private static readonly string CurrentUserDefault = "none";

        #endregion


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

        public static string IsLoggedIn
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

    }
}
