//using Microsoft.Extensions.DependencyInjection;
//using Sepes.Common.Constants;
//using Sepes.Common.Dto;
//using Sepes.Infrastructure.Service;
//using Sepes.Infrastructure.Service.Interface;
//using Sepes.Tests.Setup;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;


//namespace Sepes.Tests.Services.Azure
//{
//    public class ResourceProvisioningQueueServiceTests
//    {
//        public ServiceCollection Services { get; private set; }
//        public ServiceProvider ServiceProvider { get; protected set; }      

//        private readonly ProvisioningQueueParentDto mockOperation = new ProvisioningQueueParentDto
//        {          
//            Children = new System.Collections.Generic.List<ProvisioningQueueChildDto>() {

//                new ProvisioningQueueChildDto(){            
//                  SandboxResourceOperationId = 2                
//                }
//            }
//        };

//        public ResourceProvisioningQueueServiceTests()
//        {
//            Services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();
//            Services.AddTransient<IProvisioningQueueService, ProvisioningQueueService>();
//            ServiceProvider = Services.BuildServiceProvider();
//        }    

//        async Task<IProvisioningQueueService> InitAsync()
//        {
//            var queueService = ServiceProvider.GetService<IProvisioningQueueService>();
//            await queueService.DeleteQueueAsync();
//            return queueService;
//        }

//        async Task Cleanup()
//        {
//            var queueService = ServiceProvider.GetService<IProvisioningQueueService>();          
//            await queueService.DeleteQueueAsync();
//        }

//        [Fact]
//        public async void SendMessage_ShouldSendMessage()
//        {
//            var queueService = await InitAsync();
        
//            var msgsBefore = await queueService.RecieveMessageAsync();

//            Assert.Null(msgsBefore);          

//            await queueService.SendMessageAsync(mockOperation);

//            var msgsAfter = await queueService.RecieveMessageAsync();
//            Assert.NotNull(msgsAfter);
            

//            var messageChild = msgsAfter.Children.SingleOrDefault();
//            Assert.NotNull(messageChild);
//            Assert.Equal(2, messageChild.SandboxResourceOperationId);       
     

//            await Cleanup();
//        }       
       
//    }
//}
