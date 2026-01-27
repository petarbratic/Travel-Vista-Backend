using Explorer.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.Domain
{
    public class WalletTransaction : Entity
    {
        public long PersonId { get; private set; }
        public int AmountAc { get; private set; }                 
        public WalletTransactionType Type { get; private set; }
        public string Description { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public string? ReferenceType { get; private set; }         
        public long? ReferenceId { get; private set; }
        public long? InitiatorPersonId { get; private set; }      

        private WalletTransaction() { }

        public WalletTransaction(
            long personId,
            int amountAc,
            WalletTransactionType type,
            string description,
            string? referenceType = null,
            long? referenceId = null,
            long? initiatorPersonId = null)
        {
            if (personId == 0) throw new ArgumentException("Invalid personId.");
            if (amountAc == 0) throw new ArgumentException("Amount cannot be 0.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.");

            PersonId = personId;
            AmountAc = amountAc;
            Type = type;
            Description = description.Trim();
            ReferenceType = referenceType;
            ReferenceId = referenceId;
            InitiatorPersonId = initiatorPersonId;
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
