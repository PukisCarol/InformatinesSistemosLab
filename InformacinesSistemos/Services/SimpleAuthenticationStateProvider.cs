
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace InformacinesSistemos.Services
{
    /// <summary>
    /// Paprastas auth provider'is, kuris saugo prisijungimo būseną naršyklėje (localStorage).
    /// Tai leidžia išlaikyti prisijungimą po navigacijų ir net po puslapio refresh.
    /// </summary>
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _storage;
        private const string StorageKey = "authState";

        public SimpleAuthenticationStateProvider(ProtectedLocalStorage storage)
        {
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var stored = await _storage.GetAsync<AuthInfo>(StorageKey);
                if (stored.Success && stored.Value is { } info && !string.IsNullOrWhiteSpace(info.Email))
                {
                    var identity = CreateIdentity(info.Email, info.Role ?? "User");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            catch
            {
                // jei nepavyksta nuskaityti, grąžiname neprisijungusį
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task SignIn(string email, string role)
        {
            var info = new AuthInfo { Email = email, Role = role };
            await _storage.SetAsync(StorageKey, info);

            var principal = new ClaimsPrincipal(CreateIdentity(email, role));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public async Task SignOut()
        {
            await _storage.DeleteAsync(StorageKey);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        private static ClaimsIdentity CreateIdentity(string email, string role) =>
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role)
                },
                authenticationType: "SimpleAuth"
            );

        private class AuthInfo
        {
            public string Email { get; set; } = string.Empty;
            public string? Role { get; set; }
        }
    }
}
