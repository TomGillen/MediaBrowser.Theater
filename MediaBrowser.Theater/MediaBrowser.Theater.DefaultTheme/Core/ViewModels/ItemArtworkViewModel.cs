﻿using System;
using System.Linq;
using System.Windows;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.UserInterface;
using MediaBrowser.Theater.DefaultTheme.Home.ViewModels;
using MediaBrowser.Theater.Presentation;
using MediaBrowser.Theater.Presentation.Controls;
using MediaBrowser.Theater.Presentation.ViewModels;

namespace MediaBrowser.Theater.DefaultTheme.Core.ViewModels
{
    public class ItemArtworkViewModel
        : BaseViewModel, IKnownSize
    {
        private BaseItemDto _item;
        private readonly IConnectionManager _connectionManager;
        private double? _desiredImageHeight;
        private double? _desiredImageWidth;
        private bool _downloadImagesAtExactSize;
        private bool _downloadPrimaryImageAtExactSize;
        private bool _enableServerImageEnhancers;
        private bool _enforcePreferredImageAspectRatio;
        private ImageViewerViewModel _image;
        private ImageType[] _preferredImageTypes;
        private bool _imageInvalid;
        private bool _enableWatchedIndicator;

        public ItemArtworkViewModel(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager)
        {
            _item = item;
            _connectionManager = connectionManager;

            _image = new ImageViewerViewModel(imageManager, Enumerable.Empty<ImageViewerImage>());
            _image.PropertyChanged += (senger, args) => {
                if (args.PropertyName == "CurrentImage") {
                    OnPropertyChanged("ActualWidth");
                }

                if (args.PropertyName == "ImageHeight") {
                    OnPropertyChanged("ActualHeight");
                }

                if (args.PropertyName == "CurrentImage") {
                    OnPropertyChanged("Size");
                }
            };

            PreferredImageTypes = new[] { ImageType.Primary };
            _imageInvalid = true;
        }

        public BaseItemDto Item
        {
            get { return _item; }
            set
            {
                if (Equals(_item, value))
                {
                    return;
                }

                _item = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public ImageViewerViewModel Image
        {
            get
            {
                if (_imageInvalid) {
                    DownloadImage();
                }

                return _image;
            }
            private set
            {
                if (Equals(value, _image)) {
                    return;
                }
                _image = value;
                OnPropertyChanged();
            }
        }

        public bool EnableWatchedIndicator
        {
            get { return _enableWatchedIndicator; }
            set
            {
                _enableWatchedIndicator = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public ImageType[] PreferredImageTypes
        {
            get { return _preferredImageTypes; }
            set
            {
                _preferredImageTypes = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool EnforcePreferredImageAspectRatio
        {
            get { return _enforcePreferredImageAspectRatio; }
            set
            {
                if (value.Equals(_enforcePreferredImageAspectRatio)) {
                    return;
                }
                _enforcePreferredImageAspectRatio = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double? DesiredImageWidth
        {
            get { return _desiredImageWidth; }
            set
            {
                if (Equals(_desiredImageWidth, value)) {
                    return;
                }

                _desiredImageWidth = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double? DesiredImageHeight
        {
            get { return _desiredImageHeight; }
            set
            {
                if (Equals(_desiredImageHeight, value)) {
                    return;
                }

                _desiredImageHeight = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public double ActualWidth
        {
            get
            {
                if (DesiredImageWidth != null) {
                    return (double) DesiredImageWidth;
                }

                if (DesiredImageHeight != null) {
                    var itemType = _item != null ? _item.Type : null;
                    double aspectRatio = EnforcePreferredImageAspectRatio || (int) Image.ImageHeight == 0 ? PreferredImageTypes.First().GetAspectRatio(itemType) : Image.ImageWidth/Image.ImageHeight;
                    return (double) DesiredImageHeight*aspectRatio;
                }

                return Image.ImageWidth;
            }
        }

        public double ActualHeight
        {
            get
            {
                if (DesiredImageHeight != null) {
                    return (double) DesiredImageHeight;
                }

                if (DesiredImageWidth != null) {
                    var itemType = _item != null ? _item.Type : null;
                    double aspectRatio = EnforcePreferredImageAspectRatio || (int) Image.ImageWidth == 0 ? PreferredImageTypes.First().GetAspectRatio(itemType) : Image.ImageWidth/Image.ImageHeight;
                    return (double) DesiredImageWidth/aspectRatio;
                }

                return Image.ImageHeight;
            }
        }

        public bool DownloadImagesAtExactSize
        {
            get { return _downloadImagesAtExactSize; }
            set
            {
                if (Equals(_downloadImagesAtExactSize, value)) {
                    return;
                }

                _downloadImagesAtExactSize = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool DownloadPrimaryImageAtExactSize
        {
            get { return _downloadPrimaryImageAtExactSize; }
            set
            {
                if (Equals(_downloadPrimaryImageAtExactSize, value)) {
                    return;
                }

                _downloadPrimaryImageAtExactSize = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public bool EnableServerImageEnhancers
        {
            get { return _enableServerImageEnhancers; }
            set
            {
                if (Equals(_enableServerImageEnhancers, value)) {
                    return;
                }

                _enableServerImageEnhancers = value;
                OnPropertyChanged();
                InvalidateImage();
            }
        }

        public Size Size
        {
            get { return new Size(ActualWidth + 2*HomeViewModel.TileMargin, ActualHeight + 2*HomeViewModel.TileMargin); }
        }

        private void InvalidateImage()
        {
            _imageInvalid = true;
            OnPropertyChanged("Image");
        }

        /// <summary>
        ///     Gets an image url that can be used to download an image from the api
        /// </summary>
        /// <param name="imageType">The type of image requested</param>
        /// <param name="imageIndex">
        ///     The image index, if there are multiple. Currently only applies to backdrops. Supply null or 0
        ///     for first backdrop.
        /// </param>
        /// <returns>System.String.</returns>
        private string GetImageUrl(ImageType imageType, int? imageIndex = null)
        {
            var imageOptions = new ImageOptions {
                ImageType = imageType,
                ImageIndex = imageIndex,
                Height = DesiredImageHeight != null && !double.IsPositiveInfinity((double) DesiredImageHeight) ? (int?) Convert.ToInt32(DesiredImageHeight) : null,
                EnableImageEnhancers = EnableServerImageEnhancers,
                AddPlayedIndicator = EnableWatchedIndicator && Item.UserData != null && Item.UserData.Played,
                UnPlayedCount = (EnableWatchedIndicator && Item.UserData != null) ? Item.UserData.UnplayedItemCount : null
            };

            if (EnforcePreferredImageAspectRatio
                || (imageType == ImageType.Primary && DownloadPrimaryImageAtExactSize)
                || (imageType != ImageType.Primary && DownloadImagesAtExactSize)) {
                if (DesiredImageWidth == null) {
                    imageOptions.Width = (int) ActualWidth;
                } else {
                    imageOptions.Width = DesiredImageWidth != null && !double.IsPositiveInfinity((double) DesiredImageWidth) ? (int?) Convert.ToInt32(DesiredImageWidth) : null;
                }
            }

            var apiClient = _connectionManager.GetApiClient(_item);

            if (imageType == ImageType.Thumb) {
                return apiClient.GetThumbImageUrl(_item, imageOptions);
            }

            return apiClient.GetImageUrl(_item, imageOptions);
        }

        public void DownloadImage()
        {
            ImageType[] preferredImageTypes = PreferredImageTypes;
            BaseItemDto item = _item;

            _imageInvalid = false;

            if (item != null) {
                foreach (ImageType imageType in preferredImageTypes) {
                    if (imageType == ImageType.Backdrop) {
                        if (item.BackdropCount == 0) {
                            continue;
                        }
                    } else if (imageType == ImageType.Thumb) {
                        if (!item.ImageTags.ContainsKey(imageType) && string.IsNullOrEmpty(item.ParentThumbImageTag) && string.IsNullOrEmpty(item.SeriesThumbImageTag)) {
                            continue;
                        }
                    } else {
                        if (!item.ImageTags.ContainsKey(imageType)) {
                            continue;
                        }
                    }

                    string url = GetImageUrl(imageType);
                    Image.Images.Clear();
                    Image.Images.Add(new ImageViewerImage { Url = url });
                    Image.StartRotating();

                    return;
                }
            }

            Image.Images.Clear();
            Image.StartRotating();
        }
    }
}