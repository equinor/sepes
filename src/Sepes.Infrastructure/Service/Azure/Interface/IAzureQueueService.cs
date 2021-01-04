﻿using Azure.Storage.Queues.Models;
using Sepes.Infrastructure.Dto.Azure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public interface IAzureQueueService
    {
        void Init(string connectionString, string queueName);

        Task<QueueStorageItemDto> SendMessageAsync(string messageText, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);
       
        // Gets first message without removing from queue, but makes it invisible for 30 seconds.
        Task<QueueStorageItemDto> RecieveMessageAsync();

        // Gets messages from queue without making them invisible.
        //Task<IEnumerable<PeekedMessage>> PeekMessagesAsync(int numberOfMessages);

        // Gets message without removing from queue, but makes it invisible for 30 seconds.
        //Task<IEnumerable<QueueMessage>> RecieveMessagesAsync(int numberOfMessages);

        // Updates the message in-place in the queue.
        // The message parameter is a message that has been fetched with RecieveMessage() or RecieveMessages()
        Task<UpdateReceipt> UpdateMessageAsync(string messageId, string popReceipt, string updatedMessage, int timespan = 30);

        // Returns approximate number of messages in queue.
        // The number is not lower than the actual number of messages in the queue, but could be higher.
        //Task<int> GetApproximateNumberOfMessengesInQueueAsync();

        // Message needs to be retrieved with recieveMessage(s)() to be able to be deleted. 
        Task DeleteMessageAsync(string messageId, string popReceipt);

        Task DeleteQueueAsync();       
    }
}
