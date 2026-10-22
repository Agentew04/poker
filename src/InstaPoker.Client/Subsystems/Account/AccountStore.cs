using InstaPoker.Client.Subsystems.Settings;

namespace InstaPoker.Client.Subsystems.Account;

public class AccountStore {

    private readonly IConfigRepository config;
    
    public AccountStore(SettingsService settingsService) {
        config = settingsService.GetRepository("Account");
    }
    
    public Login? GetSavedLogin() {
        if (!config.TryGetValue("lastusername", out string? login)) {
            return null;
        }

        // key is present but is null
        if (login == null) {
            return null;
        }
        
        string? password = config.GetValue<string>("lastpassword");
        if (password == null) {
            return null;
        }
        return new Login(login, password);
    }

    public void SaveLogin(Login login) {
        config.SetValue("lastusername", login.Username);
        config.SetValue("lastpassword", login.Password);
        config.Save();
    }

    public void ClearLogin() {
        config.SetValue("lastusername", string.Empty);
        config.SetValue("lastpassword", string.Empty);
        config.Save();
    }
}

public record Login(string Username, string Password);