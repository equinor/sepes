using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Azure.Service.Interface;
using System;
using System.Threading.Tasks;
using Azure.Core;

namespace Sepes.Azure.Service
{
    public class AzureKeyVaultSecretService : IAzureKeyVaultSecretService
    {
        readonly ILogger _logger;
        readonly IConfiguration _configuration;

        public AzureKeyVaultSecretService(ILogger<AzureKeyVaultSecretService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Uri> AddKeyVaultSecret(string nameOfKeyVaultUrlSetting, string secretName, string secretValue)
        {
            var client = GetKeyVaultClient(nameOfKeyVaultUrlSetting);
            var setResult = await client.SetSecretAsync(secretName, secretValue);
            return setResult.Value.Id;
        }

        public async Task<string> GetKeyVaultSecretValue(string nameOfKeyVaultUrlSetting, string secretName)
        {
            var client = GetKeyVaultClient(nameOfKeyVaultUrlSetting);
            var secret = await client.GetSecretAsync(secretName);

            return secret.Value.Value;
        }

        public async Task<string> DeleteKeyVaultSecretValue(string nameOfKeyVaultUrlSetting, string secretName, bool purge = false)
        {
            var client = GetKeyVaultClient(nameOfKeyVaultUrlSetting);
            var secret = await client.StartDeleteSecretAsync(secretName);

            if (purge)
            {
                try
                {
                    var purgedSecret = await client.PurgeDeletedSecretAsync(secretName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to purge secret {secretName}", ex);
                }

            }

            return secret.Value.Value;
        }

        //  Key vault sometimes get bloated with vm passwords, use this routine to delete and purge those. Remember not to leave reference to this so it runs in production
        public async Task ClearAllVmPasswords(string nameOfKeyVaultUrlSetting)
        {
            try
            {
                var client = GetKeyVaultClient(nameOfKeyVaultUrlSetting);

                foreach (var cur in client.GetPropertiesOfSecrets())
                {
                    if (cur.Name.ToLower().IndexOf("newvmpassword-") == 0)
                    {
                        if (cur.CreatedOn.HasValue)
                        {
                            if (cur.CreatedOn.Value.UtcDateTime.AddHours(1) < DateTime.UtcNow)
                            {
                                await client.StartDeleteSecretAsync(cur.Name);

                            }
                        }
                    }
                }

                foreach (var cur in client.GetDeletedSecrets())
                {
                    if (cur.Name.ToLower().IndexOf("newvmpassword-") == 0)
                    {

                        await client.PurgeDeletedSecretAsync(cur.Name);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete VM passwords from keyvault");
            }
        }

        SecretClient GetKeyVaultClient(string nameOfKeyVaultUrlSetting)
        {
            var keyVaultUrl = _configuration[nameOfKeyVaultUrlSetting];

            var clientId = _configuration[ConfigConstants.AZ_CLIENT_ID];
            var clientSecret = _configuration[ConfigConstants.AZ_CLIENT_SECRET];

            TokenCredential credential;

            if (String.IsNullOrWhiteSpace(clientId) || String.IsNullOrWhiteSpace(clientSecret))
            {
                credential = new DefaultAzureCredential();
            }
            else
            {
                var tenantId = _configuration[ConfigConstants.AZ_TENANT_ID];
                credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            }
            
            return new SecretClient(new Uri(keyVaultUrl), credential);
        }
    }
}
