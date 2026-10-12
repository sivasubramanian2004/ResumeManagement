using Microsoft.EntityFrameworkCore;
namespace ResumeManagement.Api.Repositories
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
    {
        private readonly AppDbContext _context = context;
        private DbSet<T> _entities = null!;

        /// <summary>
        /// Gets the entities set.
        /// </summary>
        protected virtual DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _context.Set<T>();

                return _entities;
            }
        }

       
        public virtual IQueryable<T> Table
        {
            get
            {
                return this.Entities;
            }
        }

        /// <summary>
        /// Gets a queryable table with "no tracking" enabled (EF feature). Use it only for read-only operations.
        /// </summary>
        public virtual IQueryable<T> TableNoTracking
        {
            get
            {
                return this.Entities.AsNoTracking();
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await Entities.AddAsync(entity);
        }

        public virtual void UpdateEntity(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Entities.Update(entity);
        }
        public virtual void DeleteEntity(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Entities.Remove(entity);
        }
        public virtual async Task SaveAsync()
        {
            await this._context.SaveChangesAsync();
        }

    }
}
