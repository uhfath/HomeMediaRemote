using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundVolUpCommand : ICommand<SoundVolUpCommand.Request>
	{
		private record Request
		{
			public int Step { get; init; }
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundVolUpCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		Task ICommand<Request>.ExecuteAsync(Request request, CancellationToken cancellationToken)
		{
			_audioOutputController.SetVolumeUp(request.Step);
			return Task.CompletedTask;
		}
	}
}
