using System;
using System.Net.Sockets;
using System.Net;
using shared;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace server {

	/**
	 * Basic TCPGameServer that runs our game.
	 * 
	 * Server is made up out of different rooms that can hold different members.
	 * Each member is identified by a TcpMessageChannel, which can also be used for communication.
	 * 
	 * Each room is responsible for cleaning up faulty clients (since it might involve gameplay, status changes etc).
	 * 
	 * Note the you can determine the max amount of supported players per game (this is the amount of clients at wich the server will start)
	 * Just run server.exe [number of players] on a console window
	 * 
	 */
	class TCPGameServer
	{
		private LoginRoom _loginRoom;   //this is the room every new user joins
		private LobbyRoom _lobbyRoom;   //this is the room a user moves to after a successful 'login'
		private List<GameRoom> _gameRooms;      //this stores the rooms a user moves to when a game is succesfully started
		public int MAX_PLAYERS_PER_GAME { get; private set; }//number of players in each game, can be modiffied providing a number as an argument to the server whe executing it

		//stores additional info for a player
		private Dictionary<TcpMessageChannel, PlayerInfo> _playerInfo = new Dictionary<TcpMessageChannel, PlayerInfo>();
		public static int Main(string[] args)
		{
			int num;
			// Test if input arguments were supplied.
			if (args.Length > 0)
			{
				// Try to convert the input arguments to numbers and set the max of players.
				// This will throw an exception if the argument is not a number.
				bool test = int.TryParse(args[0], out num);
				if (!test || num < 2 || num > 10)
				{
					Console.WriteLine("Please enter a numeric argument between 2 and 10 (player max per game) or no argument (defaults to 3).");
					return 1;
				}
			}
			else
				num = 3;

			TCPGameServer tcpGameServer = new TCPGameServer();
			tcpGameServer.run(num);
			return 0;
		}
		private TCPGameServer()
		{
			_loginRoom = new LoginRoom(this);
			_lobbyRoom = new LobbyRoom(this);
			_gameRooms = new List<GameRoom>();
		}

		private void run(int maxPlayers)
		{
			MAX_PLAYERS_PER_GAME = maxPlayers;
			Log.LogInfo("Maximum players per game is set to " + MAX_PLAYERS_PER_GAME, this, ConsoleColor.Green);
			Log.LogInfo("Starting server on port 55555", this, ConsoleColor.Green);

			var timer = new Timer(
			e => checkHeartBeat(),
			null,
			TimeSpan.Zero,
			TimeSpan.FromSeconds(2));

			//start listening for incoming connections (with max 50 in the queue)
			//we allow for a lot of incoming connections, so we can handle them
			//and tell them whether we will accept them or not instead of bluntly declining them
			TcpListener listener = new TcpListener(IPAddress.Any, 55555);
			listener.Start(50);

			while (true)
			{
				//check for new members	
				if (listener.Pending())
				{
					//get the waiting client
					Log.LogInfo("Accepting new client...", this, ConsoleColor.White);
					TcpClient client = listener.AcceptTcpClient();
					//and wrap the client in an easier to use communication channel
					TcpMessageChannel channel = new TcpMessageChannel(client);
					//and add it to the login room for further 'processing'
					_loginRoom.AddMember(channel);
				}
				//now update every single room
				_loginRoom.Update();
				_lobbyRoom.Update();
				safeForEachGameRoomUpdate();

				Thread.Sleep(100);
			}

		}
		
		//provide access to the different rooms on the server 
		public LoginRoom GetLoginRoom() { return _loginRoom; }
		public LobbyRoom GetLobbyRoom() { return _lobbyRoom; }

		public GameRoom NewGameRoom() {
			GameRoom room = new GameRoom(this);
			_gameRooms.Add(room);
			Log.LogInfo("New game room created. There are " + _gameRooms.Count + " game rooms running right now", this, ConsoleColor.Magenta);
			return room;
		}

		public void DestroyRoom(GameRoom room)
		{
			_gameRooms.Remove(room);
			Log.LogInfo("Game room destroyed. There are " + _gameRooms.Count + " game rooms running right now", this, ConsoleColor.Magenta);

		}


		/**
		 * Returns a handle to the player info for the given client 
		 * (will create new player info if there was no info for the given client yet)
		 */
		public PlayerInfo GetPlayerInfo (TcpMessageChannel pClient)
		{
			if (!_playerInfo.ContainsKey(pClient))
			{
				_playerInfo[pClient] = new PlayerInfo();
			}

			return _playerInfo[pClient];
		}

		public List<PlayerInfo> GetPlayerInfo(Predicate<PlayerInfo> pPredicate)
		{
			return _playerInfo.Values.ToList<PlayerInfo>().FindAll(pPredicate);
		}

		/**
		 * Should be called by a room when a member is closed and removed.
		 */
		public void RemovePlayerInfo (TcpMessageChannel pClient)
		{
			_playerInfo.Remove(pClient);
		}

		//call this method whe a heartbeat is received
		public void Beat(TcpMessageChannel pClient)
		{
			_playerInfo[pClient].heartBeatState = true;
		}

		//updates all gamerooms while preventing errors with the use of collections to store them
		private void safeForEachGameRoomUpdate()
		{
			for (int i = _gameRooms.Count - 1; i >= 0; i--)
			{
				//skip any rooms that have been 'killed' in the mean time
				if (i >= _gameRooms.Count) continue;
				//call the method on any still existing member
				_gameRooms[i].Update();
			}
		}

		//kill clients that didn't heartbeat and set the hartbeatstate of the ones that did to false
		private void checkHeartBeat()
		{
			foreach (TcpMessageChannel player in _playerInfo.Keys.ToArray<TcpMessageChannel>())
			{
				if (_playerInfo[player].heartBeatState == false)
					player.SendMessage(new Hb());
				else _playerInfo[player].heartBeatState = false;
			}
		}
	}
}


