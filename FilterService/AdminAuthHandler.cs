using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DataService;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace FilterService
{
    public class AdminAuthHandler : AuthenticationHandler<AdminAuthOp>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IServiceProvider provider;
        private readonly IdentityDefaultOp identityDefaultOptions;
        private readonly DataProtectionKeys dataProtectionKeys;
        private readonly AppSettings appSettings;
        private const string AccessToken = "access_token";
        private const string UserId = "user_id";
        private const string UserName = "username";
        private string[] UserRoles = new[] { "Administrator" };
        public AdminAuthHandler(IOptionsMonitor<AdminAuthOp> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, 
            UserManager<AppUser> usrMan, IOptions<AppSettings> appSet, IOptions<DataProtectionKeys> dPK, IServiceProvider prov, IOptions<IdentityDefaultOp> iDO
            ):base(options, logger,encoder,clock)
        {
            userManager = usrMan;
            provider = prov;
            identityDefaultOptions = iDO.Value;
            appSettings = appSet.Value;
            dataProtectionKeys = dPK.Value;
        }
        //Validation of tokens through decryption
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(UserId))
            {
                Log.Error("no access token or user id");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            if(!AuthenticationHeaderValue.TryParse($"{"Bearer"+ Request.Cookies[AccessToken]}", out AuthenticationHeaderValue headerValue)){
                Log.Error("failed to parse token from authentication header");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            if (!AuthenticationHeaderValue.TryParse($"{"Bearer" + Request.Cookies[UserId]}", out AuthenticationHeaderValue headerValueUid)){
                Log.Error("failed to parse user id from authentication header");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            try
            {
                var key = Encoding.ASCII.GetBytes(appSettings.Secret);
                var handler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = appSettings.Site,
                        ValidAudience = appSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                var protProvider = provider.GetService<IDataProtectionProvider>();
                var protector = protProvider.CreateProtector(dataProtectionKeys.AppUserKey);
                var decryptUid = protector.Unprotect(headerValueUid.Parameter);
                var decryptToken = protector.Unprotect(headerValue.Parameter);
                TokenModel tokenModel = new TokenModel();
                using (var scope = provider.CreateScope())
                {
                    var dbContextService = scope.ServiceProvider.GetService<AppDbContext>();
                    var userToken = dbContextService.Tokens.Include(x => x.User)
                        .FirstOrDefault(y => y.UserId == decryptUid
                        && y.User.UserName==Request.Cookies[UserName]
                        && y.User.Id==decryptUid
                        && y.User.Role == "Administrator");
                    tokenModel = userToken;
                }
                if (tokenModel == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You're not authorized!"));
                }
                IDataProtector layer2prot = protProvider.CreateProtector(tokenModel?.EncryptionKeyJwt);
                string decryptTokenL2 = layer2prot.Unprotect(decryptToken);
                var validateToken = handler.ValidateToken(decryptTokenL2, validationParameters, out var securityToken);
                if(!(securityToken is JwtSecurityToken jwtSecurityToken)||!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You're not authorized!"));
                }
                var uname = validateToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
                if (Request.Cookies[UserName] != uname)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You're not authorized!"));
                }
                var user = await userManager.FindByNameAsync(uname);
                if (user == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You're not authorized!"));
                }
                if (!UserRoles.Contains(user.Role))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You're not authorized!"));
                }
                var id = new ClaimsIdentity(validateToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(id);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch(Exception e)
            {
                Log.Error("Unable to decrypt cookies {Error} {StackTrace} {InnerException} {Source}",
                e.Message, e.StackTrace, e.InnerException, e.Source);
                return await Task.FromResult(AuthenticateResult.Fail("You're Not Authorized!"));
            }
        }
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //Delete all cookies
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("user_id");
            Response.Headers["WWW-Authenticate"] = $"Not Authorized";
            Response.Redirect(identityDefaultOptions.FailPath);
            return Task.CompletedTask;
        }
    }
}
