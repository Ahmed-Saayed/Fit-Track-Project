using FitTrack_Pro.Models;
using System.Linq.Expressions;

namespace Common
{
	public class GenericRepository<T>(ApplicationDbContext _context) : IGenericRepository<T> where T : class
	{
		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _context.Set<T>().ToListAsync();
		}

		public async Task<T?> GetByIdAsync(int id)
		{
			return await _context.Set<T>().FindAsync(id);
		}

		public async Task AddAsync(T entity)
		{
			await _context.Set<T>().AddAsync(entity);
		}

		public void Update(T entity)
		{
			_context.Set<T>().Update(entity);
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var entity = await _context.Set<T>().FindAsync(id);

			if (entity == null)
			{
				return false;
			}

			_context.Set<T>().Remove(entity);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
		{
			IQueryable<T> query = _context.Set<T>();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			return await query.CountAsync();
		}

		public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null)
		{
			IQueryable<T> query = _context.Set<T>();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			return await query
						.Skip((pageNumber - 1) * pageSize)
						.Take(pageSize)
						.ToListAsync();
		}
	}
}
	
