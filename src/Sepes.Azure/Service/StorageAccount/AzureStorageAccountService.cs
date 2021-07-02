using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Dto.Provisioning;
using Sepes.Common.Exceptions;

namespace Sepes.Azure.Service
{
    public class AzureStorageAccountService : AzureServiceBase, IAzureStorageAccountService
    {
        readonly IMapper _mapper;

        public AzureStorageAccountService(IConfiguration config, ILogger<AzureStorageAccountService> logger, IMapper mapper)
            : base(config, logger)
        {
            _mapper = mapper;
        }

        public async Task<ResourceProvisioningResult> EnsureCreated(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Ensuring Storage Account {parameters.Name} exists in Resource Group: {parameters.ResourceGroupName}");

            var storageAccount = await GetResourceAsync(parameters.ResourceGroupName, parameters.Name, false);

            if (storageAccount == null)
            {
                _logger.LogInformation($"Storage account not found, creating");

                var nameIsAvailable = await _azure.StorageAccounts.CheckNameAvailabilityAsync(parameters.Name);

                if (!(bool)nameIsAvailable.IsAvailable)
                {
                    _logger.LogError($"StorageAccountName not available/invalid. Message: {nameIsAvailable.Message}");
                    throw new ArgumentException($"StorageAccountName not available/invalid. Message: {nameIsAvailable.Message}");
                }

                // Create storage account
                storageAccount = await _azure.StorageAccounts.Define(parameters.Name)
                    .WithRegion(parameters.Region)
                    .WithExistingResourceGroup(parameters.ResourceGroupName)                   
                    .WithAccessFromAllNetworks()                     
                    .WithGeneralPurposeAccountKindV2()
                    .WithOnlyHttpsTraffic()                    
                    .WithSku(StorageAccountSkuType.Standard_LRS)
                    .WithTags(parameters.Tags)                     
                    .CreateAsync(cancellationToken);           

                _logger.LogInformation($"Done creating storage account");
            }

            var result = CreateResult(storageAccount);

            return result;
        }

        public async Task<ResourceProvisioningResult> GetSharedVariables(ResourceProvisioningParameters parameters)
        {
            var diagnosticStorageAccount = await GetResourceAsync(parameters.ResourceGroupName, parameters.Name);

            var result = CreateResult(diagnosticStorageAccount);

            return result;
        }

        ResourceProvisioningResult CreateResult(IStorageAccount storageAccount = null)
        {
            var result = ResourceProvisioningResultUtil.CreateFromIResource(storageAccount);
            result.CurrentProvisioningState = storageAccount.ProvisioningState.ToString();
            return result;
        }       

        async Task<IStorageAccount> GetResourceAsync(string resourceGroupName, string resourceName, bool failIfNotFound = true, CancellationToken cancellationToken = default)
        {
            var resource = await _azure.StorageAccounts.GetByResourceGroupAsync(resourceGroupName, resourceName, cancellationToken);

            if (resource == null)
            {
                if (failIfNotFound)
                {
                    throw NotFoundException.CreateForAzureResource(resourceName, resourceGroupName);
                }
                else
                {
                    return null;
                }
            }

            return resource;
        }    

        public async Task<string> GetProvisioningState(string resourceGroupName, string resourceName)
        {
            var resource = await GetResourceAsync(resourceGroupName, resourceName, false);

            if (resource == null)
            {
                return null;
            }

            return resource.ProvisioningState.ToString();
        }

        public async Task<IDictionary<string, string>> GetTagsAsync(string resourceGroupName, string resourceName)
        {
            var storageAccount = await GetResourceAsync(resourceGroupName, resourceName);
            return TagUtils.TagReadOnlyDictionaryToDictionary(storageAccount.Tags);
        }

        public async Task UpdateTagAsync(string resourceGroupName, string resourceName, KeyValuePair<string, string> tag, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceAsync(resourceGroupName, resourceName);

            //Ensure resource is is managed by this instance
            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithoutTag(tag.Key).ApplyAsync(cancellationToken);
            _ = await resource.Update().WithTag(tag.Key, tag.Value).ApplyAsync(cancellationToken);
        }

        public async Task SetTagsAsync(string resourceGroupName, string resourceName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceAsync(resourceGroupName, resourceName);

            EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

            _ = await resource.Update().WithTags(tags).ApplyAsync(cancellationToken);
        }

        public async Task<ResourceProvisioningResult> EnsureDeleted(ResourceProvisioningParameters parameters)
        {
            try
            {
                await Delete(parameters.ResourceGroupName, parameters.Name);
                var provisioningState = await GetProvisioningState(parameters.ResourceGroupName, parameters.Name);
                return ResourceProvisioningResultUtil.CreateFromProvisioningState(provisioningState);
            }
            catch (Exception)
            {
                throw;
            }            
        }

        public async Task Delete(string resourceGroupName, string storageAccountName, CancellationToken cancellationToken = default)
        {
            var resource = await GetResourceAsync(resourceGroupName, storageAccountName, false, cancellationToken);

            if (resource != null)
            {
                //Ensure resource is is managed by this instance
                EnsureResourceIsManagedByThisIEnvironmentThrowIfNot(resourceGroupName, resource.Tags);

                await _azure.StorageAccounts.DeleteByResourceGroupAsync(resourceGroupName, storageAccountName, cancellationToken);
            }
        }      

        public async Task<AzureStorageAccountDto> Create(Region region, string resourceGroupName, string name, Dictionary<string, string> tags, List<string> onlyAllowAccessFrom = null, CancellationToken cancellationToken = default)
        {
            var storageAccount = await CreateStorageAccountInternal(region, resourceGroupName, name, tags, onlyAllowAccessFrom, cancellationToken);

            return _mapper.Map<AzureStorageAccountDto>(storageAccount);
        }

        async Task<IStorageAccount> CreateStorageAccountInternal(Region region, string resourceGroupName, string name, Dictionary<string, string> tags, List<string> onlyAllowAccessFrom = null, CancellationToken cancellation = default)
        {
            var creator = _azure.StorageAccounts.Define(name)
            .WithRegion(region)
            .WithExistingResourceGroup(resourceGroupName);

            if (onlyAllowAccessFrom == null)
            {
                creator = creator.WithAccessFromAllNetworks();
            }
            else
            {
                //creator = creator.WithAccessFromAzureServices();

                foreach (var curAddr in onlyAllowAccessFrom)
                {
                    creator = creator.WithAccessFromIpAddress(curAddr);
                }
            }

            creator = creator
            .WithGeneralPurposeAccountKindV2()
            .WithOnlyHttpsTraffic()
            .WithSku(StorageAccountSkuType.Standard_LRS)
            .WithTags(tags);

            return await creator.CreateAsync(cancellation);
        } 
        
        public Task<ResourceProvisioningResult> Update(ResourceProvisioningParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }      
    }
}
