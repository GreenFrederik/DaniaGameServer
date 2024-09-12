using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using DaniaGameServer;

public class Server
{
	private readonly UdpClient socket;
	private readonly int port;
	private bool active;

	private readonly Dictionary<IPEndPoint, Client> connections;
	
	public Server(int port)
	{
		this.port = port;
		socket = new(port);
		connections = new();
	}
	
	public void Start()
	{
		active = true;
		Task.Run(Listen);
		Console.WriteLine("Server started on port: " + port);
	}
	
	private async Task Listen()
	{
		while (active)
		{
			if (socket.Available > 0)
			{
				UdpReceiveResult received = await socket.ReceiveAsync();
				await HandleMessage(received.RemoteEndPoint, received.Buffer);
			}
		}
	}
	
	private async Task HandleMessage(IPEndPoint remoteEndPoint, byte[] data)
	{
		PacketType type;
		using (PacketReader reader = new(data))
			type = (PacketType)reader.Type;
		
		switch (type)
		{
			case PacketType.Connect:
				{
					bool isAuthorized = true;
					// using (HttpClient http = new())
					// {
					// 	HttpResponseMessage response = await http.GetAsync("url"); // GET FROM SESSION SERVICE
					// 	isAuthorized = response.IsSuccessStatusCode;
					// }

					if (isAuthorized)
					{
						string name = null; // GET FROM SESSION SERVICE
						Client client = new(name);
						connections.Add(remoteEndPoint, client);
						System.Console.WriteLine(remoteEndPoint + " connected.");
						
						using (PacketWriter writer = new(PacketType.Connect))
						{
							writer.Write(true);
							SendTo(writer, remoteEndPoint);
						}
					}
					else
					{
						using (PacketWriter writer = new(PacketType.Connect))
						{
							writer.Write(false);
							SendTo(writer, remoteEndPoint);
						}
					}
				}
				break;
				
			case PacketType.Disconnect:
				{
					if (connections.ContainsKey(remoteEndPoint))
					{
						using (PacketWriter packet = new(PacketType.Disconnect))
						{
							ulong id = connections[remoteEndPoint].ID;
							packet.Write(id);
							SendToAll(packet);
						}
						
						connections.Remove(remoteEndPoint);
					}
				}
				break;
				
			case PacketType.Move:
				{
					if (!connections.ContainsKey(remoteEndPoint))
						return;

					float moveSpeed = 5f;
					
					using (PacketReader reader = new(data))
					{
						Client client = connections[remoteEndPoint];
						float x = reader.Read<float>();
						float y = reader.Read<float>();
						
						Vector2 moveDirection = new(x, y);
						client.Position += moveDirection * moveSpeed;
						
						using (PacketWriter writer = new(PacketType.Move))
						{
							writer.Write(client.ID);
							writer.Write(x);
							writer.Write(y);
							SendToAll(writer);
						}
					}
				}
				break;
		}
	}
	
	private void SendTo(PacketWriter packet, IPEndPoint endPoint)
	{
		socket.Send(packet.Bytes, endPoint);
	}
	
	private void SendToAll(PacketWriter packet)
	{
		foreach (var pair in connections)
		{
			IPEndPoint endPoint = pair.Key;
			SendTo(packet, endPoint);
		}
	}
}