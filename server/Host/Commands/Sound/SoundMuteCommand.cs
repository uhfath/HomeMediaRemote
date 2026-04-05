using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundMuteCommand : ICommand<SoundMuteCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundMuteCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.SetMute(true);
		}
	}
}
