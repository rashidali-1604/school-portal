using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Wrappers;

namespace SchoolPortal.Application.Services
{
    public interface IService<T> where T : class
    {
        Task<IList<T>> GetAllAsync(CancellationToken cancellationToken);

        Task<T> FindAsync(Guid id, CancellationToken cancellationToken);

        IQueryable<T> FindBy(Expression<Func<T, bool>> where);

        Task<IList<T>> FindByAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken);

        Task<ITransactionResult> CreateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken);

        Task<ITransactionResult> UpdateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken);

        Task<ITransactionResult> SaveAllAsync(CancellationToken cancellationToken);
    }
}
