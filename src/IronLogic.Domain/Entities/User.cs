using Microsoft.AspNetCore.Identity;

namespace IronLogic.Domain.Entities;

public class User : IdentityUser
{
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<DailyWeight> DailyWeights { get; set; } = new List<DailyWeight>();
}