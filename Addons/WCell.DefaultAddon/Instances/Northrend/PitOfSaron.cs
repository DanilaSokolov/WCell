using WCell.Core.Initialization;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 2/11/2010
///

namespace WCell.Addons.Default.Instances
{
	public class PitOfSaron : BaseInstance
	{
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
		}
	}

}