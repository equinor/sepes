﻿namespace Sepes.Common.Dto.VirtualMachine
{
    public class VmSizeDto
    {
        public string Name { get; set; }

        public int NumberOfCores { get; set; }

        public int OsDiskSizeInMB { get; set; }

        public int ResourceDiskSizeInMB { get; set; }

        public int MemoryGB { get; set; }

        public int MaxDataDiskCount { get; set; }

        public string Region { get; set; }
    }
}
