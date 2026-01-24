using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.API.Dtos
{
    public class WalletTransactionDto
    {
        public long Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int AmountAc { get; set; }
        public int Type { get; set; }
        public string Description { get; set; } = "";
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
