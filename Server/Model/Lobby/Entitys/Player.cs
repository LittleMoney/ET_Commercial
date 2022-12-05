using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	/// <summary>
	/// 网关上的用户实体
	/// </summary>
	public sealed class Player : Entity
	{
		public int userId;

		public long gameActorId;

		public long userActorId;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}