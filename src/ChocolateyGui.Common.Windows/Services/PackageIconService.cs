// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using LiteDB;
	using MahApps.Metro.IconPacks;
	using Microsoft.VisualStudio.Threading;
	using Serilog;
	using SkiaSharp;
	using Splat;
	using Svg.Skia;
	using ILogger = Serilog.ILogger;

	public class PackageIconService : IPackageIconService
	{
		#region Declarations

		private static readonly AsyncReaderWriterLock Lock = new AsyncReaderWriterLock();
		private static readonly ILogger Logger = Log.ForContext<PackageIconService>();
		private readonly LiteDatabase _userDatabase;

		#endregion

		#region Constructors

		public PackageIconService(LiteDatabase userDatabase)
		{
			_userDatabase = userDatabase;
		}

		#endregion

		#region IPackageIconService implementation

		public virtual ImageSource GetEmptyIconImage()
		{
			var image = new BitmapImage(new Uri("pack://application:,,,/ChocolateyGui;component/chocolatey@4.png", UriKind.RelativeOrAbsolute));
			image.Freeze();
			return image;
		}

		public virtual ImageSource GetErrorIconImage()
		{
			var packIcon = new PackIconEntypo { Kind = PackIconEntypoKind.CircleWithCross };

			var pen = new Pen();
			pen.Freeze();
			var geometry = Geometry.Parse(packIcon.Data);
			var geometryDrawing = new GeometryDrawing(Brushes.OrangeRed, pen, geometry);
			var drawingGroup = new DrawingGroup();
			drawingGroup.Children.Add(geometryDrawing);
			drawingGroup.Transform = new ScaleTransform(3.5, 3.5);
			var drawingImage = new DrawingImage(drawingGroup);
			drawingImage.Freeze();
			return drawingImage;
		}

		public virtual async Task<ImageSource> GetImage(String url, Size desiredSize, DateTime absoluteExpiration)
		{
			return (await LoadImage(url, desiredSize, absoluteExpiration)).ToNative();
		}

		#endregion

		#region Private implementation

		private async Task<Stream> DownloadUrl(String url, Size desiredSize, DateTime absoluteExpiration)
		{
			var id = $"imagecache/{url}";
			var imageStream = new MemoryStream();
			var fileStorage = _userDatabase.FileStorage;

			using (await PackageIconService.Lock.UpgradeableReadLockAsync())
			{
				if (fileStorage.Exists(id))
				{
					var info = fileStorage.FindById(id);
					if (info.Metadata.ContainsKey("Expires"))
					{
						var expires = info.Metadata["Expires"].AsDateTime;
						if (expires > DateTime.UtcNow)
						{
							info.CopyTo(imageStream);
							return imageStream;
						}
					}
				}
			}

			// If we couldn't find the image or it expired
			using (var client = new HttpClient())
			{
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();

				using (var memoryStream = new MemoryStream())
				{
					await response.Content.CopyToAsync(memoryStream);
					memoryStream.Position = 0;

					PackageIconService.ExtractImageFromStream(url, desiredSize, memoryStream, imageStream);
				}
			}

			using (await PackageIconService.Lock.WriteLockAsync())
			{
				// we don't need to delete the file, cause a upload does
				// Upload: Send file or stream to database. Can be used with file or Stream. If file already exists, file content is overwritten.
				PackageIconService.UploadFileAndSetMetadata(absoluteExpiration, imageStream, fileStorage, id, url);
			}

			return imageStream;
		}

		private static void ExtractImageFromStream(String url, Size desiredSize, Stream inputStream, Stream imageStream)
		{
			if (PackageIconService.IsSvg(url))
			{
				using (var svg = new SKSvg())
				{
					try
					{
						svg.Load(inputStream);
					}
					catch (Exception exception)
					{
						PackageIconService.Logger.Warning(exception, $"Something is wrong with: \"{url}\".");
						return;
					}

					var skPicture = svg.Picture;
					var imageInfo = new SKImageInfo((Int32)desiredSize.Width, (Int32)desiredSize.Height);

					using (var surface = SKSurface.Create(imageInfo))
					{
						using (var canvas = surface.Canvas)
						{
							// calculate the scaling need to fit to desired size
							var scaleX = desiredSize.Width / skPicture.CullRect.Width;
							var scaleY = desiredSize.Height / skPicture.CullRect.Height;
							var matrix = SKMatrix.MakeScale((Single)scaleX, (Single)scaleY);

							// draw the svg
							canvas.Clear(SKColors.Transparent);
							canvas.DrawPicture(skPicture, ref matrix);
							canvas.Flush();

							using (var data = surface.Snapshot())
							{
								using (var pngImage = data.Encode(SKEncodedImageFormat.Png, 100))
								{
									pngImage.SaveTo(imageStream);
								}
							}
						}
					}
				}
			}
			else
			{
				var bitmap = SKBitmap.Decode(inputStream);
				if (bitmap != null)
				{
					var resizeInfo = PackageIconService.GetResizeSkImageInfo(desiredSize, bitmap);
					using (var resizedBitmap = bitmap.Resize(resizeInfo, SKFilterQuality.High))
					{
						bitmap.Dispose();

						using (var image = SKImage.FromBitmap(resizedBitmap))
						{
							using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
							{
								data.SaveTo(imageStream);
							}
						}
					}
				}
			}
		}

		private static SKImageInfo GetResizeSkImageInfo(Size desiredSize, SKBitmap bitmap)
		{
			var resizeInfo = new SKImageInfo((Int32)desiredSize.Width, (Int32)desiredSize.Height);

			// Test whether there is more room in width or height
			if (Math.Abs(bitmap.Width - desiredSize.Width) > Math.Abs(bitmap.Height - desiredSize.Height))
			{
				// More room in width, so leave image width set to canvas width
				// and increase/decrease height by same ratio
				var widthRatio = desiredSize.Width / bitmap.Width;
				var newHeight = (Int32)Math.Floor(bitmap.Height * widthRatio);

				resizeInfo.Height = newHeight;
			}
			else
			{
				// More room in height, so leave image height set to canvas height
				// and increase/decrease width by same ratio
				var heightRatio = desiredSize.Height / bitmap.Height;
				var newWidth = (Int32)Math.Floor(bitmap.Width * heightRatio);

				resizeInfo.Width = newWidth;
			}

			return resizeInfo;
		}

		private static Boolean IsSvg(String url)
		{
			var extension = Path.GetExtension(url)?.ToLower();
			return extension == ".svg" || extension == ".svgz";
		}

		private async Task<IBitmap> LoadImage(String url, Size desiredSize, DateTime absoluteExpiration)
		{
			var imageStream = await DownloadUrl(url, desiredSize, absoluteExpiration).ConfigureAwait(false);

			// Don't specify width and height to keep the aspect ratio of the image.
			return await BitmapLoader.Current.Load(imageStream, null, null);
		}

		private static void UploadFileAndSetMetadata(DateTime absoluteExpiration, MemoryStream imageStream, ILiteStorage<String> fileStorage, String id, String url)
		{
			imageStream.Position = 0;
			fileStorage.Upload(id, url, imageStream);
			fileStorage.SetMetadata(id, new BsonDocument { new KeyValuePair<String, BsonValue>("Expires", absoluteExpiration) });
			imageStream.Position = 0;
		}

		#endregion
	}
}