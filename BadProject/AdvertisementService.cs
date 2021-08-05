using BadProject.Interfaces;
using BadProject.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using ThirdParty;

namespace Adv
{
    public class AdvertisementService
    {
        private MemoryCache cache;
        private IThirdPartyProviderRepository thirdPartyRepo;
        private List<DateTime> errors;
        private readonly int maxErrorsCount;
        private readonly int hoursForErrorCount;
        private readonly int cacheDeletionMinutes;

        public AdvertisementService(MemoryCache cache, IThirdPartyProviderRepository thirdPartyRepo, List<DateTime> errors, int maxErrorsCount = 10, int hoursForErrorCount = 1, int cacheDeletionMinutes = 0)
        {
            this.cache = cache;
            this.thirdPartyRepo = thirdPartyRepo;
            this.errors = errors;
            this.maxErrorsCount = maxErrorsCount;
            this.hoursForErrorCount = hoursForErrorCount;
            this.cacheDeletionMinutes = cacheDeletionMinutes;
        }
        // **************************************************************************************************
        // Loads Advertisement information by id
        // from cache or if not possible uses the "mainProvider" or if not possible uses the "backupProvider"
        // **************************************************************************************************
        // Detailed Logic:
        // 
        // 1. Tries to use cache (and retuns the data or goes to STEP2)
        //
        // 2. If the cache is empty it uses the NoSqlDataProvider (mainProvider), 
        //    in case of an error it retries it as many times as needed based on AppSettings
        //    (returns the data if possible or goes to STEP3)
        //
        // 3. If it can't retrive the data or the ErrorCount in the last hour is more than 10, 
        //    it uses the SqlDataProvider (backupProvider)
        public async Task<Advertisement> GetAdvertisement(string id)
        {
            if (id == null)
            {
                throw new ArgumentException("Id Cannot be null");
            }
            errors.RemoveAll(e => e < DateTime.Now.AddHours(-this.hoursForErrorCount));
            var advertisment = GetAdvertismentFromCache(id);
            if (advertisment != null)
            {
                return advertisment;
            }
            
            if (errors.Count < maxErrorsCount)
            {
                advertisment = await GetAdvertismentFromHttpProvider(id);
            }

            if (advertisment != null)
            {
                cache.Set($"AdvKey_{id}", advertisment, DateTimeOffset.Now.AddMinutes(cacheDeletionMinutes));
                return advertisment;
            }
            advertisment = await GetAdvertismentFromBackupProvider(id);
            if (advertisment != null)
            {
                cache.Set($"AdvKey_{id}", advertisment, DateTimeOffset.Now.AddMinutes(cacheDeletionMinutes));
            }
            return advertisment;

        }

        private Advertisement GetAdvertismentFromCache(string id)
        {
            if (cache.Contains($"AdvKey_{id}"))
            {
                var item = cache.GetCacheItem($"AdvKey_{id}");
                if (item.Value is Advertisement)
                {
                    return (Advertisement)item.Value;
                }
            }
            return null;
        }



        private async Task<Advertisement> GetAdvertismentFromHttpProvider(string id)
        {

            var IsRetry = int.TryParse(ConfigurationManager.AppSettings["RetryCount"], out var retries);
            if (!IsRetry)
            {
                //default retry is one
                retries = 1;
            }
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    
                    return await thirdPartyRepo.GetAdvFromNonSqlProvider(id);
                    
                }
                catch (Exception e)
                {

                    Thread.Sleep(1000);
                    errors.Add(DateTime.Now); // Store HTTP error timestamp    
                }
            }
            return null;

        }

        

        private async Task<Advertisement> GetAdvertismentFromBackupProvider(string id)
        {
            Advertisement advert = null;
            try
            {
                advert = await thirdPartyRepo.GetAdvFromSqlProvider(id);
            }
            catch (Exception e)
            {

                errors.Add(DateTime.Now); // Store HTTP error timestamp 

            }
            return advert;
        }


    }
}
