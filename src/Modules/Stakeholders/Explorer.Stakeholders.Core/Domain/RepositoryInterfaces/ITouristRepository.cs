using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer.Stakeholders.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

public interface ITouristRepository
{
    Tourist Get(long touristId); // Vraća turistu po Tourist.Id
    Tourist GetByPersonId(long personId); // Vraća turistu po Tourist.PersonId
    Tourist Create(Tourist tourist);
    Tourist Update(Tourist tourist);
    List<Tourist> GetAll();
}
