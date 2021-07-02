using System;
using System.Collections.Generic;
using Sepes.Common.Model;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;

namespace Sepes.Tests.ModelFactory
{
    public static class SandboxPopulator
    {
        public static void Add(SepesDbContext db, int sandboxId, string sandboxName, List<SandboxPhaseHistory> allPhases, List<SandboxDataset> allDatasets)
        {
            var newSb = new Sandbox()
            {
                Id = sandboxId,
                Name = sandboxName,
                Region = "norwayeast",
                Created = DateTime.UtcNow,
                CreatedBy = "unittest",
                PhaseHistory = allPhases,
                SandboxDatasets = allDatasets
            };

            db.Sandboxes.Add(newSb);
            db.SaveChanges();
        }

        public static void AddWithTwoDatasets(SepesDbContext db, int sandboxId, string sandboxName, List<SandboxPhaseHistory> allPhases)
        {
            var datasets = new List<SandboxDataset>() {
                new SandboxDataset() { Dataset = new Dataset() { Id = 1, Name = "ds1", Location = "norwayeast", StorageAccountName = "sa1Name" } },
                new SandboxDataset() { Dataset = new Dataset() { Id = 2, Name = "ds2", Location = "norwayeast", StorageAccountName = "sa2Name" } }
            };

            Add(db, sandboxId, sandboxName, allPhases, datasets);
         
        }

        public static void AddWithoutPhases(SepesDbContext db, int sandboxId, string sandboxName)
        {
            AddWithTwoDatasets(db, sandboxId, sandboxName, new List<SandboxPhaseHistory>());
        }

        public static void AddWithOpen(SepesDbContext db, int sandboxId, string sandboxName)
        {
            var phaseHistory = new List<SandboxPhaseHistory>() { 
            
                new SandboxPhaseHistory(){ Counter = 0, Phase = SandboxPhase.Open }
            
            };

            AddWithTwoDatasets(db, sandboxId, sandboxName, phaseHistory);
        }

        public static void AddWithOpenAndDataAvailable(SepesDbContext db, int sandboxId, string sandboxName)
        {
            var phaseHistory = new List<SandboxPhaseHistory>() {

                new SandboxPhaseHistory(){ Counter = 0, Phase = SandboxPhase.Open },
                new SandboxPhaseHistory(){ Counter = 1, Phase = SandboxPhase.DataAvailable }

            };

            AddWithTwoDatasets(db, sandboxId, sandboxName, phaseHistory);
        }
    }
}
