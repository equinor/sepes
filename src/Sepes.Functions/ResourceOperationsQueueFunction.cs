using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Util;
using Sepes.Provisioning.Service.Interface;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sepes.Functions
{
    public class ResourceOperationsQueueFunction
    {
        readonly ILogger _logger;
        readonly IResourceProvisioningService _provisioningService;

        public ResourceOperationsQueueFunction(ILogger<ResourceOperationsQueueFunction> logger, IResourceProvisioningService provisioningService)
        {
            _logger = logger;
            _provisioningService = provisioningService;
        }

        [FunctionName("ResourceOperationsQueue")]
        [StorageAccount(ConfigConstants.RESOURCE_PROVISIONING_QUEUE_CONSTRING)]        
        public async Task Run([QueueTrigger(queueName: "sandbox-resource-operations-queue")] QueueMessage queueMessage)
        {           
            _logger.LogInformation($"Processing message: {queueMessage.MessageId}, pop count: {queueMessage.DequeueCount}, exp: {queueMessage.ExpiresOn}, next visible: { queueMessage.NextVisibleOn}");

            var transformedQueueItem = JsonSerializer.Deserialize<ProvisioningQueueParentDto>(queueMessage.Body, JsonSerializerUtil.GetDefaultOptions());
            transformedQueueItem.MessageId = queueMessage.MessageId;
            transformedQueueItem.PopReceipt = queueMessage.PopReceipt;
            transformedQueueItem.DequeueCount = Convert.ToInt32(queueMessage.DequeueCount);

            await  _provisioningService.HandleWork(transformedQueueItem);          
        }
    }
}
