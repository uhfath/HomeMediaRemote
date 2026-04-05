using System.Drawing;
using System.Windows.Forms;

namespace HomeMediaRemote.Status.Windows;

/// <summary>
/// Управление иконкой в системном трее (notification area).
/// </summary>
internal sealed class TrayIconManager : IDisposable
{
	private NotifyIcon? _notifyIcon;
	private Icon? _icon;
	private Action? _iconAction;

	// ── показать / скрыть ────────────────────────────────────

	/// <summary>Показать иконку из Bitmap с подсказкой.</summary>
	public void Show(Bitmap bitmap, string tooltip, Action iconAction)
	{
		Hide();

		_icon = BitmapToManagedIcon(bitmap);
		_notifyIcon = new NotifyIcon
		{
			Icon = _icon,
			Text = tooltip,  // макс. 127 символов
			Visible = true
		};

		_iconAction = iconAction;
		_notifyIcon.Click += _notifyIcon_Click;
	}

	private void _notifyIcon_Click(object? sender, EventArgs e)
	{
		_iconAction?.Invoke();
	}

	/// <summary>Скрыть и удалить иконку из трея.</summary>
	public void Hide()
	{
		if (_notifyIcon is not null)
		{
			_notifyIcon.Click -= _notifyIcon_Click;
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
			_notifyIcon = null;
		}
		_icon?.Dispose();
		_icon = null;
	}

	// ── обновление ───────────────────────────────────────────

	/// <summary>Заменить картинку иконки (если видна).</summary>
	public void UpdateIcon(Bitmap bitmap)
	{
		if (_notifyIcon is null) return;

		var oldIcon = _icon;
		_icon = BitmapToManagedIcon(bitmap);
		_notifyIcon.Icon = _icon;
		oldIcon?.Dispose();
	}

	/// <summary>Обновить текст подсказки.</summary>
	public void UpdateTooltip(string tooltip)
	{
		if (_notifyIcon is not null)
			_notifyIcon.Text = tooltip;
	}

	public void Dispose() => Hide();

	// ── helper ───────────────────────────────────────────────

	/// <summary>
	/// Bitmap → управляемый Icon.
	/// GetHicon() возвращает неуправляемый HICON — оборачиваем
	/// через Clone(), затем сразу освобождаем дескриптор.
	/// </summary>
	private static Icon BitmapToManagedIcon(Bitmap bitmap)
	{
		nint hIcon = bitmap.GetHicon();
		try
		{
			using var tmp = Icon.FromHandle(hIcon);   // не владеет hIcon
			return (Icon)tmp.Clone();                  // полностью управляемая копия
		}
		finally
		{
			NativeMethods.DestroyIcon(hIcon);
		}
	}
}
