using Sepes.Common.Util;
using Sepes.Infrastructure.Model;

namespace Sepes.Infrastructure.Util
{
    public static class VmOsUtil
    {
        public static string GetOsName(CloudResource resource)
        {
            var vmSettings = CloudResourceConfigStringSerializer.VmSettings(resource.ConfigString);

            if (vmSettings != null)
            {
                return vmSettings.OperatingSystem;
            }

            return null;
        }
    }
}