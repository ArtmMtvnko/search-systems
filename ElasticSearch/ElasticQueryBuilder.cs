namespace ElasticSearch;

static class ElasticQueryBuilder
{
    public static object CreateMatchAllQuery() => new
    {
        query = new
        {
            match_all = new { }
        }
    };

    public static object CreateTextSearchQuery(IEnumerable<string> fields, string query) => new
    {
        query = new
        {
            multi_match = new
            {
                query,
                fields
            }
        }
    };

    public static object CreateFilterQuery(FilterParams filterParams)
    {
        var filters = BuildFilters(filterParams);

        return new
        {
            query = new
            {
                constant_score = new
                {
                    filter = new
                    {
                        @bool = new
                        {
                            filter = filters
                        }
                    }
                }
            }
        };
    }

    private static List<object> BuildFilters(FilterParams filterParams)
    {
        var filters = new List<object>
        {
            CreateRangeFilter("first_appeared", filterParams.FirstAppearedAfter, filterParams.FirstAppearedBefore),
            CreateRangeFilter("active_users", filterParams.ActiveUsersMoreThan, filterParams.ActiveUsersLessThan)
        };

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
        {
            filters.Add(CreateWildcardFilter("name", filterParams.Name));
        }

        if (filterParams.IsStaticallyTyped is not null)
        {
            filters.Add(CreateTermFilter("is_statically_typed", filterParams.IsStaticallyTyped.Value));
        }

        if (filterParams.Paradigms.Count > 0)
        {
            filters.Add(CreateTermsFilter("paradigms", filterParams.Paradigms));
        }

        return filters;
    }

    private static object CreateRangeFilter<T>(string field, T? gte, T lte) => new
    {
        range = new Dictionary<string, object>
        {
            [field] = new
            {
                gte,
                lte
            }
        }
    };

    private static object CreateTermsFilter(string field, IEnumerable<string> values) => new
    {
        terms = new Dictionary<string, IEnumerable<string>>
        {
            [field] = values
        }
    };

    private static object CreateWildcardFilter(string field, string value) => new
    {
        wildcard = new Dictionary<string, object>
        {
            [field] = new
            {
                value,
                case_insensitive = true
            }
        }
    };

    private static object CreateTermFilter(string field, bool value) => new
    {
        term = new Dictionary<string, object>
        {
            [field] = value
        }
    };
}
