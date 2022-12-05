using ETModel;
using System.Collections.Generic;
using System.Linq;

namespace ETHotfix
{
	[ObjectSystem]
	public class PlayerComponentSystem : AwakeSystem<PlayerComponent>
	{
		public override void Awake(PlayerComponent self)
		{
			self.Awake();
		}
	}


}