using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class  AddEncounterToKeyPointDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int XP { get; set; }
        public string Type { get; set; } = "";

        // specificna po tipu
        public string? ActionDescription { get; set; }        // Misc
        public int? RequiredPeopleCount { get; set; }          // Social
        public double? RangeInMeters { get; set; }             // Social
        public string? ImageUrl { get; set; }                  // HiddenLocation

        // za keypoint
        public bool IsMandatory { get; set; }
    }
}
