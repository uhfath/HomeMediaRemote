using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundUnMuteCommand : ICommand<SoundUnMuteCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundUnMuteCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.Mute = false;
		}
	}
}
