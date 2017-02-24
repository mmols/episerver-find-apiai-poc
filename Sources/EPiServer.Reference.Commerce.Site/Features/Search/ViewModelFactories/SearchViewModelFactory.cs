using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using Mediachase.Search;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using ApiAiSDK;
using ApiAiSDK.Model;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using System.Configuration;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories
{
    public class SearchViewModelFactory
    {
        private readonly ISearchService _searchService;
        private readonly LocalizationService _localizationService;
        private readonly IContentLoader _contentLoader;

        public SearchViewModelFactory(LocalizationService localizationService, ISearchService searchService)
        {
            _searchService = searchService;
            _localizationService = localizationService;
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        }

        public virtual SearchViewModel<T> Create<T>(T currentContent, FilterOptionViewModel viewModel) where T : IContent
        {
            if (viewModel.Q != null && (viewModel.Q.StartsWith("*") || viewModel.Q.StartsWith("?")))
            {
                return new SearchViewModel<T>
                {
                    CurrentContent = currentContent,
                    FilterOption = viewModel,
                    HasError = true,
                    ErrorMessage = _localizationService.GetString("/Search/BadFirstCharacter")
                };
            }

            if (!String.IsNullOrEmpty(viewModel.Q) && AISearchIsEnabled())
            {
                
                var config = new AIConfiguration(ConfigurationManager.AppSettings["api-ai-key"], SupportedLanguage.English);
                var apiAi = new ApiAi(config);
                var response = apiAi.TextRequest(viewModel.Q);

                if (response.Result.HasParameters)
                {
                    viewModel.Q = String.Empty;
                    AddFacet(SearchFilterField.Color, response.Result.GetStringParameter(ApiAiEntity.Color), viewModel);
                    AddFacet(SearchFilterField.Brand, response.Result.GetStringParameter(ApiAiEntity.Brand), viewModel);
                    AddFacet(SearchFilterField.Gender, response.Result.GetStringParameter(ApiAiEntity.Gender), viewModel);
                    AddFacet(SearchFilterField.Category, response.Result.GetStringParameter(ApiAiEntity.Category), viewModel);
                }
            }

            var customSearchResult = _searchService.Search(currentContent, viewModel);

            viewModel.TotalCount = customSearchResult.TotalCount;
            viewModel.FacetGroups = customSearchResult.FacetGroups.ToList();

            viewModel.Sorting = _searchService.GetSortOrder().Select(x => new SelectListItem
            {
                Text = _localizationService.GetString("/Category/Sort/" + x.Name),
                Value = x.Name.ToString(),
                Selected = string.Equals(x.Name.ToString(), viewModel.Sort)
            });

            return new SearchViewModel<T>
            {
                CurrentContent = currentContent,
                ProductViewModels = customSearchResult.ProductViewModels,
                FilterOption = viewModel
            };
        }

        private bool AISearchIsEnabled()
        {
            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            var searchPage = _contentLoader.Get<SearchPage>(startPage.SearchPage);
            return searchPage.EnableAiSearch;
        }

        private FacetGroupOption AddFacet(string facetName, string facetValue, FilterOptionViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(facetValue))
            {
                if (viewModel.FacetGroups.FirstOrDefault(x => x.GroupFieldName == facetName) == null)
                {
                    var prefilledFacet = new Models.FacetGroupOption()
                    {
                        GroupFieldName = facetName,
                        GroupName = facetName,
                        Facets =
                            new System.Collections.Generic.List<Models.FacetOption>()
                            {
                                new Models.FacetOption() {Key = facetValue, Name = facetValue, Selected = true}
                            }
                    };
                    viewModel.FacetGroups.Add(prefilledFacet);
                }
            }

            return null;
        }
    }
}