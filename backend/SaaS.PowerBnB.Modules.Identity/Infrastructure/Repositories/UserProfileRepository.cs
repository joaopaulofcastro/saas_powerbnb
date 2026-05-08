using SaaS.PowerBnB.Modules.Identity.Domain.Entities;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Identity.Infrastructure.Repositories;

internal class UserProfileRepository
    : Repository<IdentityDbContext, UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(IdentityDbContext dbContext) : base(dbContext) { }
}
