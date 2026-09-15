using System.Collections.Generic;
using System.Linq;
using SchoolPortal.Application.Common;

namespace SchoolPortal.Api.ViewModels
{
    public class PagedViewModel<T>
    {
        public IList<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public static class PagedViewModel
    {
        public static PagedViewModel<TView> From<TSource, TView>(PagedResult<TSource> source, System.Func<TSource, TView> map)
        {
            return new PagedViewModel<TView>
            {
                Items = source.Items.Select(map).ToList(),
                Page = source.Page,
                PageSize = source.PageSize,
                TotalCount = source.TotalCount,
                TotalPages = source.TotalPages
            };
        }
    }
}
