﻿using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Extensions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using System.Linq;

namespace Sepes.Infrastructure.Service.Queries
{
    public static class SandboxBaseQueries
    {
        public static IQueryable<Sandbox> ActiveSandboxesBaseQueryable(SepesDbContext db)
        {
            return db.Sandboxes.Where(s => !s.Deleted);
        }

        public static IQueryable<Sandbox> SandboxDetailsQueryable(SepesDbContext db)
        {
            return ActiveSandboxesBaseQueryable(db)
                .Include(s => s.Study)
                .ThenInclude(s => s.StudyParticipants)
                .Include(sb => sb.PhaseHistory)
                .Include(ds => ds.Resources)
                 .ThenInclude(r => r.Operations)
                .Include(sb => sb.SandboxDatasets)
                    .ThenInclude(sd => sd.Dataset);
        }

        public static IQueryable<Sandbox> ActiveSandboxesMinimalIncludesQueryable(SepesDbContext db)
        {
            return ActiveSandboxesBaseQueryable(db)
                .Include(s => s.Study)
                    .ThenInclude(s => s.StudyParticipants);

        }

        public static IQueryable<Sandbox> ActiveSandboxesWithIncludesQueryable(SepesDbContext db)
        {
            return ActiveSandboxesMinimalIncludesQueryable(db)
                  .Include(sb => sb.SandboxDatasets)
                    .ThenInclude(sd => sd.Dataset)
                    .ThenInclude(ds => ds.Resources)
                .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations)
                .Include(sb => sb.PhaseHistory);
        }

        public static IQueryable<Sandbox> ForResourceCreation(SepesDbContext db)
        {
            return ActiveSandboxesBaseQueryable(db)
               .Include(s => s.Study)
                   .ThenInclude(s => s.StudyParticipants)
                     .ThenInclude(sp => sp.User)
                      .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations)
                .Include(sb => sb.PhaseHistory);
        }

        public static IQueryable<Sandbox> ActiveSandboxWithResourceAndOperations(SepesDbContext db)
        {
            return ActiveSandboxesMinimalIncludesQueryable(db)
                    .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations);
        }

        public static IQueryable<Sandbox> SandboxWithResources(SepesDbContext db)
        {
            return db.Sandboxes.Include(s => s.Study)
                .ThenInclude(s => s.StudyParticipants)
                    .Include(sb => sb.Resources);
        }

        public static IQueryable<Sandbox> SandboxWithStudyParticipantResourceAndOperations(SepesDbContext db)
        {
            return db.Sandboxes.Include(s => s.Study)
                .ThenInclude(s => s.StudyParticipants)
                    .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations);
        }

        public static IQueryable<Sandbox> SandboxWithResourceAndOperations(SepesDbContext db)
        {
            return db.Sandboxes
                    .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations);
        }



        public static IQueryable<Sandbox> AllSandboxesBaseQueryable(SepesDbContext db)
        {
            return db.Sandboxes.Include(s => s.Study)
                .ThenInclude(s => s.StudyParticipants)
                 .Include(sb => sb.SandboxDatasets)
                    .ThenInclude(sd => sd.Dataset)
                .Include(sb => sb.Resources)
                    .ThenInclude(r => r.Operations)
                .Include(sb => sb.PhaseHistory);
        }

        public static IQueryable<Sandbox> SandboxForDatasetOperations(SepesDbContext db, bool includePhase = false)
        {
            return ActiveSandboxesBaseQueryable(db)
                    .Include(sb => sb.Study)
                 .ThenInclude(s => s.StudyParticipants)
                .Include(sb => sb.Study)
                 .ThenInclude(s => s.StudyDatasets)
                     .ThenInclude(sd => sd.Dataset)
                      .ThenInclude(sd => sd.SandboxDatasets)
                .If(includePhase, x => x.Include(sb => sb.PhaseHistory));

        }

        public static IQueryable<Sandbox> SandboxForPhaseShift(SepesDbContext db)
        {
            return ActiveSandboxesMinimalIncludesQueryable(db)
                        .Include(sb => sb.Resources)
                            .ThenInclude(r => r.Operations)
                        .Include(sb => sb.PhaseHistory)
                        .Include(sb => sb.SandboxDatasets);
            //.ThenInclude(sds => sds.Dataset)
            //   .ThenInclude(ds => ds.Resources)
            //   .ThenInclude(ds => ds.Operations);

        }

        public static IQueryable<SandboxDataset> SandboxDatasetForPhaseShift(SepesDbContext db)
        {
            return db.SandboxDatasets
                   .Include(sds => sds.Dataset)
                            .ThenInclude(ds => ds.Resources)
                            .ThenInclude(ds => ds.Operations);
        }
    }
}
