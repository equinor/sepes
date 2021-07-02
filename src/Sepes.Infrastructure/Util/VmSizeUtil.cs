using Sepes.Infrastructure.Model;

namespace Sepes.Infrastructure.Util
{
    public static class VmSizeUtil
    {
        public static string GetDisplayTextSizeForDropdown(VmSize vmSizeInfo)
        {
            if (vmSizeInfo == null)
            {
                return "unknown";
            }

            return $"{vmSizeInfo.Key} ({vmSizeInfo.NumberOfCores} cores, {vmSizeInfo.MemoryGB} GB Memory, os disk: {vmSizeInfo.OsDiskSizeInMB}, max data disks: {vmSizeInfo.MaxDataDiskCount})";
        }
    }
}