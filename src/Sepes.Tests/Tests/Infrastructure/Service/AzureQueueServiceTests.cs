//using Microsoft.Extensions.DependencyInjection;
//using Sepes.Infrastructure.Service;
//using Sepes.Tests.Setup;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;


//namespace Sepes.Tests.Services.Azure
//{
//    public class AzureQueueServiceTests
//    {
//        public ServiceCollection Services { get; private set; }
//        public ServiceProvider ServiceProvider { get; protected set; }        

//        public AzureQueueServiceTests()
//        {
//            Services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();
//            Services.AddTransient<IAzureQueueService, AzureQueueService>();
//            ServiceProvider = Services.BuildServiceProvider();
//        }     

//        IAzureQueueService Init()
//        {
//            var queueService = ServiceProvider.GetService<IAzureQueueService>();
//            return queueService;
//        }

//        async Task Cleanup()
//        {
//            var queueService = ServiceProvider.GetService<IAzureQueueService>();
//            await queueService.DeleteQueueAsync();
//        }

//        //[Fact]
//        //public async void SendMessage_ShouldSendMessage()
//        //{
//        //    var queueService = Init();
          
//        //    var msgsBefore = await queueService.PeekMessagesAsync(32);
//        //    int noBefore = msgsBefore.ToList().Count;

//        //    await queueService.SendMessageAsync("Test message");

//        //    var msgsAfter = await queueService.PeekMessagesAsync(32);
//        //    int noAfter = msgsAfter.ToList().Count;

//        //    Assert.Equal<int>(noBefore + 1, noAfter);
//        //    await Cleanup();
//        //}

//        //[Fact]
//        //public async void SandboxResourceOperationToMessageString_ShouldConvertToSameFormatAsFoundInQueue()
//        //{
//        //    var queueService = Init();
//        //    var convertedOperation = queueService.SandboxResourceOperationToMessageString(mockOperation);
//        //    await queueService.SendMessage(createMockMessage());

//        //    var recievedMessageResult = await queueService.RecieveMessage();

//        //    Assert.Equal(recievedMessageResult.MessageText, convertedOperation);
//        //    await Cleanup();
//        //}

//        //[Fact]
//        //public async void MessageToSandboxResourceOperation_ShouldConvertToCorrectObject()
//        //{
//        //    var queueService = Init();
//        //    await queueService.SendMessage(createMockMessage());

//        //    var recievedMessageResult = await queueService.RecieveMessages(5);
//        //    var recievedMessage = recievedMessageResult.First();
//        //    var deserializedMessage = queueService.MessageToSandboxResourceOperation(recievedMessage);

//        //    Assert.Equal<SandboxResourceOperationDto>(mockOperation, deserializedMessage);

//        //    await Cleanup();
//        //}
//    }
//}
