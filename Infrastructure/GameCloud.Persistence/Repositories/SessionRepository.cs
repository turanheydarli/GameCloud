using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class SessionRepository(GameCloudDbContext context) : ISessionRepository
{
    public async Task<Session> CreateAsync(Session session)
    {
        context.Entry(session).State = EntityState.Added;
        await context.SaveChangesAsync();
        return session;
    }
}