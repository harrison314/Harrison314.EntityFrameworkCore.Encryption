using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders.Remote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders
{
    public class RemoteDbContextEncryptedCryptoProvider : IDbContextEncryptedCryptoProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOptions<RemoteDbContextEncryptedCryptoProviderOptions> setup;
        private readonly ILogger<RemoteDbContextEncryptedCryptoProvider> logger;

        public string ProviderName
        {
            get => "RemoteProvider";
        }

        public RemoteDbContextEncryptedCryptoProvider(IHttpClientFactory httpClientFactory,
            IOptions<RemoteDbContextEncryptedCryptoProviderOptions> setup,
            ILogger<RemoteDbContextEncryptedCryptoProvider> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.setup = setup;
            this.logger = logger;
        }

        public async ValueTask<byte[]> DecryptMasterKey(MasterKeyData masterKeyData, CancellationToken cancellationToken)
        {
            if (masterKeyData == null) throw new ArgumentNullException(nameof(masterKeyData));

            DecryptMasterKeyRequest request = new DecryptMasterKeyRequest()
            {
                Data = masterKeyData.Data,
                KeyId = masterKeyData.KeyId,
                Parameters = masterKeyData.Parameters
            };

            DecryptMasterKeyResponse response = await this.ExecuteRequest<DecryptMasterKeyRequest, DecryptMasterKeyResponse>("DecryptMasterKey", request, cancellationToken);

            if (response.MasterKey == null)
            {
                throw new EfEncryptionException("Response in bad state.");
            }

            return response.MasterKey;
        }

        public async ValueTask<MasterKeyData> EncryptMasterKey(byte[] masterKey, CancellationToken cancellationToken)
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));

            EncryptMasterKeyRequest request = new EncryptMasterKeyRequest()
            {
                MasterKey = masterKey
            };

            EncryptMasterKeyResponse response = await this.ExecuteRequest<EncryptMasterKeyRequest, EncryptMasterKeyResponse>("EncryptMasterKey", request, cancellationToken);

            if (response == null || response.Data == null || response.KeyId == null)
            {
                throw new EfEncryptionException("Response in bad state.");
            }

            return new MasterKeyData()
            {
                Data = response.Data,
                KeyId = response.KeyId,
                Parameters = response.Parameters
            };
        }

        public async ValueTask<string> FilterAcceptKeyIds(List<string> keyIds, CancellationToken cancellationToken)
        {
            if (keyIds == null) throw new ArgumentNullException(nameof(keyIds));

            FilterAcceptKeyIdsRequest request = new FilterAcceptKeyIdsRequest()
            {
                KeyIds = keyIds
            };

            FilterAcceptKeyIdsResponse response = await this.ExecuteRequest<FilterAcceptKeyIdsRequest, FilterAcceptKeyIdsResponse>("FilterAcceptKeyIds", request, cancellationToken);
            return response.SelectedKeyId;
        }

        private string CreateUrl(string suffix)
        {
            ReadOnlySpan<char> prefixUrl = this.setup.Value.EndpointUrl.AsSpan().TrimEnd('/');
            ReadOnlySpan<char> suffixUrl = suffix.AsSpan().TrimStart('/');

            return string.Concat(prefixUrl, "/".AsSpan(), suffixUrl);
        }

        private async Task<TResult> ExecuteRequest<TRequest, TResult>(string suffix, TRequest requestObject, CancellationToken cancellationToken)
        {
            string fullUrl = this.CreateUrl(suffix);
            HttpClient client = this.httpClientFactory.CreateClient();
            using HttpResponseMessage? response = await client.PostAsJsonAsync<TRequest>(fullUrl, requestObject, cancellationToken);

            TResult? responseObject = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Forbidden => throw new UnauthorizedAccessException($"Remote DbContextEncryption provider return forbiden. Url: {fullUrl}"),
                System.Net.HttpStatusCode.Unauthorized => throw new UnauthorizedAccessException($"Remote DbContextEncryption provider return unathorized access. Url: {fullUrl}"),
                System.Net.HttpStatusCode.NotFound => throw new EfEncryptionException($"Remote DbContextEncryption provider not found. Url: {fullUrl}"),
                System.Net.HttpStatusCode.InternalServerError => throw new EfEncryptionException($"Remote DbContextEncryption provider return internal server error. Url: {fullUrl}"),
                System.Net.HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TResult>(null, cancellationToken),
                _ => throw new EfEncryptionException($"Remote DbContextEncryption provider return {response.StatusCode}. Url: {fullUrl}")
            };

            if (responseObject == null)
            {
                throw new EfEncryptionException("Response in bad state.");
            }

            return responseObject;
        }
    }
}
