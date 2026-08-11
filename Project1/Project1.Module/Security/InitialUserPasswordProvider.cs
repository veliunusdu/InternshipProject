namespace Project1.Module.Security
{
    /// <summary>
    /// Başlangıç parolalarını kaynak kod veya appsettings.json yerine ortam değişkenlerinden alır.
    /// </summary>
    public static class InitialUserPasswordProvider
    {
        public static string GetRequiredPassword(string configurationKey, string accountName)
        {
            string environmentVariableName = configurationKey.Replace(':', '_').Replace("_", "__");
            string password = Environment.GetEnvironmentVariable(environmentVariableName);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    $"'{accountName}' hesabı oluşturulamadı. " +
                    $"'{environmentVariableName}' ortam değişkeni boş olamaz.");
            }

            return password;
        }

        public static bool ShouldResetInitialPasswords()
        {
            const string environmentVariableName = "InitialUsers__ResetPasswords";
            return bool.TryParse(Environment.GetEnvironmentVariable(environmentVariableName), out bool reset) && reset;
        }
    }
}
