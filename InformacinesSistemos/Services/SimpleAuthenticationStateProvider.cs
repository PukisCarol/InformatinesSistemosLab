using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace InformacinesSistemos.Services
{
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anonymous =
            new(new ClaimsIdentity());

        private ClaimsPrincipal _currentUser;

        public SimpleAuthenticationStateProvider()
        {
            _currentUser = _anonymous;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void SignIn(string userNameOrEmail, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userNameOrEmail),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "FakeAuth");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public void SignOut()
        {
            _currentUser = _anonymous;
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }
}
