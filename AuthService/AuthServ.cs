using ActivityService;
using CookiesService;
using DataService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthService
{
    public class AuthServ : IAuthServ
    {
        private readonly UserManager<AppUser> userManager;
        private readonly AppSettings appSettings;
        private readonly AppDbContext appDb;
        private readonly IServiceProvider provider;
        private readonly DataProtectionKeys dataProtKey;
        private readonly ICookiesServ cookieServ;
        private readonly IActivityServ actServ;
        private IPersonalDataProtector protector;
        private string[] UserRole = new[] { "Administractor", "Customer" };
        private TokenValidationParameters validationParameters;
        private JwtSecurityTokenHandler handler;
        private string unProtToken;
        private ClaimsPrincipal validateToken;


        public AuthServ(UserManager<AppUser> userMan, IOptions<AppSettings> appSet, IOptions<DataProtectionKeys> dpk, AppDbContext dbCon, ICookiesServ cookies, IServiceProvider prov, IActivityServ act)
        {
            userManager = userMan;
            appSettings = appSet.Value;
            dataProtKey = dpk.Value;
            cookieServ = cookies;
            provider = prov;
            actServ = act;
        }
        private static TokenResponseModel CreateErrorResponseToken(string eMessage, HttpStatusCode statusCode)
        {
            var eToken = new TokenResponseModel
            {
                Token = null,
                UserName = null,
                Role = null,
                RefreshExpiration = DateTime.Now,
                RefreshToken = null,
                Expiration = DateTime.Now,
                ResponseInfo = CreateResponse(eMessage, statusCode)
            };
            return eToken;
        }

        private static ResponseStatusInfoModel CreateResponse(string eMessage, HttpStatusCode statusCode)
        {
            var res = new ResponseStatusInfoModel
            {
                Message = eMessage,
                StatusCode = statusCode
            };
            return res;
        }

        public async Task<TokenResponseModel> Auth(LoginModel model)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if(user == null) 
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                var role = await userManager.GetRolesAsync(user);
                if(role.FirstOrDefault() != "Administrator")
                {
                    Log.Error("Error: Not an Admin");
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                }
                if(!await userManager.CheckPasswordAsync(user, model.Password))
                {
                    Log.Error("Invalid Password");
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                }
                if(!await userManager.IsEmailConfirmedAsync(user))
                {
                    Log.Error("Email is not cofirmed");
                    return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
                }
                var authToken = await GenerateNewtoken(user, model);
                return authToken;
            }
            catch (Exception e)
            {
                Log.Error("Unable to decrypt cookies {Error} {StackTrace} {InnerException} {Source}",
                e.Message, e.StackTrace, e.InnerException, e.Source);
            }
            return CreateErrorResponseToken("Request Not Supported", HttpStatusCode.Unauthorized);
        }

        private async Task<TokenResponseModel> GenerateNewtoken(AppUser user, LoginModel model)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.Secret));
            var roles = await userManager.GetRolesAsync(user);
            var tokeHandler = new JwtSecurityTokenHandler();
            var tokenDesc = new SecurityTokenDescriptor { 
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim("LoggedOn", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = appSettings.Site,
                Audience = appSettings.Audience,
                Expires = (string.Equals(roles.FirstOrDefault(), "Administrator", StringComparison.CurrentCultureIgnoreCase)) ? DateTime.UtcNow.AddMinutes(60) : DateTime.UtcNow.AddMinutes(Convert.ToDouble(appSettings.ExpireTime))
            };
            var encryptionKey = Guid.NewGuid().ToString();
            var encryptionKeyJwt = Guid.NewGuid().ToString();
            var protProvider = provider.GetService<IDataProtectionProvider>();
            var protectorJwt = protProvider.CreateProtector(encryptionKeyJwt);
            var token = tokeHandler.CreateToken(tokenDesc);
            var encryptedToken = protectorJwt.Protect(tokeHandler.WriteToken(token));
            TokenModel tm = new TokenModel();
            tm = CreateRefreshToken(appSettings.ClientId, user.Id, Convert.ToInt32(appSettings.ExpireTime));
            tm.EncryptionKeyJwt = encryptionKeyJwt;
            tm.EncryptionKeyRt = encryptionKey;
            try
            {
                var rt = appDb.Tokens.FirstOrDefault(t => t.UserId == user.Id);
                if (rt != null)
                {
                    appDb.Tokens.Remove(rt);
                    appDb.Tokens.Add(tm);
                }
                else await appDb.Tokens.AddAsync(tm);
                await appDb.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("Unable to decrypt cookies {Error} {StackTrace} {InnerException} {Source}",
                e.Message, e.StackTrace, e.InnerException, e.Source);
            }
            var protRt = protProvider.CreateProtector(encryptionKey);
            var layerOneProt = protProvider.CreateProtector(dataProtKey.AppUserKey);
            var encAuthToken = new TokenResponseModel
            {
                Token = layerOneProt.Protect(encryptedToken),
                UserId = layerOneProt.Protect(tm.Value),
                UserName = user.UserName,
                Role = roles.FirstOrDefault(),
                RefreshToken = protRt.Protect(tm.Value),
                Expiration = token.ValidTo,
                ResponseInfo = CreateResponse("Authentication Token Created", HttpStatusCode.OK)
            };
            return encAuthToken;
        }
        private static TokenModel CreateRefreshToken(string cliId, string uId, int exTime)
        {
            return new TokenModel()
            {
                ClientId = cliId,
                UserId = uId,
                ExpireTime = DateTime.UtcNow.AddMinutes(exTime),
                Value=Guid.NewGuid().ToString("N"),
                CreatedDate=DateTime.Now,
                EncryptionKeyJwt="",
                EncryptionKeyRt=""
            };
        }
    }
}
