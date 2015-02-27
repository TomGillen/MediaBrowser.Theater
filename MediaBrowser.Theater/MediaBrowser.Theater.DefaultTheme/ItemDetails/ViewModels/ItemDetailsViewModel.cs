using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Library;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemDetailsViewModel
        : BaseViewModel, IHasRootPresentationOptions, IItemDetailsViewModel
    {
        private readonly BaseItemDto _item;
        private readonly IEnumerable<IItemDetailSection> _sections;

        public BaseItemDto Item
        {
            get { return _item; }
        }

        public ItemDetailsViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IEnumerable<IItemDetailSection> sections)
        {
            _item = item;
            _sections = sections.ToList();

            PresentationOptions = new RootPresentationOptions {
                ShowMediaBrowserLogo = false,
                BackgroundMedia = new ItemArtworkViewModel(item, connectionManager, imageManager) {
                    DesiredImageWidth = 1920,
                    DesiredImageHeight = 1280,
                    PreferredImageTypes = new[] { ImageType.Backdrop }
                }
                //Title = item.GetDisplayName(new DisplayNameFormat(true, false))
            };
        }

        public Func<object, object> TitleSelector
        {
            get { return item => ((IItemDetailSection)item).Title; }
        }

        public IEnumerable<IItemDetailSection> Sections
        {
            get { return _sections; }
        }

        public RootPresentationOptions PresentationOptions { get; private set; }
    }
}