using System;
using System.Media;
using WCell.Addons.Default.Lang;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Misc;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 6/11/2009
///

namespace WCell.Addons.Default.Instances
{
	public class Ulduar : BaseInstance
	{

		private static GOEntry _ulduarTeleport;
        private static GOEntry __doodadUlIniverseGlobe01;

		private static NPCEntry AlgalonTheObserver;

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void InitGOs()
		{
            _ulduarTeleport = GOMgr.GetEntry(GOEntryId.UlduarTeleporter);

            _ulduarTeleport.DefaultGossip = new GossipMenu(1,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTiare),
                    convo => convo.Speaker.SpellCast.Trigger(SpellId.AntechamberTeleport, convo.Character))
                );

            __doodadUlIniverseGlobe01 = GOMgr.GetEntry(GOEntryId.Doodad_UL_UniverseGlobe01);
        }

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			AlgalonTheObserver = NPCMgr.GetEntry(NPCId.AlgalonTheObserver);
            AlgalonTheObserver.BrainCreator = algalon => new AlgalonTheObserverBrain(algalon);
            AlgalonTheObserver.Activated += algalon =>
            {
                ((BaseBrain)algalon.Brain).DefaultCombatAction.Strategy = new AlgalonTheObserverAIAttack(algalon);
            };
		}


		public class AlgalonTheObserverBrain : MobBrain
        {
            #region Text constant
            private const string TEXT_AGGRO = "Your actions are illogical. All possible results for this encounter have been calculated. The pantheon will receive the observer's message regardless outcome.";
            private const string TEXT_DEATH = "I've seen worlds bathed in The Makers flames. Their their denizens fading without so much as whimper. Entire planetary systems born and raised in the time it takes your mortal hearts to beat once. Yet all throughout, my own heart, devoid of emotion of empathy, I have felt.... NOTHING. A million, million lives wasted. Had they all held within them your tenacity, had they all loved life as you do...";
            #endregion

            #region Sound constant
            private const int SOUND_AGGRO = 15386;
            private const int SOUND_DEATH = 15393;
            #endregion

            [Initialization(InitializationPass.Second)]
            public static void InitAlgalonTheObserver()
            {

            }

            public AlgalonTheObserverBrain(NPC algalon)
				:base(algalon)
            {

            }

            public override void OnEnterCombat()
            {
                m_owner.Yell(TEXT_AGGRO);
                m_owner.PlaySound((int)SOUND_AGGRO);
                __doodadUlIniverseGlobe01.Activated += dood =>
                {
                   
                }; 
                base.OnEnterCombat();
            }

            

            public override void OnDeath()
            {
                m_owner.Yell(TEXT_DEATH);
                m_owner.PlaySound(SOUND_DEATH);
                Stop();
                base.OnDeath();
            }

        }

        public class AlgalonTheObserverAIAttack : AIAttackAction
        {
            public AlgalonTheObserverAIAttack(NPC algalon)
                : base(algalon)
            {

            }

            private static Spell CosmicSmash, PhasePunch;

            private static bool isCosmicSmash;

            private DateTime timeSinceLastInterval;
            private static int interval = 15;

            [Initialization(InitializationPass.Second)]
            public static void InitAlgalonTheObserver()
            {
                CosmicSmash = SpellHandler.Get(SpellId.CosmicSmash);
                PhasePunch = SpellHandler.Get(SpellId.PhasePunch);
            }

            public override void Start()
            {
                timeSinceLastInterval = DateTime.Now;

                isCosmicSmash = false;

                base.Start();
            }
            public override void Update()
            {
                var timeNow = DateTime.Now;
                var timeBetween = timeNow - timeSinceLastInterval;

                if (timeBetween.TotalSeconds >= interval)
                {
                    timeSinceLastInterval = timeNow;
                    CheckSpellCast();
                    CheckHealth();
                }

                base.Update();
            }

            public override void Stop()
            {
                
                base.Stop();
            }

            public void CheckSpellCast()
            {
                m_owner.SpellCast.Start(PhasePunch, false);
                
            }

            public void CheckHealth()
            {
                

            }
        }

	}

}