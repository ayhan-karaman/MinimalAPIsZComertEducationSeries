namespace Repositories;
public abstract class RepositoryBase<T>
where T : class, new()
{
    protected readonly RepositoryContext _context;

    public RepositoryBase(RepositoryContext context)
    {
        _context = context;
    }

    public T? Get(int id)
    => _context
    .Set<T>()
    .Find(id);
    

    public virtual List<T> GetAll()
    => _context
    .Set<T>().ToList();

    public void Add(T item)
    {
        _context.Set<T>().Add(item);
        _context.SaveChanges();
    }

    public void Remove(T item)
    {
        _context.Set<T>().Remove(item);
        _context.SaveChanges();
    }

    public void Update(T item)
    {
        _context.Set<T>().Update(item);
        _context.SaveChanges();
    }
}