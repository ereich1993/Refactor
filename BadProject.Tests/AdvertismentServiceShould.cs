using Adv;
using BadProject.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.Tests
{
    [TestClass]
    public class AdvertismentServiceShould
    {
        [TestMethod]
        public async Task GetAdvertismentFromCache()
        {
            var memoryCache = new MemoryCache("cache");
            var id = "test advertisment";
            var advert = new Advertisement()
            {
                Description = "test desc",
                Name = "test name",
                WebId = "test id"
            };
            memoryCache.Set($"AdvKey_{id}", advert, DateTimeOffset.Now.AddMinutes(5));
            var thirdPartyProviderRepoMock = new Mock<IThirdPartyProviderRepository>();
            AdvertisementService advertisementService = new AdvertisementService(memoryCache, thirdPartyProviderRepoMock.Object,new System.Collections.Generic.List<DateTime>());

            var result = await advertisementService.GetAdvertisement(id);
            Assert.IsInstanceOfType(result, typeof(Advertisement));
            Assert.AreEqual(result, advert);

        }

        [TestMethod]
        public async Task GetAdvertismentFromNonSqlProvider()
        {
            var memoryCache = new MemoryCache("cache");
            var id = "test advertisment";
            var advert = new Advertisement()
            {
                Description = "test desc",
                Name = "test name",
                WebId = "test id"
            };
            
            var thirdPartyProviderRepoMock = new Mock<IThirdPartyProviderRepository>();
            thirdPartyProviderRepoMock.Setup(a => a.GetAdvFromNonSqlProvider(id)).ReturnsAsync(advert).Verifiable();
            AdvertisementService advertisementService = new AdvertisementService(memoryCache, thirdPartyProviderRepoMock.Object, new System.Collections.Generic.List<DateTime>());

            var result = await advertisementService.GetAdvertisement(id);

            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromNonSqlProvider(id), Times.Once);
            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromSqlProvider(id), Times.Never);
            Assert.IsInstanceOfType(result, typeof(Advertisement));
            Assert.AreEqual(result, advert);

        }

        [TestMethod]
        public async Task GetAdvertismentFromSqlIfToManyErrorsProvider()
        {
            var memoryCache = new MemoryCache("cache");
            var id = "test advertisment";
            var advert = new Advertisement()
            {
                Description = "test desc",
                Name = "test name",
                WebId = "test id"
            };
            var errors = new List<DateTime>();
            for (int i = 0; i < 20; i++)
            {
                errors.Add(DateTime.Now);
            }

            var thirdPartyProviderRepoMock = new Mock<IThirdPartyProviderRepository>();
            thirdPartyProviderRepoMock.Setup(a => a.GetAdvFromSqlProvider(id)).ReturnsAsync(advert).Verifiable();
            thirdPartyProviderRepoMock.Setup(a => a.GetAdvFromNonSqlProvider(id)).ReturnsAsync(advert).Verifiable();
            AdvertisementService advertisementService = new AdvertisementService(memoryCache, thirdPartyProviderRepoMock.Object, errors);

            var result = await advertisementService.GetAdvertisement(id);
            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromNonSqlProvider(id),Times.Never);
            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromSqlProvider(id), Times.Once);
            Assert.IsInstanceOfType(result, typeof(Advertisement));
            Assert.AreEqual(result, advert);

        }

        [TestMethod]
        public async Task GetAdvertismentFromNonSqlIfErrorsAreOutdatedProvider()
        {
            var memoryCache = new MemoryCache("cache");
            var id = "test advertisment";
            var advert = new Advertisement()
            {
                Description = "test desc",
                Name = "test name",
                WebId = "test id"
            };
            var errors = new List<DateTime>();
            for (int i = 0; i < 20; i++)
            {
                errors.Add(DateTime.Now.AddDays(-1));
            }

            var thirdPartyProviderRepoMock = new Mock<IThirdPartyProviderRepository>();
            thirdPartyProviderRepoMock.Setup(a => a.GetAdvFromSqlProvider(id)).ReturnsAsync(advert).Verifiable();
            thirdPartyProviderRepoMock.Setup(a => a.GetAdvFromNonSqlProvider(id)).ReturnsAsync(advert).Verifiable();
            AdvertisementService advertisementService = new AdvertisementService(memoryCache, thirdPartyProviderRepoMock.Object, errors);

            var result = await advertisementService.GetAdvertisement(id);
            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromNonSqlProvider(id), Times.Once);
            thirdPartyProviderRepoMock.Verify(e => e.GetAdvFromSqlProvider(id), Times.Never);
            Assert.IsInstanceOfType(result, typeof(Advertisement));
            Assert.AreEqual(result, advert);

        }

        [TestMethod]
        public async Task GetAdvertismentThrowArgumentExceptionIfIdIsNull() 
        { 
            var memoryCache = new MemoryCache("cache");
            
            var advert = new Advertisement()
            {
                Description = "test desc",
                Name = "test name",
                WebId = "test id"
            };
            var errors = new List<DateTime>();
            

            var thirdPartyProviderRepoMock = new Mock<IThirdPartyProviderRepository>();

            AdvertisementService advertisementService = new AdvertisementService(memoryCache, thirdPartyProviderRepoMock.Object, errors);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => advertisementService.GetAdvertisement(null));


        }
    }
}
