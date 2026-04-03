namespace HomeMediaRemote.Host.Commands
{
	internal interface ICommand<in TRequest>
	{
		public Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
	}
}
