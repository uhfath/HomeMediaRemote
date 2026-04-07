using HomeMediaRemote.Audio.Windows;
using HomeMediaRemote.Status.Windows;

namespace HomeMediaRemote.Host.Commands.Sound
{
	internal class SoundTriggerCommand : ICommand<SoundTriggerCommand.Request>
	{
		public record Request
		{
			public static readonly Request Empty = new();
		}

		private readonly AudioOutputController _audioOutputController;

		public SoundTriggerCommand(
			AudioOutputController audioOutputController)
		{
			this._audioOutputController = audioOutputController;
		}

		void ICommand<Request>.Execute(Request request)
		{
			_audioOutputController.TriggerMute();
		}
	}
}
