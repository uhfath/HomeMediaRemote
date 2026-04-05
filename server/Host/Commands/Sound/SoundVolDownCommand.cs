using HomeMediaRemote.Audio.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundVolDownCommand : ICommand<SoundVolDownCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
			public int Step { get; init; }
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundVolDownCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.SetVolumeDown(request.Step);
		}
	}
}
