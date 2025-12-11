using UnityEngine;

namespace SinmiStation.Core
{
    public static class UserProfile
    {
        private const string KeyUserId = "SS_UserId";
        private const string KeyUserName = "SS_UserName";

        public static string UserId
        {
            get
            {
                if (!PlayerPrefs.HasKey(KeyUserId))
                {
                    // Generar GUID una sola vez
                    string newId = System.Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(KeyUserId, newId);
                    PlayerPrefs.Save();
                }
                return PlayerPrefs.GetString(KeyUserId);
            }
        }

        public static string UserName
        {
            get => PlayerPrefs.GetString(KeyUserName, string.Empty);
        }

        public static bool HasUserName => !string.IsNullOrEmpty(UserName);

        public static void SetUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            PlayerPrefs.SetString(KeyUserName, name.Trim());
            PlayerPrefs.Save();
        }
    }
}
