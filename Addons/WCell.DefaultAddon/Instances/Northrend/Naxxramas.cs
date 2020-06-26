using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 6/11/2009
///

namespace WCell.Addons.Default.Instances
{
	public class Naxxramas : BaseInstance
	{

		private static NPCEntry AnubrekanEntry;
        private static NPCEntry GrobbulusEntry;

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
            InitAnubrekan();
            InitGrobbulus();
		}

        #region Anubrekan Brain and AIAttack

        private static void InitAnubrekan()
        {
            AnubrekanEntry = NPCMgr.GetEntry(NPCId.AnubRekhan);
            AnubrekanEntry.BrainCreator = anubrekan => new AnubrekanBrain(anubrekan);
            AnubrekanEntry.Activated += anubrekan =>
            {
                ((BaseBrain)anubrekan.Brain).DefaultCombatAction.Strategy = new AnubrekanAIAttack(anubrekan);
            };
        }

        public class AnubrekanBrain : MobBrain
        {
            #region Text constant

            #endregion

            #region Sound constant
            private const int SOUND_AGRO_1 = 8785;
            private const int SOUND_AGRO_2 = 8786;
            private const int SOUND_AGRO_3 = 8787;
            #endregion

            [Initialization(InitializationPass.Second)]
            public static void InitAnubrekan()
            {

            }

            public AnubrekanBrain(NPC anubrekan)
                : base(anubrekan)
            {

            }

            public override void OnEnterCombat()
            {
                m_owner.PlaySound(SOUND_AGRO_1);
                base.OnEnterCombat();
            }



            public override void OnDeath()
            {
               
                base.OnDeath();
            }

        }

        public class AnubrekanAIAttack : AIAttackAction
        {
            public AnubrekanAIAttack(NPC anubrekan)
                : base(anubrekan)
            {

            }         

            [Initialization(InitializationPass.Second)]
            public static void InitAnubrekan()
            {
                
            }

            public override void Start()
            {
               

                base.Start();
            }
            public override void Update()
            {
               

                base.Update();
            }

            public override void Stop()
            {

                base.Stop();
            }         
        }
        #endregion

        #region Grobbulus Brain and AIAttack

        private static void InitGrobbulus()
        {
            GrobbulusEntry = NPCMgr.GetEntry(NPCId.AnubRekhan);
            GrobbulusEntry.BrainCreator = grobbulus => new GrobbulusBrain(grobbulus);
            GrobbulusEntry.Activated += grobbulus =>
            {
                ((BaseBrain)grobbulus.Brain).DefaultCombatAction.Strategy = new GrobbulusAIAttack(grobbulus);
            };
        }

        public class GrobbulusBrain : MobBrain
        {
            #region Text constant

            #endregion

            #region Sound constant
            private const int SOUND_AGRO_1 = 8708;
            private const int SOUND_WOUND = 8709;
            private const int SOUND_WOUND_CRIT = 8710;
            private const int SOUND_DEATH = 8711;
            #endregion

            [Initialization(InitializationPass.Second)]
            public static void InitGrobbulus()
            {

            }

            public GrobbulusBrain(NPC grobbulus)
                : base(grobbulus)
            {

            }

            public override void OnEnterCombat()
            {
                m_owner.PlaySound(SOUND_AGRO_1);
                base.OnEnterCombat();
            }

            public override void OnDeath()
            {
                m_owner.PlaySound(SOUND_DEATH);
                base.OnDeath();
            }
        }

        public class GrobbulusAIAttack : AIAttackAction
        {
            public GrobbulusAIAttack(NPC grobbulus)
                : base(grobbulus)
            {

            }

            [Initialization(InitializationPass.Second)]
            public static void InitGrobbulus()
            {

            }

            public override void Start()
            {

                base.Start();
            }
            public override void Update()
            {

                base.Update();
            }

            public override void Stop()
            {

                base.Stop();
            }
        }
        #endregion
    }
}