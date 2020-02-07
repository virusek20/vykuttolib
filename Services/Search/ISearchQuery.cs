using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vykuttolib.Services.Search
{
    public interface ISearchQuery<TReturn>
    {
        Task<List<TReturn>> ToListAsync();
        IOrderedQueryable<TReturn> ToOrderedQuery();
        IQueryable<TReturn> ToEFQuery();
    }
}
