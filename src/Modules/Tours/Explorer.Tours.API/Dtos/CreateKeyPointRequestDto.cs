using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class CreateKeyPointRequestDto
    {
        public KeyPointDto KeyPoint { get; set; } = new();
        public AddEncounterToKeyPointDto? Encounter { get; set; }
    }
}
