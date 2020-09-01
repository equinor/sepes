﻿using Sepes.Infrastructure.Dto;
using System.Collections.Generic;

namespace Sepes.Infrastructure.Service
{
    public interface ILookupService
    {
        public IEnumerable<LookupDto> GetAzureRegions();
    }
}