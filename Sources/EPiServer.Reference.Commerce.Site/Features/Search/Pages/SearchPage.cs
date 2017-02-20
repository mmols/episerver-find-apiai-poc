using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Pages
{
    [ContentType(DisplayName = "Search page", GUID = "6e0c84de-bd17-43ee-9019-04f08c7fcf8d", Description = "", AvailableInEditMode = false)]
    public class SearchPage : PageData
    {
        [Display(
            Name = "Enable AI Search?",
            Description = "Enables or disables api.ai pre-filtering on the Search Page",
            GroupName = SystemTabNames.Settings,
            Order = 10)]
        public virtual bool EnableAiSearch { get; set; }
    }
}