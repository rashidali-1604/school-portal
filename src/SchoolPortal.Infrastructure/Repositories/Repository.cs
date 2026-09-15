using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolPortal.Application.Repositories;
using SchoolPortal.Application.Wrappers;
using SchoolPortal.Infrastructure.Context;

namespace SchoolPortal.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<T> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> where)
        {
            return _context.Set<T>().Where(where);
        }

        public async Task<IList<T>> FindByAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().Where(where).ToListAsync(cancellationToken);
        }

        public async Task<ITransactionResult> CreateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Set<T>().AddAsync(entity, cancellationToken);

                if (!autoSaveAll)
                {
                    return new SuccessfulTransactionResult();
                }

                await _context.SaveChangesAsync(cancellationToken);
                return new SuccessfulTransactionResult();
            }
            catch (DbUpdateException ex) when (ex is not DbUpdateConcurrencyException)
            {
                return new FailedTransactionResult(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ITransactionResult> UpdateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken)
        {
            try
            {
                _context.Set<T>().Update(entity);

                if (!autoSaveAll)
                {
                    return new SuccessfulTransactionResult();
                }

                await _context.SaveChangesAsync(cancellationToken);
                return new SuccessfulTransactionResult();
            }
            catch (DbUpdateException ex) when (ex is not DbUpdateConcurrencyException)
            {
                return new FailedTransactionResult(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ITransactionResult> SaveAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return new SuccessfulTransactionResult();
            }
            catch (DbUpdateException ex) when (ex is not DbUpdateConcurrencyException)
            {
                return new FailedTransactionResult(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
