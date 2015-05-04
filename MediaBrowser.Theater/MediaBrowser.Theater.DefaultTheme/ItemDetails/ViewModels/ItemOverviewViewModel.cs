﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.Commands.ItemCommands;
using MediaBrowser.Theater.Api.Session;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Core.ViewModels;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.DefaultTheme.ItemCommands;
using MediaBrowser.Theater.Playback;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.ItemDetails.ViewModels
{
    public class ItemOverviewViewModel
        : BaseViewModel, IItemDetailSection, IKnownSize
    {
        private readonly BaseItemDto _item;
        private readonly IPresenter _presenter;
        private ItemArtworkViewModel _posterArtwork;
        private CroppedBitmap _primaryButtonImage;
        private IItemCommand _primaryCommand;
        private CroppedBitmap _secondaryButtonImage;
        private IItemCommand _secondaryCommand;
        private ICommand _showCommands;
        private CroppedBitmap _toggleDislikeButtonImage;
        private IItemCommand _toggleDislikeCommand;
        private CroppedBitmap _toggleFavoriteButtonImage;
        private IItemCommand _toggleFavoriteCommand;
        private CroppedBitmap _toggleLikeButtonImage;
        private IItemCommand _toggleLikeCommand;
        private CroppedBitmap _toggleWatchedButtonImage;

        public ItemOverviewViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, ISessionManager sessionManager, IItemCommandsManager commands, IPresenter presenter)
        {
            _item = item;
            _presenter = presenter;

            Info = new ItemInfoViewModel(item) {
                ShowDisplayName = true,
                ShowUserRatings = false,
                ShowParentText = item.IsType("Season") || item.IsType("Episode") || item.IsType("Album") || item.IsType("Track")
            };

            if (item.ImageTags.ContainsKey(ImageType.Primary)) {
                PosterArtwork = new ItemArtworkViewModel(item, connectionManager, imageManager) {
                    DesiredImageHeight = PosterHeight,
                    PreferredImageTypes = new[] { ImageType.Primary }
                };

                PosterArtwork.PropertyChanged += (s, e) => {
                    if (e.PropertyName == "Size") {
                        if (PosterArtwork.Size.Width > PosterHeight*0.8) {
                            // hide artwork if it is too wide
                            PosterArtwork = null;
                        }

                        OnPropertyChanged("Size");
                    }
                };
            }

            EnableButtons = !item.IsType("person");

            BackgroundArtwork = new ItemArtworkViewModel(item, connectionManager, imageManager) {
                DesiredImageWidth = DetailsWidth,
                PreferredImageTypes = new[] { ImageType.Backdrop, ImageType.Screenshot }
            };

            PlayButton = new PlayButtonViewModel(item, playbackManager, connectionManager, imageManager, sessionManager, item.BackdropImageTags.Count > 1 ? 1 : (int?) null);

            SetupCommands(item, commands);
            SetupButtonImage(item, connectionManager, imageManager);
        }

        public ItemArtworkViewModel PosterArtwork
        {
            get { return _posterArtwork; }
            set
            {
                if (Equals(value, _posterArtwork)) {
                    return;
                }

                _posterArtwork = value;
                OnPropertyChanged();
            }
        }

        public ItemArtworkViewModel BackgroundArtwork { get; set; }
        public ItemInfoViewModel Info { get; set; }

        public bool EnableButtons { get; set; }

        public ICommand ShowCommands
        {
            get { return _showCommands; }
            private set
            {
                if (Equals(value, _showCommands)) {
                    return;
                }

                _showCommands = value;
                OnPropertyChanged();
            }
        }

        public IItemCommand PrimaryCommand
        {
            get { return _primaryCommand; }
            set
            {
                if (Equals(value, _primaryCommand)) {
                    return;
                }
                _primaryCommand = value;
                OnPropertyChanged();
                OnPropertyChanged("HasPrimaryCommand");
            }
        }

        public IItemCommand SecondaryCommand
        {
            get { return _secondaryCommand; }
            set
            {
                if (Equals(value, _secondaryCommand)) {
                    return;
                }
                _secondaryCommand = value;
                OnPropertyChanged();
                OnPropertyChanged("HasSecondaryCommand");
            }
        }

        public IItemCommand ToggleFavoriteCommand
        {
            get { return _toggleFavoriteCommand; }
            set
            {
                if (Equals(value, _toggleFavoriteCommand)) {
                    return;
                }

                _toggleFavoriteCommand = value;
                OnPropertyChanged();
            }
        }

        public IItemCommand ToggleLikeCommand
        {
            get { return _toggleLikeCommand; }
            set
            {
                if (Equals(value, _toggleLikeCommand)) {
                    return;
                }

                _toggleLikeCommand = value;
                OnPropertyChanged();
            }
        }

        public IItemCommand ToggleDislikeCommand
        {
            get { return _toggleDislikeCommand; }
            set
            {
                if (Equals(value, _toggleDislikeCommand)) {
                    return;
                }

                _toggleDislikeCommand = value;
                OnPropertyChanged();
            }
        }

        public bool HasPrimaryCommand
        {
            get { return PrimaryCommand != null; }
        }

        public bool HasSecondaryCommand
        {
            get { return SecondaryCommand != null; }
        }

        public CroppedBitmap PrimaryButtonImage
        {
            get { return _primaryButtonImage; }
            set
            {
                if (Equals(value, _primaryButtonImage)) {
                    return;
                }
                _primaryButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap SecondaryButtonImage
        {
            get { return _secondaryButtonImage; }
            set
            {
                if (Equals(value, _secondaryButtonImage)) {
                    return;
                }
                _secondaryButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleFavoriteButtonImage
        {
            get { return _toggleFavoriteButtonImage; }
            set
            {
                if (Equals(value, _toggleFavoriteButtonImage)) {
                    return;
                }
                _toggleFavoriteButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleLikeButtonImage
        {
            get { return _toggleLikeButtonImage; }
            set
            {
                if (Equals(value, _toggleLikeButtonImage)) {
                    return;
                }
                _toggleLikeButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleDislikeButtonImage
        {
            get { return _toggleDislikeButtonImage; }
            set
            {
                if (Equals(value, _toggleDislikeButtonImage)) {
                    return;
                }
                _toggleDislikeButtonImage = value;
                OnPropertyChanged();
            }
        }

        public CroppedBitmap ToggleWatchedButtonImage
        {
            get { return _toggleWatchedButtonImage; }
            set
            {
                if (Equals(value, _toggleWatchedButtonImage)) {
                    return;
                }
                _toggleWatchedButtonImage = value;
                OnPropertyChanged();
            }
        }

        public bool ShowInfo
        {
            get { return (!_item.IsFolder && _item.Type != "Person") || !string.IsNullOrEmpty(_item.Overview); }
        }

        public double PosterHeight
        {
            get { return HomeViewModel.TileHeight*3 + HomeViewModel.TileMargin*4; }
        }

        public double DetailsWidth
        {
            get { return HomeViewModel.TileWidth*2 + HomeViewModel.TileMargin*2; }
        }

        public double DetailsHeight
        {
            get { return HomeViewModel.TileHeight*2 + HomeViewModel.TileMargin*2; }
        }

        public PlayButtonViewModel PlayButton { get; private set; }

        public int SortOrder
        {
            get { return 0; }
        }

        public string Title
        {
            get { return "MediaBrowser.Theater.DefaultTheme:Strings:DetailSection_OverviewHeader".Localize(); }
        }

        public Size Size
        {
            get
            {
//                if (ShowInfo)
//                    return new Size(800 + 20 + 250, 700);

                double artWidth = Math.Min(1200, PosterArtwork != null ? PosterArtwork.ActualWidth : 0);
//                return new Size(artWidth + 20 + 250, 700);
                return new Size(artWidth + DetailsWidth + 4 + 50, PosterHeight);
            }
        }

        private async void SetupCommands(BaseItemDto item, IItemCommandsManager commandManager)
        {
            List<IItemCommand> commands = (await commandManager.GetCommands(item)).ToList();
            // pick out play/resume, like, dislike, favorite and watched - bind to static buttons
            // sort rest by sort order
            // pick top 2, set as primary and secondary actions
            // swap empty action slots for images
            // if more than 2 actions, replace watched button with overflow

            //var playCommand = commands.FirstOrDefault(c => c is PlayItemCommand);
            ToggleLikeCommand = commands.FirstOrDefault(c => c is LikeItemCommand);
            ToggleDislikeCommand = commands.FirstOrDefault(c => c is DislikeItemCommand);
            ToggleFavoriteCommand = commands.FirstOrDefault(c => c is FavoriteItemCommand);

            List<IItemCommand> extraActions = commands.Where(c => !(c is PlayItemCommand) &&
                                                                  !(c is LikeItemCommand) &&
                                                                  !(c is DislikeItemCommand) &&
                                                                  !(c is FavoriteItemCommand) &&
                                                                  !(c is WatchedItemCommand))
                                                      .ToList();

            if (extraActions.Count > 0) {
                PrimaryCommand = extraActions[0];
            }

            if (extraActions.Count > 1) {
                SecondaryCommand = extraActions[1];
            }

            ShowCommands = new RelayCommand(o => _presenter.ShowPopup(new CommandsPopupViewModel(commands), true, false));
        }

        private async void SetupButtonImage(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager)
        {
            if (item.BackdropCount < 3) {
                return;
            }

            var width = (int) (DetailsWidth/2 - 2);
            var height = (int) HomeViewModel.TileHeight;

            IApiClient api = connectionManager.GetApiClient(item);
            string url = api.GetImageUrl(item, new ImageOptions {
                ImageType = ImageType.Backdrop,
                ImageIndex = 2,
                Width = width,
                Height = height
            });

            BitmapImage bitmap = await imageManager.GetRemoteBitmapAsync(url);
            PrimaryButtonImage = new CroppedBitmap(bitmap, new Int32Rect(0, 0, width/2 - 2, height*2/3 - 2));
            SecondaryButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width/2 + 2, 0, width/2 - 2, height*2/3 - 2));
            ToggleFavoriteButtonImage = new CroppedBitmap(bitmap, new Int32Rect(0, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleLikeButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleDislikeButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width*2/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
            ToggleWatchedButtonImage = new CroppedBitmap(bitmap, new Int32Rect(width*3/4 + 2, height*2/3 + 2, width/4 - 4, height/3 - 2));
        }
    }

    public class ItemOverviewSectionGenerator
        : IItemDetailSectionGenerator
    {
        private readonly IItemCommandsManager _commands;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;
        private readonly IPlaybackManager _playbackManager;
        private readonly IPresenter _presenter;
        private readonly ISessionManager _sessionManager;

        public ItemOverviewSectionGenerator(IConnectionManager connectionManager, IImageManager imageManager, IPlaybackManager playbackManager, ISessionManager sessionManager, IItemCommandsManager commands, IPresenter presenter)
        {
            _connectionManager = connectionManager;
            _imageManager = imageManager;
            _playbackManager = playbackManager;
            _sessionManager = sessionManager;
            _commands = commands;
            _presenter = presenter;
        }

        public bool HasSection(BaseItemDto item)
        {
            return item != null;
        }

        public Task<IEnumerable<IItemDetailSection>> GetSections(BaseItemDto item)
        {
            IItemDetailSection section = new ItemOverviewViewModel(item, _connectionManager, _imageManager, _playbackManager, _sessionManager, _commands, _presenter);
            return Task.FromResult<IEnumerable<IItemDetailSection>>(new[] { section });
        }
    }
}