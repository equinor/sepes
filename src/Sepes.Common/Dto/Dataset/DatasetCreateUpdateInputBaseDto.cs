﻿namespace Sepes.Common.Dto.Dataset
{
    public class DatasetCreateUpdateInputBaseDto
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string Classification { get; set; }

        public int DataId { get; set; }      
    }
}
