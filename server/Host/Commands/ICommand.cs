namespace HomeMediaRemote.Host.Commands
{
	internal interface ICommand<in TRequest>
	{
		public void Execute(TRequest request);
	}
}
