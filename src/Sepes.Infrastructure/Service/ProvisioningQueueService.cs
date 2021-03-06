﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Service.Interface;
using Sepes.Azure.Util;
using Sepes.Common.Constants;
using Sepes.Common.Dto;
using Sepes.Common.Dto.Sandbox;
using Sepes.Common.Util;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class ProvisioningQueueService : IProvisioningQueueService
    {
        readonly ILogger _logger;
        readonly IAzureQueueService _queueService;

        public ProvisioningQueueService(IConfiguration config, ILogger<ProvisioningQueueService> logger, IAzureQueueService queueService)
        {
            _logger = logger;
            _queueService = queueService;
            _queueService.Init(config[ConfigConstants.RESOURCE_PROVISIONING_QUEUE_CONSTRING], "sandbox-resource-operations-queue");
        }

        public async Task<ProvisioningQueueParentDto> SendMessageAsync(ProvisioningQueueParentDto message, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Queue: Adding message: {message.Description}, having {message.Children.Count} children");
            var serializedMessage = JsonSerializerUtil.Serialize(message);
            var sendtMessage = await _queueService.SendMessageAsync(serializedMessage, visibilityTimeout, cancellationToken);

            message.MessageId = sendtMessage.MessageId;
            message.PopReceipt = sendtMessage.PopReceipt;
            message.NextVisibleOn = sendtMessage.NextVisibleOn;

            return message;

        }

        // Gets first message as QueueMessage without removing from queue, but makes it invisible for 30 seconds.
        public async Task<ProvisioningQueueParentDto> ReceiveMessageAsync()
        {
            _logger.LogInformation($"Queue: Receive message");
            var messageFromQueue = await _queueService.ReceiveMessageAsync();

            if (messageFromQueue != null)
            {
                var convertedMessage = JsonSerializerUtil.Deserialize<ProvisioningQueueParentDto>(messageFromQueue.MessageText);

                convertedMessage.MessageId = messageFromQueue.MessageId;
                convertedMessage.PopReceipt = messageFromQueue.PopReceipt;
                convertedMessage.NextVisibleOn = messageFromQueue.NextVisibleOn;

                return convertedMessage;
            }

            return null;
        }

        // Message needs to be retrieved with ReceiveMessageAsync() to be able to be deleted.
        public async Task DeleteMessageAsync(ProvisioningQueueParentDto message)
        {
            _logger.LogInformation($"Queue: Deleting message: {message.MessageId} with description \"{message.Description}\", having {message.Children.Count} children");
            await _queueService.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }

        // Message needs to be retrieved with ReceiveMessageAsync() to be able to be deleted.
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            _logger.LogInformation($"Queue: Deleting message: {messageId}");
            await _queueService.DeleteMessageAsync(messageId, popReceipt);
        }      

        public async Task DeleteQueueAsync()
        {
            await _queueService.DeleteQueueAsync();
        }

        public async Task IncreaseInvisibilityAsync(ProvisioningQueueParentDto message, int invisibleForInSeconds)
        {
            _logger.LogInformation($"Queue: Increasing message invisibility for {message.MessageId} with description \"{message.Description}\" by {invisibleForInSeconds} seconds.");
            var messageAsJson = JsonSerializerUtil.Serialize(message);
            var updateReceipt = await _queueService.UpdateMessageAsync(message.MessageId, message.PopReceipt, messageAsJson, invisibleForInSeconds);
            message.PopReceipt = updateReceipt.PopReceipt;
            message.NextVisibleOn = updateReceipt.NextVisibleOn;
            _logger.LogInformation($"Queue: Message {message.MessageId} will be visible again at {updateReceipt.NextVisibleOn} (UTC)");

        }

        public async Task ReQueueMessageAsync(ProvisioningQueueParentDto message, int? invisibleForInSeconds = default, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Queue: Re-queuing message: {message.Description}, having {message.Children.Count} children.");

            await DeleteMessageAsync(message);
            message.DequeueCount = 0;
            message.PopReceipt = null;
            message.MessageId = null;

            TimeSpan invisibleForTimespan = invisibleForInSeconds.HasValue ? new TimeSpan(0, 0, invisibleForInSeconds.Value) : new TimeSpan(0, 0, 10);
            await SendMessageAsync(message, visibilityTimeout: invisibleForTimespan, cancellationToken: cancellationToken);
        }

        public async Task IncreaseInvisibleBasedOnResource(CloudResourceOperationDto currentOperation, ProvisioningQueueParentDto queueParentItem)
        {
            var increaseBy = ResourceProivisoningTimeoutResolver.GetTimeoutForOperationInSeconds(currentOperation.Resource.ResourceType, currentOperation.OperationType);
            await IncreaseInvisibilityAsync(queueParentItem, increaseBy);
        }

        public async Task CreateItemAndEnqueue(int operationId, string operationDescription)
        {
            var queueParentItem = new ProvisioningQueueParentDto
            {
                Description = operationDescription
            };

            queueParentItem.Children.Add(new ProvisioningQueueChildDto() { ResourceOperationId = operationId });

            await SendMessageAsync(queueParentItem);
        }

        public async Task CreateItemAndEnqueue(CloudResourceOperation operation)
        {
            await CreateItemAndEnqueue(operation.Id, operation.Description);
        }

        public async Task CreateItemAndEnqueue(CloudResourceOperationDto operation)
        {
            await CreateItemAndEnqueue(operation.Id, operation.Description);
        }       

        public async Task AddNewQueueMessageForOperation(CloudResourceOperation operation)
        {
            var queueParentItem = new ProvisioningQueueParentDto
            {
                Description = operation.Description
            };

            queueParentItem.Children.Add(new ProvisioningQueueChildDto() { ResourceOperationId = operation.Id });

            await SendMessageAsync(queueParentItem, visibilityTimeout: TimeSpan.FromSeconds(5));
        }
    }
}
