using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services.Identity;

namespace Infrastructure.Services.Identity;

public class RoleManager(IEfDbContext dbContext) : IRoleManager { }
