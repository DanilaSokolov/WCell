using WCell.Core.Initialization;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
	public class AzjolNerub : BaseInstance
	{
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
		}
	}

}