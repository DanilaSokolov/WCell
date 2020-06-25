using WCell.Addons.Default.Lang;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
    public class TrialOfTheCrusader : BaseInstance
    {
		public NPC announcerNPC;
		static NPCEntry barretEntry;

        private Vector3 barretPosition = new Vector3(559.1528f, 90.55729f, 395.2734f);

		public void ChangeAnnouncer(NPC announcer)
		{
			announcer.SetEntry(barretEntry);
			announcerNPC = announcer;
		}

		[Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            barretEntry = NPCMgr.GetEntry(NPCId.BarrettRamsey);
			
			FuckMyBall();
        }

        private static void FuckMyBall()
        {
			
			barretEntry.DefaultGossip = new GossipMenu(new DynamicGossipEntry(10599, new GossipStringFactory(
				convo =>
				{
					var instance = convo.Character.Map as TrialOfTheCrusader;
					if (instance != null)
					{
						return string.Format("Are you ready for your next challenge, {0} ?", convo.Character.Class);
						
					}
					else return string.Empty;
				})),
				new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCArelas1),
					new NonNavigatingDecidingGossipAction(new GossipActionHandler(
					convo =>
					{
						((NPC)convo.Speaker).NPCFlags &= ~NPCFlags.Gossip;
						var instance = convo.Character.Map as TrialOfTheCrusader;
						if (instance != null)
						{
							instance.announcerNPC.MoveToThenExecute(instance.barretPosition,
								unit => unit.Orientation = 4.714f/*5.078908f*/);
						}
					}), new GossipActionDecider(
								convo =>
								{									
										return false;
								}))),
				new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCArelas2),
					new NonNavigatingDecidingGossipAction(new GossipActionHandler(
						convo =>
						{
							((NPC)convo.Speaker).NPCFlags &= ~NPCFlags.Gossip;
							var instance = convo.Character.Map as TrialOfTheCrusader;
							if (instance != null)
							{
								
							}
						}), new GossipActionDecider(
									convo =>
									{
										var instance = convo.Speaker.Map as TrialOfTheCrusader;
										if (instance != null)
										{
											return true;
										}
										return false;
									}))));
		}

    }
}