namespace HomeMediaRemote.Status.Windows
{
	public class FormManager : IDisposable
	{
		private Form _marshalForm = null!;
		private bool _isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					_marshalForm.Invoke(Application.ExitThread);
				}

				_isDisposed = true;
			}
		}

		public FormManager()
		{
			var isFormReady = new ManualResetEventSlim();

			var uiThread = new Thread(() =>
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// Невидимая форма — единственная цель: иметь HWND,
				// через который Invoke перебрасывает делегаты в этот поток.
				_marshalForm = new Form
				{
					FormBorderStyle = FormBorderStyle.None,
					ShowInTaskbar = false,
					Size = Size.Empty
				};

				_ = _marshalForm.Handle;
				isFormReady.Set();

				Application.Run();
			});

			uiThread.SetApartmentState(ApartmentState.STA); // WinForms требует STA
			uiThread.IsBackground = true;
			uiThread.Start();

			isFormReady.Wait(); // ждём, пока HWND готов
		}

		public void Invoke(Action action)
		{
			_marshalForm.Invoke(action);
		}

		public T Invoke<T>(Func<T> action)
		{
			return _marshalForm.Invoke(action);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
