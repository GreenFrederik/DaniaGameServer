using System.Net;
using System.Numerics;

namespace DaniaGameServer;

public class Client
{
	private readonly ulong id;
	private readonly string name;
	private Vector2 position;
	
	private static ulong nextID;

	public ulong ID => id;
	
	public Vector2 Position 
	{
		get => position;
		set => position = value;
	}
	
	public Client(string name)
	{
		id = nextID++;
		position = Vector2.Zero;
		this.name = name;
	}
}