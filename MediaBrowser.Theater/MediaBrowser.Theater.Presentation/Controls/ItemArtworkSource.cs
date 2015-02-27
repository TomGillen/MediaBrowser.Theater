using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Theater.Api.UserInterface;

namespace MediaBrowser.Theater.Presentation.Controls
{
    public class ItemArtworkSource
    {
        private readonly BaseItemDto _item;
        private readonly ImageType[] _imageType;
        private readonly IConnectionManager _connectionManager;
        private readonly IImageManager _imageManager;

        public ItemArtworkSource(BaseItemDto item, IConnectionManager connectionManager, IImageManager imageManager, ImageType[] imageType = null)
        {
            _item = item;
            _imageType = imageType;
            _connectionManager = connectionManager;
            _imageManager = imageManager;
        }

        public async Task<Image> DownloadImage(Size availableSize)
        {
            IEnumerable<ImageType> imageTypes = _imageType ?? CalculateImageType(availableSize);

            foreach (var type in imageTypes.Where(ImageTypePresent)) {
                var url = await GetImageUrl(type, availableSize).ConfigureAwait(false);
                if (url != null) {
                    var image = await _imageManager.GetRemoteImageAsync(url).ConfigureAwait(false);
                    image.Stretch = System.Windows.Media.Stretch.UniformToFill;
                    image.StretchDirection = StretchDirection.Both;
                    image.HorizontalAlignment = HorizontalAlignment.Stretch;
                    image.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }

            return null;
        }

        private IEnumerable<ImageType> CalculateImageType(Size availableSize)
        {
            var defaultPriority = new[] { ImageType.Backdrop, ImageType.Screenshot, ImageType.Box, ImageType.Thumb, ImageType.Disc, ImageType.Primary };

            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height)) {
                return new[] { ImageType.Primary }.Concat(defaultPriority);
            }

            var aspectRatio = availableSize.Width/availableSize.Height;

            if (_item.OriginalPrimaryImageAspectRatio != null &&
                ApproximatelyEqual(_item.OriginalPrimaryImageAspectRatio.Value, aspectRatio, 0.2)) {
                return new[] { ImageType.Primary }.Concat(defaultPriority);
            }

            return defaultPriority;
        }

        private bool ApproximatelyEqual(double x, double y, double threshold)
        {
            return Math.Abs(x - y) <= threshold;
        }

        private async Task<string> GetImageUrl(ImageType imageType, Size availableSize)
        {
            var imageOptions = new ImageOptions {
                ImageType = imageType,
                EnableImageEnhancers = false,
                Width = !double.IsInfinity(availableSize.Width) && (double.IsInfinity(availableSize.Height) || availableSize.Width > availableSize.Height) ? (int?)availableSize.Width : null,
                Height = !double.IsInfinity(availableSize.Height) && (double.IsInfinity(availableSize.Width) || availableSize.Height > availableSize.Width) ? (int?)availableSize.Height : null,
            };

            var apiClient = _connectionManager.GetApiClient(_item);

            try {
                if (imageType == ImageType.Thumb) {
                    return apiClient.GetThumbImageUrl(_item, imageOptions);
                }

                return apiClient.GetImageUrl(_item, imageOptions);
            }
            catch {
                return null;
            }
        }

        private bool ImageTypePresent(ImageType imageType)
        {
            if (imageType == ImageType.Backdrop) {
                if (_item.BackdropCount == 0) {
                    return false;
                }
            } else if (imageType == ImageType.Thumb) {
                if (!_item.ImageTags.ContainsKey(imageType) && string.IsNullOrEmpty(_item.ParentThumbImageTag) && string.IsNullOrEmpty(_item.SeriesThumbImageTag)) {
                    return false;
                }
            } 

            return _item.ImageTags.ContainsKey(imageType);
        }
    }
}
