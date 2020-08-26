﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sepes.Infrastructure.Dto;
using Sepes.Infrastructure.Exceptions;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Model.Context;
using Sepes.Infrastructure.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sepes.Infrastructure.Service
{
    public class ParticipantService : IParticipantService
    {
        readonly SepesDbContext _db;
        readonly IMapper _mapper;
        readonly IStudyService _studyService;

        public ParticipantService(SepesDbContext db, IMapper mapper, IStudyService studyService)
        {            
            _db = db;
            _mapper = mapper;
            _studyService = studyService;
        }

        public async Task<IEnumerable<ParticipantListItemDto>> GetLookupAsync()
        {
            var participantsFromDb = await _db.Participants.ToListAsync();
            var participantDtos = _mapper.Map<IEnumerable<ParticipantListItemDto>>(participantsFromDb);

            return participantDtos;  
        }

        public async Task<ParticipantDto> GetByIdAsync(int id)
        {
            var participantFromDb = await GetOrThrowAsync(id);

            var participantDto = _mapper.Map<ParticipantDto>(participantFromDb);

            return participantDto;
        } 
        
        async Task<Participant> GetOrThrowAsync(int id)
        {
            var entityFromDb = await _db.Participants.FirstOrDefaultAsync(s => s.Id == id);

            if (entityFromDb == null)
            {
                throw NotFoundException.CreateForIdentity("Participant", id);
            }

            return entityFromDb;
        }
    }
}
