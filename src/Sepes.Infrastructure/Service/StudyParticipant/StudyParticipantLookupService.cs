﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sepes.Azure.Dto;
using Sepes.Azure.Service.Interface;
using Sepes.Common.Dto;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.DataModelService.Interface;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class StudyParticipantLookupService : StudyParticipantBaseService, IStudyParticipantLookupService
    {      
        readonly ICombinedUserLookupService _combinedUserLookupService;
       

        public StudyParticipantLookupService(SepesDbContext db,
            ILogger<StudyParticipantLookupService> logger,
            IMapper mapper,
            IUserService userService,
            ICombinedUserLookupService combinedUserLookupService,
            IStudyEfModelService studyModelService,
            IProvisioningQueueService provisioningQueueService,
            ICloudResourceReadService cloudResourceReadService,
            ICloudResourceOperationCreateService cloudResourceOperationCreateService,
            ICloudResourceOperationUpdateService cloudResourceOperationUpdateService)
            : base(db, mapper, logger, userService, studyModelService, provisioningQueueService, cloudResourceReadService, cloudResourceOperationCreateService, cloudResourceOperationUpdateService)
        {
            _combinedUserLookupService = combinedUserLookupService;            
        }

        public async Task<IEnumerable<ParticipantLookupDto>> GetLookupAsync(string searchText, int limit = 30, CancellationToken cancellationToken = default)
        { 
            if (_userService.IsMockUser()) //If mock user, he can only add him self
            {
                var currentUser = await _userService.GetCurrentUserAsync();

                var listWithMockUser = new List<ParticipantLookupDto>
                {
                    new ParticipantLookupDto
                    {
                        ObjectId = currentUser.ObjectId,
                        FullName = currentUser.FullName,
                        UserName = currentUser.UserName,
                        EmailAddress = currentUser.EmailAddress,
                        Source = "Azure"
                    }
                };

                return listWithMockUser;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return new List<ParticipantLookupDto>();
            }

            Task<Dictionary<string, AzureUserDto>> usersFromAzureAdTask = null;

            try
            {
                usersFromAzureAdTask = _combinedUserLookupService.SearchAsync(searchText, limit, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Could not get user list from Azure. Use only list from DB instead");
            }

            var usersFromDbTask = _db.Users.Where(u => u.EmailAddress.StartsWith(searchText) || u.FullName.StartsWith(searchText) || u.ObjectId.Equals(searchText)).ToListAsync(cancellationToken);

            await Task.WhenAll(usersFromDbTask, usersFromAzureAdTask);

            var usersFromDb = _mapper.Map<IEnumerable<ParticipantLookupDto>>(usersFromDbTask.Result);
            var usersFromDbAsDictionary = new Dictionary<string, ParticipantLookupDto>();

            foreach (var curUserFromDb in usersFromDb)
            {
                if (string.IsNullOrWhiteSpace(curUserFromDb.ObjectId))
                {
                    continue;
                }

                if (!usersFromDbAsDictionary.ContainsKey(curUserFromDb.ObjectId))
                {
                    usersFromDbAsDictionary.Add(curUserFromDb.ObjectId, curUserFromDb);
                }
            }

            if (usersFromAzureAdTask.IsCompletedSuccessfully)
            {
                foreach (var curAzureUser in usersFromAzureAdTask.Result)
                {
                    if (!usersFromDbAsDictionary.ContainsKey(curAzureUser.Key))
                    {
                        usersFromDbAsDictionary.Add(curAzureUser.Key, _mapper.Map<ParticipantLookupDto>(curAzureUser.Value));
                    }
                }
            }

            return usersFromDbAsDictionary.OrderBy(o => o.Value.FullName).Select(o => o.Value);
        }
    }
}
