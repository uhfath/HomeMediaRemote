using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Sound;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.States
{
	internal class SoundState : IHostedService
	{
		private readonly AudioOutputController _audioOutputController;
		private readonly IconStatusManager _iconStatusManager;
		private readonly IServiceProvider _serviceProvider;

		private void OnAudioOutputChanged(AudioVolumeChangedEventArgs eventArgs)
		{
			if (eventArgs.IsMuted)
			{
				_iconStatusManager.ShowNoSoundStatus(() =>
				{
					var soundUnMuteCommand = _serviceProvider.GetRequiredService<ICommand<SoundUnMuteCommand.Request>>();
					soundUnMuteCommand.Execute(SoundUnMuteCommand.Request.Empty);
				});
			}
			else
			{
				_iconStatusManager.HideNoSoundStatus();
			}
		}

		public SoundState(
			AudioOutputController audioOutputController,
			IconStatusManager iconStatusManager,
			IServiceProvider serviceProvider)
		{
			this._audioOutputController = audioOutputController;
			this._iconStatusManager = iconStatusManager;
			this._serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_audioOutputController.Subscribe(OnAudioOutputChanged);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_audioOutputController.Unsubscribe();
			return Task.CompletedTask;
		}
	}
}
