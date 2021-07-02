using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sepes.Common.Constants;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyDeleteService : IStudyDeleteService
    {      
        readonly SepesDbContext _db;
        readonly IUserService _userService;
        readonly IStudyEfModelService _studyModelService;

        readonly IStudyLogoDeleteService _studyLogoDeleteService;
        readonly IStudySpecificDatasetService _studySpecificDatasetService;
        readonly ICloudResourceReadService _cloudResourceReadService;

        public StudyDeleteService(ILogger<StudyDeleteService> logger,
            SepesDbContext db,
            IUserService userService,
            IStudyEfModelService studyModelService,           
            IStudyLogoDeleteService studyLogoDeleteService,
            IStudySpecificDatasetService studySpecificDatasetService,
            ICloudResourceReadService cloudResourceReadService) 
        {           
            _db = db;
            _userService = userService;
            _studyModelService = studyModelService;

            _studyLogoDeleteService = studyLogoDeleteService;
            _studySpecificDatasetService = studySpecificDatasetService;
            _cloudResourceReadService = cloudResourceReadService;
        }

        public async Task CloseStudyAsync(int studyId)
        {
            var studyFromDb = await _studyModelService.GetForCloseAsync(studyId, UserOperation.Study_Close);

            ValidateStudyForCloseOrDeleteThrowIfNot(studyFromDb);

            await _studySpecificDatasetService.SoftDeleteAllStudySpecificDatasetsAsync(studyFromDb);
            await _studySpecificDatasetService.DeleteAllStudyRelatedResourcesAsync(studyFromDb);

            var currentUser = await _userService.GetCurrentUserAsync();
            studyFromDb.Closed = true;
            studyFromDb.ClosedBy = currentUser.UserName;
            studyFromDb.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteStudyAsync(int studyId)
        {
            var studyFromDb = await _studyModelService.GetForDeleteAsync(studyId, UserOperation.Study_Delete);

            ValidateStudyForCloseOrDeleteThrowIfNot(studyFromDb);

            await _studyLogoDeleteService.DeleteAsync(studyFromDb.LogoUrl);

            await _studySpecificDatasetService.HardDeleteAllStudySpecificDatasetsAsync(studyFromDb);
            await _studySpecificDatasetService.DeleteAllStudyRelatedResourcesAsync(studyFromDb);

            await RemoveSandboxAndRelatedEntriesFromContext(studyFromDb);

            await RemoveStudyParticipantsAndRelatedEntries(studyFromDb);
        }

        void ValidateStudyForCloseOrDeleteThrowIfNot(Study studyFromDb)
        {
            foreach (var curSandbox in studyFromDb.Sandboxes)
            {
                if (!curSandbox.Deleted)
                {
                    throw new Exception($"Cannot delete study {studyFromDb.Id}, it has open sandboxes that must be deleted first");
                }
            }
        }

        async Task RemoveSandboxAndRelatedEntriesFromContext(Study studyFromDb)
        {
            foreach (var curSandbox in studyFromDb.Sandboxes)
            {
                var sandboxResources = await _cloudResourceReadService.GetSandboxResourcesForDeletion(curSandbox.Id);

                foreach (var curResource in sandboxResources)
                {
                    foreach (var curOperation in curResource.Operations)
                    {
                        if (curOperation.DependsOnOperation != null)
                        {
                            if (_db.CloudResourceOperations.Contains(curOperation.DependsOnOperation))
                            {
                                _db.CloudResourceOperations.Remove(curOperation.DependsOnOperation);
                            }
                        }

                        if (_db.CloudResourceOperations.Contains(curOperation))
                        {
                            _db.CloudResourceOperations.Remove(curOperation);
                        }
                    }

                    _db.CloudResources.Remove(curResource);
                }

                _db.Sandboxes.Remove(curSandbox);
            }

            await _db.SaveChangesAsync();
        }

        async Task RemoveStudyParticipantsAndRelatedEntries(Study studyFromDb)
        {
            var userEntriesForDeletedStudyParticipants = new HashSet<int>();

            foreach (var curParticipant in studyFromDb.StudyParticipants)
            {
                if (!userEntriesForDeletedStudyParticipants.Contains(curParticipant.UserId))
                {
                    userEntriesForDeletedStudyParticipants.Add(curParticipant.UserId);
                }

                _db.StudyParticipants.Remove(curParticipant);

            }

            await _db.SaveChangesAsync();

            foreach (var curUserId in userEntriesForDeletedStudyParticipants)
            {
                var userEntry = await _db.Users.Include(u => u.StudyParticipants).FirstOrDefaultAsync(u => u.Id == curUserId);

                if (userEntry != null)
                {
                    if (userEntry.StudyParticipants.Count == 0)
                    {
                        _db.Users.Remove(userEntry);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
