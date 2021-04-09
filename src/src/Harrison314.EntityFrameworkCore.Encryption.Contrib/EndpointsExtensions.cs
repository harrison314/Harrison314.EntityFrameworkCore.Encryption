using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Harrison314.EntityFrameworkCore.Encryption.CryptoProviders.Remote;
using Microsoft.Extensions.DependencyInjection;

namespace Harrison314.EntityFrameworkCore.Encryption.Contrib
{
    public static class EndpointsExtensions
    {
        public static void MapRemoteEncryptedCryptoProvider<T>(this IEndpointRouteBuilder endpoints, string startUrl, Action<IEndpointConventionBuilder> customizeSetup = null)
            where T : IDbContextEncryptedCryptoProvider
        {
            if (customizeSetup == null)
            {
                customizeSetup = _ => { };
            }

            IEndpointConventionBuilder encryptMasterKeyBehoiar = endpoints.MapPost(string.Concat(startUrl.TrimEnd('/'), "/EncryptMasterKey"), async context =>
            {
                if (!context.Request.HasJsonContentType())
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return;
                }

                try
                {
                    EncryptMasterKeyRequest request = await context.Request.ReadFromJsonAsync<EncryptMasterKeyRequest>(context.RequestAborted);
                    //TODO: Validate
                    T provider = context.RequestServices.GetRequiredService<T>();

                    MasterKeyData data = await provider.EncryptMasterKey(request.MasterKey, context.RequestAborted);
                    EncryptMasterKeyResponse response = new EncryptMasterKeyResponse()
                    {
                        Data = data.Data,
                        KeyId = data.KeyId,
                        Parameters = data.Parameters
                    };

                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsJsonAsync<EncryptMasterKeyResponse>(response, context.RequestAborted);
                }
                catch (Exception ex)
                {
                    HandleError(ex, context);
                }
            });

            customizeSetup.Invoke(encryptMasterKeyBehoiar);

            IEndpointConventionBuilder filterAcceptKeyIdsBehoiar = endpoints.MapPost(string.Concat(startUrl.TrimEnd('/'), "/FilterAcceptKeyIds"), async context =>
            {
                if (!context.Request.HasJsonContentType())
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return;
                }

                try
                {
                    FilterAcceptKeyIdsRequest request = await context.Request.ReadFromJsonAsync<FilterAcceptKeyIdsRequest>(context.RequestAborted);
                    //TODO: Validate
                    T provider = context.RequestServices.GetRequiredService<T>();

                    string selectedKeyId = await provider.FilterAcceptKeyIds(request.KeyIds, context.RequestAborted);
                    FilterAcceptKeyIdsResponse response = new FilterAcceptKeyIdsResponse()
                    {
                        SelectedKeyId = selectedKeyId
                    };

                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsJsonAsync<FilterAcceptKeyIdsResponse>(response, context.RequestAborted);
                }
                catch (Exception ex)
                {
                    HandleError(ex, context);
                }
            });

            customizeSetup.Invoke(filterAcceptKeyIdsBehoiar);

            IEndpointConventionBuilder decryptMasterKeyBehoiar = endpoints.MapPost(string.Concat(startUrl.TrimEnd('/'), "/DecryptMasterKey"), async context =>
           {
               if (!context.Request.HasJsonContentType())
               {
                   context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                   return;
               }

               try
               {
                   DecryptMasterKeyRequest request = await context.Request.ReadFromJsonAsync<DecryptMasterKeyRequest>(context.RequestAborted);
                   //TODO: Validate
                   T provider = context.RequestServices.GetRequiredService<T>();

                   MasterKeyData data = new MasterKeyData()
                   {
                       Data = request.Data,
                       KeyId = request.KeyId,
                       Parameters = request.Parameters
                   };

                   byte[] masterKey = await provider.DecryptMasterKey(data, context.RequestAborted);
                   DecryptMasterKeyResponse response = new DecryptMasterKeyResponse()
                   {
                       MasterKey = masterKey
                   };

                   context.Response.StatusCode = 200;
                   await context.Response.WriteAsJsonAsync<DecryptMasterKeyResponse>(response, context.RequestAborted);

               }
               catch (Exception ex)
               {
                   HandleError(ex, context);
               }
           });

            customizeSetup.Invoke(decryptMasterKeyBehoiar);
        }

        private static void HandleError(Exception exception, HttpContext _)
        {
            if (exception is not EfEncryptionException)
            {
                throw new EfEncryptionException("Internal error in remote provider.", exception);
            }
        }
    }
}
