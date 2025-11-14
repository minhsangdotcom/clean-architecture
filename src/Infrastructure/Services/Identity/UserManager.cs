using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services.Identity;

namespace Infrastructure.Services.Identity;

public class UserManager(IRoleManager roleManager, IEfDbContext dbContext) : IUserManager { }
