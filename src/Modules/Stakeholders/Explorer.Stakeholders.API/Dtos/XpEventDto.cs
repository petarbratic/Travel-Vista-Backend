using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.API.Dtos
{
    public class XpEventDto
    {
        public string Type { get; set; }
        public int Amount { get; set; }
        public long SourceEntityId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        // pripremljen tekst za front
        public string Description { get; set; }
    }
}
