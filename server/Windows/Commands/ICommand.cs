namespace HomeMediaRemote.Windows.Commands
{
	internal interface ICommand<in TRequest>
	{
		public Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
	}
}
