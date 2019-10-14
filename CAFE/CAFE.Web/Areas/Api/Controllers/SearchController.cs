
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using CAFE.Core.Resources;
using CAFE.Core.Searching;
using CAFE.Core.Security;
using CAFE.Web.Areas.Api.Models.Search;
using Microsoft.AspNet.Identity;

namespace CAFE.Web.Areas.Api.Controllers
{
    /// <summary>
    /// API endpoint for performs search for all data in system
    /// </summary>
    //[Authorize]
    [AllowAnonymous]
    public class SearchController : ApiController
    {
        private readonly ISearchService _searchService;
        private readonly ISecurityServiceAsync _securityService;

        public SearchController(ISearchService searchService, ISecurityServiceAsync securityService)
        {
            _searchService = searchService;
            _securityService = securityService;
        }

        /// <summary>
        /// Searches items by query and filters
        /// </summary>
        /// <param name="model">Query data</param>
        /// <returns>Found items</returns>
        [HttpPost]
        public async Task<IEnumerable<SearchResultItemViewModel>> SearchItems([FromBody]SearchRequestModel model)
        {
            SearchRequest mappedRequest = null;
            try
            {
                mappedRequest = Mapper.Map(model, new SearchRequest());
            }
            catch (Exception e)
            {
                throw;
            }
            var searchResult =
                await
                    _searchService.SearchWithFiltersAsync(
                        await _securityService.GetUserByIdAsync(User.Identity.GetUserId()), mappedRequest);

            var mappedResult =
                Mapper.Map<IEnumerable<SearchResultItem>, IEnumerable<SearchResultItemViewModel>>(searchResult);
            return mappedResult;
        }

        /// <summary>
        /// Searches items by query and filters
        /// </summary>
        /// <param name="itemsType">Type of finding item</param>
        /// <param name="orderBy">Sorting field</param>
        /// <param name="searchText">Search pattern</param>
        /// <param name="filters">Advanced filters</param>
        /// <returns>Found items</returns>
        [HttpGet]
        public async Task<IEnumerable<SearchResultItemViewModel>> SearchItems(string itemsType = "All", string orderBy = "-CreationDate", string searchText = "",
            SearchRequestFilterUrlParameters filters = null)
        {
            SearchRequestModel searchRequestModel;
            try
            {
                searchRequestModel = await ConvertUrlParametersToSearchModelAsync(itemsType, searchText, orderBy, filters);
            }
            catch (Exception e)
            {
                throw;
            }
            var mappedRequest = Mapper.Map(searchRequestModel, new SearchRequest());
            IEnumerable<SearchResultItem> searchResult;
            try
            {
                searchResult =
                    await
                        _searchService.SearchWithFiltersAsync(
                            await _securityService.GetUserByIdAsync(User.Identity.GetUserId()), mappedRequest);
            }
            catch (Exception e)
            {
                throw;
            }

            var mappedResult =
                Mapper.Map<IEnumerable<SearchResultItem>, IEnumerable<SearchResultItemViewModel>>(searchResult);
            return mappedResult;
        }

        /// <summary>
        /// Returns available filters for search
        /// </summary>
        /// <param name="itemsType">Filter type</param>
        /// <returns>Found filters</returns>
        [HttpGet]
        public async Task<IEnumerable<SearchRequestComplexFilterModel>> GetFilters(string itemsType = "All")
        {
            var serchResultItemType = (SearchResultItemType) Enum.Parse(typeof (SearchResultItemType), itemsType);

            IEnumerable<SearchRequestComplexFilterModel> mappedParameters = null;

            try
            {
                var parameters = await _searchService.GetFilterParametersAsync(serchResultItemType, true);

                mappedParameters =
                    Mapper.Map<IEnumerable<SearchRequestComplexFilter>, IEnumerable<SearchRequestComplexFilterModel>>(
                        parameters);
            }
            catch (Exception ex)
            {
                throw;
            }


            ////Fill additional related filter models
            //foreach(var filter in parameters)
            //{
            //    if(filter.RelatedFilters.Count > 0)
            //    {
            //        var matchedMappedFilter = mappedParameters.FirstOrDefault(f => f.Name == filter.Name);
            //        foreach(var relatedFilter in filter.RelatedFilters)
            //        {
            //            matchedMappedFilter.RelatedFilterModels.Add(Mapper.Map<SearchRequestFilterModel>(relatedFilter));
            //        }
            //    }
            //}
            return mappedParameters;
        }

        /// <summary>
        /// Return data for select in filter definition
        /// </summary>
        /// <param name="filter">Filter data</param>
        /// <returns>Selectable data for filter</returns>
        [HttpPost]
        public async Task<IEnumerable<SearchFilterSelectionNamedModel>> GetSelectValuesForFilter(SearchRequestFilterModel filter)
        {
            var searchRequestFilter = Mapper.Map(filter, new SearchRequestFilterItem());

            if (searchRequestFilter.Description == "InteractionPartnerOne")
                searchRequestFilter.Description = searchRequestFilter.Description.Replace("InteractionPartnerOne", "InteractionPartner");
            else if (searchRequestFilter.Description == "InteractionPartnerTwo")
                searchRequestFilter.Description = searchRequestFilter.Description.Replace("InteractionPartnerTwo", "InteractionPartner");

            IEnumerable<VocabularyValue> result = null;
            if (searchRequestFilter.ValueType == "AccessModes")
            {
                var resultList = new List<VocabularyValue>();
                resultList.Add(new VocabularyValue()
                {
                    Value = AccessModes.Explicit.ToString(),
                    Description = AccessModes.Explicit.ToString()
                });
                resultList.Add(new VocabularyValue()
                {
                    Value = AccessModes.Internal.ToString(),
                    Description = AccessModes.Internal.ToString()
                });
                resultList.Add(new VocabularyValue()
                {
                    Value = AccessModes.Private.ToString(),
                    Description = AccessModes.Private.ToString()
                });
                resultList.Add(new VocabularyValue()
                {
                    Value = AccessModes.Public.ToString(),
                    Description = AccessModes.Public.ToString()
                });
                result = resultList;
            }
            else
            {
                result =
                    await _searchService.GetSelectValuesAsync(searchRequestFilter,
                        System.Web.HttpContext.Current?.User?.Identity?.GetUserId());
            }
            return Mapper.Map<IEnumerable<VocabularyValue>, IEnumerable<SearchFilterSelectionNamedModel>>(result);
        }

        /// <summary>
        /// Searches data for autocomplete field
        /// </summary>
        /// <param name="q">Searched data</param>
        /// <returns>Found data</returns>
        [HttpGet]
        public async Task<IEnumerable<QuickSearchResultItemModel>> SearchForAutocomplete(string q)
        {
            var result = await _searchService.SearchForAutocompleteAsync(q, System.Web.HttpContext.Current.User.Identity.GetUserId());

            return Mapper.Map<IEnumerable<QuickSearchResultItemModel>>(result);
        }


        private async Task<SearchRequestModel> ConvertUrlParametersToSearchModelAsync(string itemsType, string searchText, string orderBy, SearchRequestFilterUrlParameters filters)
        {
            var searchRequestModel = new SearchRequestModel();
            searchRequestModel.SearchItemsType = itemsType;
            searchRequestModel.SearchText = searchText;
            searchRequestModel.OrderBy = orderBy;

            var serchResultItemType = (SearchResultItemType)Enum.Parse(typeof(SearchResultItemType), itemsType);
            var parameters =  (await _searchService.GetFilterParametersAsync(serchResultItemType, false)).ToList();

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var foundFilter = parameters.SelectMany(s => s.Items).Where(f => f.Name == filter.Key).FirstOrDefault();

                    ((List<SearchRequestFilterModel>)searchRequestModel.Filters).Add(new SearchRequestFilterModel()
                    {
                        FilterType = foundFilter.FilterType.ToString(),
                        Name = filter.Key,
                        Value = filter.Value
                    });
                }
            }

            return searchRequestModel;
        }
    }
}