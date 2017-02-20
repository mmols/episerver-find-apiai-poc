using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.Find.Cms;
using EPiServer.Find.Commerce;
using EPiServer.Find.Framework;
using EPiServer.Framework.Localization;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.Util;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Search;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Services
{
    public class FindSearchService : SearchService
    {
        private readonly SearchFacade _search;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly UrlResolver _urlResolver;
        private readonly LanguageResolver _languageResolver;
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;

        public FindSearchService(ICurrentMarket currentMarket, ICurrencyService currencyService, UrlResolver urlResolver,
            SearchFacade search, LanguageResolver languageResolver, IContentLoader contentLoader,
            LocalizationService localizationService)
            : base(
                currentMarket, currencyService, urlResolver, search, languageResolver, contentLoader,
                localizationService)
        {
            _search = search;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _urlResolver = urlResolver;
            _languageResolver = languageResolver;
            _contentLoader = contentLoader;
            _localizationService = localizationService;
        }

        private static readonly int _defaultPageSize = 18;

        public override CustomSearchResult Search(IContent currentContent, FilterOptionViewModel filterOptions)
        {
            var returnResult = new CustomSearchResult();
            var searchClient = SearchClient.Instance.Search<FashionProduct>();

            var pageSize = filterOptions.PageSize > 0 ? filterOptions.PageSize : _defaultPageSize;
            var query = searchClient.For(filterOptions.Q)
                .InAllField()
                .TermsFacetFor(x => x.Gender())
                .TermsFacetFor(x => x.CategoryCode())
                .TermsFacetFor(x => x.Brand)
                .TermsFacetFor(x => x.AvailableColors)
                .PublishedInCurrentLanguage()
                .Skip(pageSize * (filterOptions.Page - 1))
                .Take(pageSize);

            if (currentContent != null && currentContent.GetOriginalType() != typeof(SearchPage))
            {
                query = query.Filter(x => x.Ancestors().Match(currentContent.ContentLink.ToReferenceWithoutVersion().ToString()));
            }

            //TODO: Wire this up to be dynamic
            foreach (var filter in filterOptions.FacetGroups)
            {
                var filterValue = filter.Facets.FirstOrDefault().Key;
                switch (filter.GroupFieldName)
                {
                    case "AvailableColors":
                        query = query.Filter(x => x.AvailableColors.MatchCaseInsensitive(filterValue));
                        break;
                    case "Brand":
                        query = query.Filter(x => x.Brand.MatchCaseInsensitive(filterValue));
                        break;
                    case "Gender":
                        query = query.Filter(x => x.Gender().MatchCaseInsensitive(filterValue));
                        break;
                    case "CategoryCode":
                        query = query.Filter(x => x.CategoryCode().MatchCaseInsensitive(filterValue));
                        break;
                }
            }


            var results = query.GetContentResult();
            var returnFacets = new List<FacetGroupOption>();
            foreach (var findFacet in results.Facets)
            {
                var termFacet = findFacet as TermsFacet;
                if (termFacet != null)
                {
                    var facetGroupOption = new FacetGroupOption()
                    {
                        GroupFieldName = termFacet.Name,
                        GroupName = GetFriendlyFacetName(termFacet.Name),
                        Facets = termFacet.Terms.Select(x => new FacetOption()
                        {
                            Name = x.Term,
                            Count = x.Count,
                            Key = x.Term,
                            Selected = IsSelectedFacet(termFacet.Name, x.Term, filterOptions)
                        }).ToList()
                    };

                    returnFacets.Add(facetGroupOption);
                }
            }

            returnResult.FacetGroups = returnFacets;
            returnResult.ProductViewModels = CreateProductViewModels(results);
            returnResult.TotalCount = results.SearchResult.TotalMatching;
            return returnResult;
        }

        private string GetFriendlyFacetName(string termFacetName)
        {
            switch (termFacetName)
            {
                case "CategoryCode":
                    return "Category";
                case "AvailableColors":
                    return "Color";
                default:
                    return termFacetName;               
            }
        }

        private bool IsSelectedFacet(string facetGroup, string term, FilterOptionViewModel filterOptions)
        {
            var filterOption = filterOptions.FacetGroups.FirstOrDefault(x => x.GroupFieldName == facetGroup);
            if (filterOption != null)
            {
                var facetValue = filterOption.Facets.FirstOrDefault();
                if (facetValue.Key.Equals(term, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<ProductViewModel> CreateProductViewModels(IContentResult<FashionProduct> searchResults)
        {
            return searchResults.Select(product => new ProductViewModel
            {
                Brand = product.Brand,
                Code = product.Code,
                DisplayName = product.DisplayName,
                ImageUrl = product.DefaultImageUrl(),
                PlacedPrice = product.DefaultPrice(),
                DiscountedPrice = product.DiscountPrice(),
                Url = _urlResolver.GetUrl(product.ContentLink),
                IsAvailable = true
            });
        }
    }
}