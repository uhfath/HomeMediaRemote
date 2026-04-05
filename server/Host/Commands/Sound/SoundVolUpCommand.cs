using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundVolUpCommand : ICommand<SoundVolUpCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
			public int Step { get; init; }
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundVolUpCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.SetVolumeUp(request.Step);
		}
	}
}
