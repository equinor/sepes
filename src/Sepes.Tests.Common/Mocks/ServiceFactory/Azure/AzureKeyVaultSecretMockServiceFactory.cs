using Moq;
using Sepes.Azure.Service.Interface;

namespace Sepes.Test.Common.ServiceMockFactories
{
    public static class AzureKeyVaultSecretMockServiceFactory
    {
        public static Mock<IAzureKeyVaultSecretService> CreateBasicForResourceCreate()
        {
            var mock = new Mock<IAzureKeyVaultSecretService>();

            mock.Setup(us => us.AddKeyVaultSecret(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(new System.Uri("https://nonexistenkeyvault.microsoft.net"));

            mock.Setup(us => us.DeleteKeyVaultSecretValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
               .ReturnsAsync("integrationtest");

            return mock;
        }

       
    }
}
