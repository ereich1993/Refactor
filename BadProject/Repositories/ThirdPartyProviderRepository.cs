using BadProject.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.Repositories
{
    public class ThirdPartyProviderRepository : IThirdPartyProviderRepository
    {
        private readonly NoSqlAdvProvider noSqlAdvProvider;

        public ThirdPartyProviderRepository(NoSqlAdvProvider noSqlAdvProvider)
        {
            this.noSqlAdvProvider = noSqlAdvProvider;
        }

        public async Task<Advertisement> GetAdvFromNonSqlProvider(string id)
        {
            if (id == null)
            {
                throw new ArgumentException("Id cannot be null");
            }
            try
            {
                return await Task.FromResult(noSqlAdvProvider.GetAdv(id));
            }
            catch (Exception e)
            {

                throw e;
            }    
        }

        public async Task<Advertisement> GetAdvFromSqlProvider(string id)
        {
            if (id == null)
            {
                throw new ArgumentException("Id cannot be null");
            }
            try
            {
                return await Task.FromResult(SQLAdvProvider.GetAdv(id));
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
