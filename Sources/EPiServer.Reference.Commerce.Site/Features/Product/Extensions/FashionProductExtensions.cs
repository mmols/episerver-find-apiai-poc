using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Commerce;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.Shell.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Pricing;
using Mediachase.Search.Extensions;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Extensions
{
    public static class FashionProductExtensions
    {
        public static string Gender(this FashionProduct fashionProduct)
        {
            var topCategory = GetTopCategory(fashionProduct);
            return topCategory.DisplayName;
        }

        public static string CategoryCode(this FashionProduct fashionProduct)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var category = contentLoader.Get<NodeContent>(fashionProduct.ParentLink);
            if (category != null)
            {
                TextInfo textInfo = fashionProduct.Language.TextInfo;
                //Adding this trim in since Quicksilver uses -w for categories which are shared across genders
                return textInfo.ToTitleCase(category.Code.TrimEnd("-w"));
            }

            return String.Empty;
        }

        public static Money DefaultPrice(this ProductContent productContent)
        {

            var pricingLoader = ServiceLocator.Current.GetInstance<ReadOnlyPricingLoader>();
            var relationRepository = ServiceLocator.Current.GetInstance<IRelationRepository>();

            var maxPrice = new Price();

            var variationLinks = productContent.GetVariants(relationRepository);
            foreach (var variationLink in variationLinks)
            {
                var defaultPrice = pricingLoader.GetDefaultPrice(variationLink);
                if (defaultPrice.UnitPrice.Amount > maxPrice.UnitPrice.Amount)
                {
                    maxPrice = defaultPrice;
                }
            }

            return maxPrice.UnitPrice;
        }

        public static Money DiscountPrice(this ProductContent productContent)
        {
            var pricingLoader = ServiceLocator.Current.GetInstance<ReadOnlyPricingLoader>();
            var promotionService = ServiceLocator.Current.GetInstance<IPromotionService>();
            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var appContext = ServiceLocator.Current.GetInstance<AppContextFacade>();
            var relationRepository = ServiceLocator.Current.GetInstance<IRelationRepository>();

            var maxPrice = new Price();
            ContentReference maxPriceVariant = null;
            var variationLinks = productContent.GetVariants(relationRepository);

            foreach (var variationLink in variationLinks)
            {
                var defaultPrice = pricingLoader.GetDefaultPrice(variationLink);
                if (defaultPrice.UnitPrice.Amount > maxPrice.UnitPrice.Amount)
                {
                    maxPrice = defaultPrice;
                    maxPriceVariant = variationLink;
                }
            }

            return promotionService.GetDiscountPrice(new CatalogKey(appContext.ApplicationId, referenceConverter.GetCode(maxPriceVariant)), maxPrice.MarketId, maxPrice.UnitPrice.Currency).UnitPrice;
        }

        private static NodeContent GetTopCategory(CatalogContentBase nodeContent)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var category = contentLoader.Get<CatalogContentBase>(nodeContent.ParentLink);
            if (category.ContentType == CatalogContentType.Catalog)
            {
                return (NodeContent)nodeContent;
            }
            return GetTopCategory(category);
        }


    }
}