using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Repositories;
using SchoolPortal.Application.Wrappers;

namespace SchoolPortal.Application.Services
{
    public class Service<T> : IService<T> where T : class
    {
        protected readonly IRepository<T> _repository;

        public Service(IRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual Task<IList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return _repository.GetAllAsync(cancellationToken);
        }

        public virtual Task<T> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return _repository.FindAsync(id, cancellationToken);
        }

        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> where)
        {
            return _repository.FindBy(where);
        }

        public virtual Task<IList<T>> FindByAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken)
        {
            return _repository.FindByAsync(where, cancellationToken);
        }

        public virtual Task<ITransactionResult> CreateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken)
        {
            return _repository.CreateAsync(entity, autoSaveAll, cancellationToken);
        }

        public virtual Task<ITransactionResult> UpdateAsync(T entity, bool autoSaveAll, CancellationToken cancellationToken)
        {
            return _repository.UpdateAsync(entity, autoSaveAll, cancellationToken);
        }

        public virtual Task<ITransactionResult> SaveAllAsync(CancellationToken cancellationToken)
        {
            return _repository.SaveAllAsync(cancellationToken);
        }
    }
}
