using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModels
{
    public class FilterOptionViewModelBinder : DefaultModelBinder
    {
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;
        private readonly LanguageResolver _languageResolver;

        public FilterOptionViewModelBinder(IContentLoader contentLoader, 
            LocalizationService localizationService,
            LanguageResolver languageResolver)
        {
            _contentLoader = contentLoader;
            _localizationService = localizationService;
            _languageResolver = languageResolver;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            bindingContext.ModelName = "FilterOption";
            var model = (FilterOptionViewModel)base.BindModel(controllerContext, bindingContext);
            if (model == null)
            {
                return model;
            }

            var contentLink = controllerContext.RequestContext.GetContentLink();
            IContent content = null;
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                content = _contentLoader.Get<IContent>(contentLink);
            }

            var query = controllerContext.HttpContext.Request.QueryString["q"];
            var sort = controllerContext.HttpContext.Request.QueryString["sort"];
            var facets = controllerContext.HttpContext.Request.QueryString["facets"];
            SetupModel(model, query, sort, facets, content);
            return model;
        }

        protected virtual void SetupModel(FilterOptionViewModel model, string q, string sort, string facets, IContent content)
        {
            EnsurePage(model);
            EnsureQ(model, q);
            EnsureSort(model, sort);
            EnsureFacets(model, facets, content);
        }

        protected virtual void EnsurePage(FilterOptionViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }
        }

        protected virtual void EnsureQ(FilterOptionViewModel model, string q)
        {
            if (string.IsNullOrEmpty(model.Q))
            {
                model.Q = q;
            }
        }

        protected virtual void EnsureSort(FilterOptionViewModel model, string sort)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = sort;
            }
        }

        protected virtual void EnsureFacets(FilterOptionViewModel model, string facets, IContent content)
        {
            if (model.FacetGroups == null)
            {
                model.FacetGroups = CreateFacetGroups(facets, content);
            }
        }

        private List<FacetGroupOption> CreateFacetGroups(string facets, IContent content)
        {
            var facetGroups = new List<FacetGroupOption>();
            if (string.IsNullOrEmpty(facets))
            {
                return facetGroups;
            }
            foreach (var rawFacet in facets.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var parsedFacet = rawFacet.Split(new[] {':'}, System.StringSplitOptions.RemoveEmptyEntries);

                var facetGroupName = parsedFacet[0];
                var facetKey = parsedFacet[1];
               
                var facetGroup = new FacetGroupOption();
                facetGroup.GroupName = facetGroupName;
                facetGroup.GroupFieldName = facetGroupName;
                facetGroup.Facets = new List<FacetOption>();
                facetGroup.Facets.Add(new FacetOption()
                {
                    Name = facetKey,
                    Key = facetKey,
                    Selected = true
                });

                facetGroups.Add(facetGroup);
            }

            return facetGroups;
        }
    }
}