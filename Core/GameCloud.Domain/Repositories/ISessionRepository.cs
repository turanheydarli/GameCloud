using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
}