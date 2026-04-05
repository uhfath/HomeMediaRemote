using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Host.Commands;
using HomeMediaRemote.Host.Commands.Mic;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.States
{
	internal class MicState : IHostedService
	{
		private readonly AudioInputController _audioInputController;
		private readonly IconStatusManager _iconStatusManager;
		private readonly IServiceProvider _serviceProvider;

		private void OnAudioInputChanged(AudioVolumeChangedEventArgs eventArgs)
		{
			if (eventArgs.IsMuted)
			{
				_iconStatusManager.ShowMicOffStatus(() =>
				{
					var micOnCommand = _serviceProvider.GetRequiredService<ICommand<MicOnCommand.Request>>();
					micOnCommand.Execute(MicOnCommand.Request.Empty);
				});
			}
			else
			{
				_iconStatusManager.HideMicOffStatus();
			}
		}

		public MicState(
			AudioInputController audioOutputController,
			IconStatusManager iconStatusManager,
			IServiceProvider serviceProvider)
		{
			this._audioInputController = audioOutputController;
			this._iconStatusManager = iconStatusManager;
			this._serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_audioInputController.Subscribe(OnAudioInputChanged);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_audioInputController.Unsubscribe();
			return Task.CompletedTask;
		}
	}
}
