using Castle.Core.Configuration;
using FluentAssertions;
using Harrison314.EntityFrameworkCore.Encryption;
using Harrison314.EntityFrameworkCore.Encryption.Contrib;
using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Contrib.Tests
{
    [TestClass]
    public class EndpointsExtensionsTests
    {
        [TestMethod]
        public async Task FilterAcceptKeyIds_Sucess()
        {
            Mock<ITestProvider> testProvider = new Mock<ITestProvider>(MockBehavior.Strict);
            testProvider.Setup(t => t.FilterAcceptKeyIds(It.Is<List<string>>(q => q != null && q.Contains("key1") && q.Contains("key3")), It.IsAny<CancellationToken>()))
                .ReturnsAsync("key3")
                .Verifiable();

            using IHost host = await new HostBuilder()
              .ConfigureWebHost(webBuilder =>
              {
                  webBuilder
                      .UseTestServer()
                      .ConfigureServices(services =>
                      {
                          services.AddRouting();
                          services.AddSingleton<ITestProvider>(testProvider.Object);
                      })
                      .Configure(app =>
                      {
                          app.UseRouting();

                          app.UseEndpoints(endpoints =>
                          {
                              endpoints.MapRemoteEncryptedCryptoProvider<ITestProvider>("/provider");
                          });
                      });
              })
              .StartAsync();

            using HttpClient client = host.GetTestClient();

            List<string> keyIds = new List<string>() { "key1", "key2", "key3" };

            Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>> loggerMock = new Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>>(MockBehavior.Strict);

            RemoteDbContextEncryptedCryptoProviderOptions remoteDbContextEncryptedCryptoProviderOptions = new RemoteDbContextEncryptedCryptoProviderOptions()
            {
                EndpointUrl = client.BaseAddress.OriginalString + "provider"
            };

            Mock<IHttpClientFactory> factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(t => t.CreateClient("")).Returns(client);

            RemoteDbContextEncryptedCryptoProvider provider = new RemoteDbContextEncryptedCryptoProvider(factoryMock.Object,
                Options.Create(remoteDbContextEncryptedCryptoProviderOptions),
                loggerMock.Object);

            string result = await provider.FilterAcceptKeyIds(keyIds, default);
            result.Should().NotBeNullOrEmpty().And.BeEquivalentTo("key3");
        }

        [TestMethod]
        public async Task FilterAcceptKeyIds_Failed()
        {
            Mock<ITestProvider> testProvider = new Mock<ITestProvider>(MockBehavior.Strict);
            testProvider.Setup(t => t.FilterAcceptKeyIds(It.Is<List<string>>(q => q != null && q.Contains("key1") && q.Contains("key3")), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as string)
                .Verifiable();

            using IHost host = await new HostBuilder()
              .ConfigureWebHost(webBuilder =>
              {
                  webBuilder
                      .UseTestServer()
                      .ConfigureServices(services =>
                      {
                          services.AddRouting();
                          services.AddSingleton<ITestProvider>(testProvider.Object);
                      })
                      .Configure(app =>
                      {
                          app.UseRouting();

                          app.UseEndpoints(endpoints =>
                          {
                              endpoints.MapRemoteEncryptedCryptoProvider<ITestProvider>("/provider");
                          });
                      });
              })
              .StartAsync();

            using HttpClient client = host.GetTestClient();

            List<string> keyIds = new List<string>() { "key1", "key2", "key3" };

            Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>> loggerMock = new Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>>(MockBehavior.Strict);

            RemoteDbContextEncryptedCryptoProviderOptions remoteDbContextEncryptedCryptoProviderOptions = new RemoteDbContextEncryptedCryptoProviderOptions()
            {
                EndpointUrl = client.BaseAddress.OriginalString + "provider"
            };

            Mock<IHttpClientFactory> factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(t => t.CreateClient("")).Returns(client);

            RemoteDbContextEncryptedCryptoProvider provider = new RemoteDbContextEncryptedCryptoProvider(factoryMock.Object,
                Options.Create(remoteDbContextEncryptedCryptoProviderOptions),
                loggerMock.Object);

            string result = await provider.FilterAcceptKeyIds(keyIds, default);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task EncryptMasterKey()
        {
            Mock<ITestProvider> testProvider = new Mock<ITestProvider>(MockBehavior.Strict);
            testProvider.Setup(t => t.EncryptMasterKey(It.Is<byte[]>(q => q != null), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MasterKeyData()
                {
                    Data = new byte[] {1,2,3,4},
                    KeyId = "testKey45",
                    Parameters = "testParams89"
                })
                .Verifiable();

            using IHost host = await new HostBuilder()
              .ConfigureWebHost(webBuilder =>
              {
                  webBuilder
                      .UseTestServer()
                      .ConfigureServices(services =>
                      {
                          services.AddRouting();
                          services.AddSingleton<ITestProvider>(testProvider.Object);
                      })
                      .Configure(app =>
                      {
                          app.UseRouting();

                          app.UseEndpoints(endpoints =>
                          {
                              endpoints.MapRemoteEncryptedCryptoProvider<ITestProvider>("/provider");
                          });
                      });
              })
              .StartAsync();

            using HttpClient client = host.GetTestClient();

            Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>> loggerMock = new Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>>(MockBehavior.Strict);

            RemoteDbContextEncryptedCryptoProviderOptions remoteDbContextEncryptedCryptoProviderOptions = new RemoteDbContextEncryptedCryptoProviderOptions()
            {
                EndpointUrl = client.BaseAddress.OriginalString + "provider"
            };

            Mock<IHttpClientFactory> factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(t => t.CreateClient("")).Returns(client);

            RemoteDbContextEncryptedCryptoProvider provider = new RemoteDbContextEncryptedCryptoProvider(factoryMock.Object,
                Options.Create(remoteDbContextEncryptedCryptoProviderOptions),
                loggerMock.Object);

            MasterKeyData result = await provider.EncryptMasterKey(new byte[] { 47, 8, 5, 4, 6, 3, 2, 1, 5, 0, 4}, default);
            result.Should().NotBeNull();
            result.KeyId.Should().Be("testKey45");
            result.Parameters.Should().Be("testParams89");
        }

        [TestMethod]
        public async Task DecryptMasterKey()
        {
            Mock<ITestProvider> testProvider = new Mock<ITestProvider>(MockBehavior.Strict);
            testProvider.Setup(t => t.DecryptMasterKey(It.Is<MasterKeyData>(q => q != null), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3, 4, 5, 6, 7 })
                .Verifiable();

            using IHost host = await new HostBuilder()
              .ConfigureWebHost(webBuilder =>
              {
                  webBuilder
                      .UseTestServer()
                      .ConfigureServices(services =>
                      {
                          services.AddRouting();
                          services.AddSingleton<ITestProvider>(testProvider.Object);
                      })
                      .Configure(app =>
                      {
                          app.UseRouting();

                          app.UseEndpoints(endpoints =>
                          {
                              endpoints.MapRemoteEncryptedCryptoProvider<ITestProvider>("/provider");
                          });
                      });
              })
              .StartAsync();

            using HttpClient client = host.GetTestClient();

            Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>> loggerMock = new Mock<ILogger<RemoteDbContextEncryptedCryptoProvider>>(MockBehavior.Strict);

            RemoteDbContextEncryptedCryptoProviderOptions remoteDbContextEncryptedCryptoProviderOptions = new RemoteDbContextEncryptedCryptoProviderOptions()
            {
                EndpointUrl = client.BaseAddress.OriginalString + "provider"
            };

            Mock<IHttpClientFactory> factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(t => t.CreateClient("")).Returns(client);

            RemoteDbContextEncryptedCryptoProvider provider = new RemoteDbContextEncryptedCryptoProvider(factoryMock.Object,
                Options.Create(remoteDbContextEncryptedCryptoProviderOptions),
                loggerMock.Object);

            byte[] result = await provider.DecryptMasterKey(new MasterKeyData() { Data = new byte[] { 1, 4, 5 }, KeyId = "keyId1", Parameters ="myParams" }, default);
            result.Should().NotBeNullOrEmpty();
        }

        public interface ITestProvider : IDbContextEncryptedCryptoProvider
        {

        }
    }
}
