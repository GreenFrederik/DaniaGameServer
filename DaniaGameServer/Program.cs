public static class Program
{
	public static void Main(string[] args)
	{
		int port = args.Length > 0 ? int.Parse(args[0]) : 7777;
		Server server = new(port);
		server.Start();

		while (true){}
	}
}