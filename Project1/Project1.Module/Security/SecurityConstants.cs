namespace Project1.Module.Security
{
    /// <summary>
    /// Uygulamanın rol, başlangıç hesabı ve yapılandırma anahtarlarını tek yerde tutar.
    /// </summary>
    public static class SecurityConstants
    {
        public const string AdministratorRoleName = "Administrators";
        public const string StandardUserRoleName = "Standard User";

        public const string AdministratorUserName = "Admin";
        public const string StandardUserName = "User";

        public const string AdminInitialPasswordConfigurationKey = "InitialUsers:AdminPassword";
        public const string UserInitialPasswordConfigurationKey = "InitialUsers:UserPassword";

        public const string ResetInitialPasswordsConfigurationKey = "InitialUsers:ResetPasswords";
    }
}
