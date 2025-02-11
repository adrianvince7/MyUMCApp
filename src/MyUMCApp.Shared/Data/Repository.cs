using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MyUMCApp.Shared.Data;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var entry = await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entry.Entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList);
        await Context.SaveChangesAsync();
        return entityList;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? await DbSet.CountAsync()
            : await DbSet.CountAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet.AnyAsync(predicate);
    }

    public virtual IQueryable<TEntity> Query()
    {
        return DbSet.AsQueryable();
    }
} 