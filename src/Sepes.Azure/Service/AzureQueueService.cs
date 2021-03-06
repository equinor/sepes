﻿using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sepes.Azure.Dto.Queue;
using Sepes.Azure.Service.Interface;

namespace Sepes.Azure.Service
{
    public class AzureQueueService : IAzureQueueService
    {
        readonly ILogger _logger;
        string _connectionString;
        string _queueName;

        public AzureQueueService(ILogger<AzureQueueService> logger)
        {
            _logger = logger;
        }

        public void Init(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task<QueueStorageItem> SendMessageAsync(string message, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default)
        {
            var client = await CreateQueueClient();
            //var base64Message = Base64Encode(message);
            var sendResponse = await client.SendMessageAsync(message, visibilityTimeout, cancellationToken: cancellationToken);

            return new QueueStorageItem() { MessageId = sendResponse.Value.MessageId, MessageText = message, PopReceipt = sendResponse.Value.PopReceipt, NextVisibleOn = sendResponse.Value.TimeNextVisible };          
        }       

        // Gets first message as QueueMessage without removing from queue, but makes it invisible for 30 seconds.
        public async Task<QueueStorageItem> ReceiveMessageAsync()
        {
            var client = await CreateQueueClient();
            QueueMessage[] messages = await client.ReceiveMessagesAsync();
            var firstMessage = messages.FirstOrDefault();

            if (firstMessage != null)
            {
                return new QueueStorageItem() { MessageId = firstMessage.MessageId, MessageText = firstMessage.MessageText, PopReceipt = firstMessage.PopReceipt, NextVisibleOn = firstMessage.NextVisibleOn };
            }

            return null;
        }

        // Updates the message in-place in the queue.
        // The message parameter is a message that has been fetched with ReceiveMessageAsync()
        public async Task<QueueUpdateReceipt> UpdateMessageAsync(string messageId, string popReceipt, string updatedMessage, int timespan = 30)
        {
            var client = await CreateQueueClient();        
            var updateReceipt = await client.UpdateMessageAsync(messageId, popReceipt, updatedMessage, TimeSpan.FromSeconds(timespan));
            return new QueueUpdateReceipt(updateReceipt.Value.PopReceipt, updateReceipt.Value.NextVisibleOn);
        }

        // Message needs to be retrieved with ReceiveMessageAsync() to be able to be deleted.
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            var client = await CreateQueueClient();
            await client.DeleteMessageAsync(messageId, popReceipt);
        }

        public async Task DeleteQueueAsync()
        {
            var client = await CreateQueueClient();
            _ = await client.DeleteIfExistsAsync();
        }

        // Helper method for creating queueClient
        async Task<QueueClient> CreateQueueClient()
        {
            if (String.IsNullOrWhiteSpace(_connectionString) || String.IsNullOrWhiteSpace(_queueName))
            {
                throw new NullReferenceException("ConnectionString or Queue name is null. Remember to call Init() method, providing a connection string and queue name");
            }

            var queueClientOptions = new QueueClientOptions()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(_connectionString, _queueName, queueClientOptions);
            
            var logMessagePrefix = $"Ensuring queue '{queueClient.Name}' exists. ";

            if (await queueClient.ExistsAsync())
            {
                _logger.LogTrace(logMessagePrefix + "Allready exists");
            }
            else
            {
                _logger.LogTrace(logMessagePrefix + "Did not exsist. Will create it");
            }

            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();         

            return queueClient;
        }

        //static string Base64Encode(string plainText)
        //{
        //    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        //    return System.Convert.ToBase64String(plainTextBytes);

        //}

        //static string Base64Decode(string encodedText)
        //{
        //    var plainTextBytes = System.Convert.FromBase64String(encodedText); System.Text.Encoding.UTF8.GetBytes(encodedText);
        //    return System.Text.Encoding.UTF8.GetString(plainTextBytes);
        //}
    }
}
