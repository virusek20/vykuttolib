namespace vykuttolib.Services.Search
{
    public interface ISearchService<TQuery, TReturn>
        where TQuery : ISearchQuery<TReturn>
    {
        TQuery Search();
    }
}
