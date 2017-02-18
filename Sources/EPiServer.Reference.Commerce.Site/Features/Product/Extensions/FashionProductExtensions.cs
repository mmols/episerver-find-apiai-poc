using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Shell.Web;

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
                //Adding this trim in since Quicksilver uses -w for categories which are shared across genders
                return category.Code.TrimEnd("-w");
            }

            return String.Empty;
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