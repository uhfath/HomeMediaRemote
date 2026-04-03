using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundVolDownCommand : ICommand<SoundVolDownCommand.Request>
	{
		private record Request
		{
			public int Step { get; init; }
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundVolDownCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioOutputController.SetVolumeDown(request.Step);
			return Task.CompletedTask;
		}
	}
}
