using Microsoft.Extensions.Logging;

namespace HomeMediaRemote.Audio.Windows
{
	/// <summary>
	/// Управление текущим активным устройством вывода (колонки / наушники):
	/// mute/unmute, уровень громкости, подписка на внешние изменения.
	///
	/// При смене устройства по умолчанию в ОС — автоматически переключается
	/// на новое устройство, сохраняя подписку на изменения громкости.
	/// </summary>
	public sealed class AudioOutputController : AudioController
	{
		public AudioOutputController(ILogger<AudioOutputController> logger)
			: base(Guid.NewGuid(), EDataFlow.eRender, logger)
		{
		}
	}
}
