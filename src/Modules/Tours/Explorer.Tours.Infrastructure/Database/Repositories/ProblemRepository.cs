using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.Core.Domain.Problems;
using Microsoft.EntityFrameworkCore;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.BuildingBlocks.Infrastructure.Database;

namespace Explorer.Tours.Infrastructure.Database.Repositories;

internal class ProblemRepository : IProblemRepository
{
    private readonly DbSet<Problem> _problems;
    private readonly ToursContext _context;

    public ProblemRepository(ToursContext context)
    {
        _context = context;
        _problems = _context.Set<Problem>();
    }

    public PagedResult<Problem> GetByTouristId(int userId, int page, int pageSize)
    {
        var problems = _problems.Include(p => p.Messages).Where(p => p.IdTourist == userId).GetPagedById(page, pageSize);
        return problems.Result;
    }

    public PagedResult<Problem> GetByGuideId(int guideId, int page, int pageSize)
    {
        var problems = _problems.Include(p => p.Messages).Where(p => p.IdGuide == guideId).GetPagedById(page, pageSize);
        return problems.Result;
    }

    public PagedResult<Problem> GetUnsolvedProblems(int page, int pageSize)
    {
        var problems = _problems.Include(p => p.Messages).Where(p => p.IsSolved == false).GetPagedById(page, pageSize);
        return problems.Result;
    }

    public PagedResult<Problem> GetProblemsOfUser(int userId, int page, int pageSize)
    {
         var problems = _problems.Include(p => p.Messages).Where(p =>p.IdGuide == userId || p.IdTourist == userId).GetPagedById(page, pageSize);
         return problems.Result;
        
    }
}

