using Microsoft.Extensions.DependencyInjection;
using Sepes.Tests.Setup;

namespace Sepes.Tests.Tests
{
    public class TestBase
    {
        protected readonly ServiceCollection _services;
        protected readonly ServiceProvider _serviceProvider;
                    
        public TestBase()
        {
            _services = BasicServiceCollectionFactory.GetServiceCollectionWithInMemory();
            _serviceProvider = _services.BuildServiceProvider();
        }
    }
}