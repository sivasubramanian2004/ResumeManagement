namespace ResumeManagement.Api.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task SaveAsync();

        // Query
        IQueryable<T> Table { get; }

        IQueryable<T> TableNoTracking { get; }

        Task AddAsync(T entity);

        void UpdateEntity(T entity);
        void DeleteEntity(T entity);
    }
}
