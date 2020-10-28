using ModelService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthService
{
    public interface IAuthServ
    {
        Task<TokenResponseModel> Auth(LoginModel model);

    }
}
