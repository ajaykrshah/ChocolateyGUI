// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;
	using Serilog;

	/// <summary>
	///     Interaction logic for InternetImage.xaml
	/// </summary>
	public partial class InternetImage
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<InternetImage>();
		private static readonly IPackageIconService PackageIconService = IoC.Get<IPackageIconService>();

		#endregion

		#region Constructors

		public InternetImage()
		{
			InitializeComponent();
#pragma warning disable 4014
			SetImage(this.IconUrl);
#pragma warning restore 4014
			this.ToObservable(InternetImage.IconUrlProperty, () => this.IconUrl)
			    .Subscribe(async url => await SetImage(url));
		}

		#endregion

		#region Fields and constants

		public static readonly DependencyProperty IconUrlProperty = DependencyProperty.Register(
			nameof(InternetImage.IconUrl), typeof(String), typeof(InternetImage), new PropertyMetadata(default(String)));

		#endregion

		#region Properties

		public String IconUrl
		{
			get => (String)GetValue(InternetImage.IconUrlProperty);
			set => SetValue(InternetImage.IconUrlProperty, value);
		}

		#endregion

		#region Private implementation

		private Size GetCurrentSize()
		{
			var scale = NativeMethods.GetScaleFactor();
			var x = (Int32)Math.Round(this.ActualWidth * scale);
			var y = (Int32)Math.Round(this.ActualHeight * scale);
			return new Size(x, y);
		}

		private async Task SetImage(String url)
		{
			if (String.IsNullOrWhiteSpace(url))
			{
				PART_Image.Source = InternetImage.PackageIconService.GetEmptyIconImage();
				PART_Loading.IsActive = false;
				return;
			}

			PART_Loading.IsActive = true;

			var size = GetCurrentSize();
			var expiration = DateTime.UtcNow + TimeSpan.FromDays(1);
			ImageSource source;
			try
			{
				source = await InternetImage.PackageIconService.GetImage(url, size, expiration);
			}
			catch (HttpRequestException)
			{
				source = InternetImage.PackageIconService.GetErrorIconImage();
			}
			catch (ArgumentException exception)
			{
				InternetImage.Logger.Warning(exception, $"Got an invalid img url: \"{url}\".");
				source = InternetImage.PackageIconService.GetErrorIconImage();
			}
			catch (Exception exception)
			{
				InternetImage.Logger.Warning(exception, $"Something went wrong with: \"{url}\".");
				source = InternetImage.PackageIconService.GetErrorIconImage();
			}

			PART_Image.Source = source;
			PART_Loading.IsActive = false;
		}

		#endregion
	}
}