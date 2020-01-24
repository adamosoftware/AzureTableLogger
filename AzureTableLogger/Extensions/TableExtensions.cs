using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTableLogger.Extensions
{
    public static class TableExtensions
    {
        /// <summary>
        /// Thanks to https://www.vivien-chevallier.com/Articles/executing-an-async-query-with-azure-table-storage-and-retrieve-all-the-results-in-a-single-operation
        /// but I removed the cancellation token arg
        /// </summary>
        public async static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this TableQuery<TElement> tableQuery, Func<TElement, bool> filter = null)
        {
            var nextQuery = tableQuery;
            var continuationToken = default(TableContinuationToken);
            var results = new List<TElement>();

            do
            {                
                var segmentResults = await nextQuery.ExecuteSegmentedAsync(continuationToken);
                var filtered = (filter != null) ? segmentResults.Results.Where(filter) : segmentResults.Results;
                
                results.Capacity += filtered.Count();                
                results.AddRange(filtered);
                continuationToken = segmentResults.ContinuationToken;
                
                if (continuationToken != null && tableQuery.TakeCount.HasValue)
                {
                    var itemsToLoad = tableQuery.TakeCount.Value - results.Count;
                    nextQuery = itemsToLoad > 0
                        ? tableQuery.Take(itemsToLoad).AsTableQuery()
                        : null;
                }
            } while (continuationToken != null && nextQuery != null);

            return results;
        }
    }
}
