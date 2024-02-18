// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reactive.Linq;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.ViewModels;
	using ChocolateyGui.Common.Windows.Services;

	public class SourcesViewModel : Conductor<ISourceViewModelBase>.Collection.OneActive, IHandleWithTask<SourcesUpdatedMessage>
	{
		#region Declarations

		private readonly IConfigService _configService;
		private readonly IImageService _imageService;
		private readonly Func<String, LocalSourceViewModel> _localSourceVmFactory;
		private readonly IChocolateyService _packageService;
		private readonly CreateRemove _remoteSourceVmFactory;
		private readonly IVersionService _versionService;
		private Boolean _firstLoad = true;

		#endregion

		#region Constructors

		public SourcesViewModel(
			IChocolateyService packageService,
			IConfigService configService,
			IImageService imageService,
			IEventAggregator eventAggregator,
			IVersionService versionService,
			Func<String, LocalSourceViewModel> localSourceVmFactory,
			CreateRemove remoteSourceVmFactory)
		{
			_packageService = packageService;
			_configService = configService;
			_imageService = imageService;
			_versionService = versionService;
			_remoteSourceVmFactory = remoteSourceVmFactory;
			_localSourceVmFactory = localSourceVmFactory;

			if (localSourceVmFactory == null)
			{
				throw new ArgumentNullException(nameof(localSourceVmFactory));
			}

			if (remoteSourceVmFactory == null)
			{
				throw new ArgumentNullException(nameof(remoteSourceVmFactory));
			}

			eventAggregator.Subscribe(this);
		}

		#endregion

		#region Delegates

		public delegate RemoteSourceViewModel CreateRemove(ChocolateySource source);

		#endregion

		#region Properties

		public String DisplayVersion => _versionService.DisplayVersion;
		public ImageSource PrimaryApplicationImageSource => _imageService.PrimaryApplicationImage;
		public ImageSource SecondaryApplicationImageSource => _imageService.SecondaryApplicationImage;

		#endregion

		#region Public interface

		public virtual async Task LoadSources()
		{
			var oldItems = this.Items.Skip(1).ToList();

			var sources = await _packageService.GetSources();
			var vms = new List<ISourceViewModelBase>();

			if (_configService.GetEffectiveConfiguration().ShowAggregatedSourceView ?? false)
			{
				vms.Add(_remoteSourceVmFactory(new ChocolateyAggregatedSources()));
				vms.Add(new SourceSeparatorViewModel());
			}

			foreach (var source in sources.Where(s => !s.Disabled).OrderBy(s => s.Priority))
			{
				vms.Add(_remoteSourceVmFactory(source));
			}

			await Execute.OnUIThreadAsync(
				() =>
				{
					this.Items.RemoveRange(oldItems);
					this.Items.AddRange(vms);

					ActivateItem(this.Items[0]);
				});
		}

		#endregion

		#region ViewAware overrides

		protected override void OnViewReady(Object view)
		{
			Observable.FromEventPattern<PropertyChangedEventArgs>(this, nameof(PropertyChangedBase.PropertyChanged))
			          .Where(p => p.EventArgs.PropertyName == nameof(ConductorBaseWithActiveItem<ISourceViewModelBase>.ActiveItem))
			          .Subscribe(p => this.DisplayName = $"Source - {this.ActiveItem?.DisplayName}");

			if (_firstLoad)
			{
				this.Items.Add(_localSourceVmFactory("[Resources_ThisPC]"));

				_ = LoadSources();
				_firstLoad = false;
			}
		}

		#endregion

		#region IHandleWithTask<SourcesUpdatedMessage> implementation

		public async Task Handle(SourcesUpdatedMessage message)
		{
			await LoadSources();
		}

		#endregion

		#region SourcesComparer Class

		private class SourcesComparer : IEqualityComparer<RemoteSourceViewModel>
		{
			#region IEqualityComparer<RemoteSourceViewModel> implementation

			public Boolean Equals(RemoteSourceViewModel x, RemoteSourceViewModel y)
			{
				return x.Source.Equals(y.Source);
			}

			public Int32 GetHashCode(RemoteSourceViewModel obj)
			{
				return obj.Source.GetHashCode();
			}

			#endregion
		}

		#endregion
	}
}