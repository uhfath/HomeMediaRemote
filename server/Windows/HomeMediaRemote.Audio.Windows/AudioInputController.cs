using Microsoft.Extensions.Logging;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством ввода (микрофоном):
	/// mute/unmute и уровень чувствительности (громкости записи).
	///
	/// При смене устройства по умолчанию в ОС — автоматически переключается
	/// на новое устройство, сохраняя подписку на изменения громкости.
	/// </summary>
	public sealed class AudioInputController : AudioController
	{
		public AudioInputController(ILogger<AudioInputController> logger)
			: base(Guid.NewGuid(), EDataFlow.eCapture, logger)
		{
		}
	}
}
