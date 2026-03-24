using System.Linq.Expressions;

namespace Common
{
	public interface IGenericRepository<T> where T : class
	{
		Task<IEnumerable<T>> GetAllAsync();
		Task<T?> GetByIdAsync(int id);
		Task AddAsync(T entity);
		void Update(T entity);
		Task<bool> DeleteAsync(int id);

		Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
		Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null);
	}
}
