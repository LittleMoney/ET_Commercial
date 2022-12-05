using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	public class PlayerComponent : Component
	{
		public static PlayerComponent Instance { get; private set; }

		private readonly Dictionary<int, Player> idPlayers = new Dictionary<int, Player>();

		public void Awake()
		{
			Instance = this;
		}

		public void Add(Player player)
		{
			this.idPlayers.Add(player.userId, player);
		}

		public Player Get(int userId)
		{
			this.idPlayers.TryGetValue(userId, out Player gamer);
			return gamer;
		}

		public void Remove(int userId)
		{
			this.idPlayers.Remove(userId);
		}

		public int Count
		{
			get
			{
				return this.idPlayers.Count;
			}
		}

		public Player[] GetAll()
		{
			return this.idPlayers.Values.ToArray();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (Player player in this.idPlayers.Values)
			{
				player.Dispose();
			}

			Instance = null;
		}
	}
}