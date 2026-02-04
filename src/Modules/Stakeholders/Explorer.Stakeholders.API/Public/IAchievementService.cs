using Explorer.Stakeholders.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Explorer.Stakeholders.API.Public
{
    public interface IAchievementService
    {
        AchievementDto Create(AchievementDto achievement, long touristId);
        string BlogCreated(long touristId);
        List<AchievementDto> GetForTourist(long touristId);
        (string Name, string Description) GetMeta(string code);
    }
}
