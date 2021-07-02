using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Tests.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sepes.Tests.Services.DomainServices.VirtualMachine
{
    public class VirtualMachineSizeServiceBase: TestBaseWithInMemoryDb
    {
        protected async Task<SepesDbContext> GetCalculateVmPrice_ShouldReturnPrice_RefreshAndSeedTestDatabase()
        {
           // return await ClearTestDatabase();
            
            var db = await ClearTestDatabase();

            var regionVmSize = new RegionVmSize()
            {
                RegionKey = "norwayeast",
                VmSizeKey = "Size1",
                Price = 10
            };

            var regionDiskSize1 = new RegionDiskSize()
            {
                RegionKey = "norwayeast",
                VmDiskKey = "Disk1",
                Price = 5
            };

            var region = new Region()
            {
                Key = "norwayeast",
                Name = "norwayeast"
            };

            db.Regions.Add(region);
            db.RegionVmSize.Add(regionVmSize);
            db.RegionDiskSize.Add(regionDiskSize1);

            await db.SaveChangesAsync();
            return db;
            
        }
    }
}
