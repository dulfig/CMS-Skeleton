using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctService
{
    public interface IFunctServ
    {
        Task CreateDefaultAdmin();
        Task CreateDefaultUser();
    }
}
