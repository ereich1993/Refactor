using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.Interfaces
{
    public interface IThirdPartyProviderRepository
    {
        Task<Advertisement> GetAdvFromNonSqlProvider(string id);
        Task<Advertisement> GetAdvFromSqlProvider(string id);
    }
}
