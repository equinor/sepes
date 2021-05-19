﻿using Sepes.Common.Dto;
using Sepes.Common.Dto.Study;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service.Interface
{
    public interface IStudyParticipantLookupService
    {       
        Task<IEnumerable<ParticipantLookupDto>> GetLookupAsync(string searchText, int limit = 30, CancellationToken cancellationToken = default);   
           
    }
}
