using System;
using System.Security.Cryptography;
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
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Instances
{

    public class IcecrownCitadel : BaseInstance
    {
        #region Setup Content
        private static NPCEntry MarrowgarEntry;
        private static NPCEntry LadyDeathWhisperEntry;
        private static NPCEntry DeathbringerSaurfangEntry;
        private static NPCEntry FestergutEntry;
        private static NPCEntry RotfaceEntry;
        private static NPCEntry ProfessorPutricideEntry;
        private static NPCEntry BloodQueenLanathelEntry;
        private static NPCEntry ValithriaDreamwalkerEntry;
        private static NPCEntry SindragosaEntry;
        private static NPCEntry tirionForndringEntry;
        private static NPCEntry TheLichKingEntry;

        private static NPCEntry _muradinBranzaborod;

        private static GOEntry scourgeTransporter;

        public static GOEntry iceWall;
        public static NPCEntry skyBreaker;
      
        #region 
        public static Vector3[] MuradinExitPath =
       {
            new Vector3(8.130936f, -0.2699585f, 20.31728f ),
            new Vector3(6.380936f, -0.2699585f, 20.31728f ),
            new Vector3(3.507703f, 0.02986573f, 20.78463f ),
            new Vector3(-2.767633f, 3.743143f, 20.37663f ),
            new Vector3(-4.017633f, 4.493143f, 20.12663f ),
            new Vector3(-7.242224f, 6.856013f, 20.03468f ),
            new Vector3(-7.742224f, 8.606013f, 20.78468f ),
            new Vector3(-7.992224f, 9.856013f, 21.28468f ),
            new Vector3(-12.24222f, 23.10601f, 21.28468f ),
            new Vector3(-14.88477f, 25.20844f, 21.59985f )
        };

        public static Vector3[] SaurfangExitPath =
        {
            new Vector3(30.43987f, 0.1475817f, 36.10674f ),
            new Vector3(21.36141f, -3.056458f, 35.42970f ),
            new Vector3(19.11141f, -3.806458f, 35.42970f ),
            new Vector3(19.01736f, -3.299440f, 35.39428f ),
            new Vector3(18.6747f, -5.862823f, 35.66611f ),
            new Vector3(18.6747f, -7.862823f, 35.66611f ),
            new Vector3(18.1747f, -17.36282f, 35.66611f ),
            new Vector3(18.1747f, -22.61282f, 35.66611f ),
            new Vector3(17.9247f, -24.36282f, 35.41611f ),
            new Vector3(17.9247f, -26.61282f, 35.66611f ),
            new Vector3(17.9247f, -27.86282f, 35.66611f ),
            new Vector3(17.9247f, -29.36282f, 35.66611f ),
            new Vector3(15.33203f, -30.42621f, 35.93796f )
        };
        #endregion

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void InitGOs()
        {
            scourgeTransporter = GOMgr.GetEntry(GOEntryId.ScourgeTransporter_3);
            scourgeTransporter.DefaultGossip = new GossipMenu(1,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTiare),
                    convo => convo.Speaker.SpellCast.Trigger(SpellId.UpperSpireTeleport, convo.Character))
                );

            iceWall = GOMgr.GetEntry(GOEntryId.IceWall_2);
                        

        }

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            MarrowgarEntry = NPCMgr.GetEntry(NPCId.LordMarrowgar);
            MarrowgarEntry.BrainCreator = marrowgar => new MarrowgarBrain(marrowgar);
            MarrowgarEntry.Activated += marrowgar =>
            {
                ((BaseBrain)marrowgar.Brain).DefaultCombatAction.Strategy = new MarrowgarAIAttackAction(marrowgar);
                iceWall.Activated += iw =>
                {
                    iw.State = GameObjectState.Disabled;
                };
            };

            skyBreaker = NPCMgr.GetEntry(NPCId.TheSkybreaker_2);

            LadyDeathWhisperEntry = NPCMgr.GetEntry(NPCId.LadyDeathwhisper);

            DeathbringerSaurfangEntry = NPCMgr.GetEntry(NPCId.DeathbringerSaurfang);

            FestergutEntry = NPCMgr.GetEntry(NPCId.Festergut);

            _muradinBranzaborod = NPCMgr.GetEntry(NPCId.MuradinBronzebeard_2);
            _muradinBranzaborod.DefaultGossip = new GossipMenu(1,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTiare),
                    convo => convo.Speaker.SpellCast.Trigger(SpellId.FrozenThroneTeleport, convo.Character))
                );


            RotfaceEntry = NPCMgr.GetEntry(NPCId.Rotface);

            ProfessorPutricideEntry = NPCMgr.GetEntry(NPCId.ProfessorPutricide);

            BloodQueenLanathelEntry = NPCMgr.GetEntry(NPCId.BloodQueenLanaThel_2);

            ValithriaDreamwalkerEntry = NPCMgr.GetEntry(NPCId.ValithriaDreamwalker);

            SindragosaEntry = NPCMgr.GetEntry(NPCId.Sindragosa_0);

            tirionForndringEntry = NPCMgr.GetEntry(NPCId.HighlordTirionFordring_13);
            tirionForndringEntry.DefaultGossip = new GossipMenu(1,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTiare),
                    convo => convo.Speaker.SpellCast.Trigger(SpellId.FrozenThroneTeleport, convo.Character))
                );

            TheLichKingEntry = NPCMgr.GetEntry(NPCId.TheLichKing_18);
        }
        #endregion
    }

    #region 10-Man Lord Marrowgar
    public class MarrowgarBrain : MobBrain
    {
        public MarrowgarBrain(NPC marrowgar) : base(marrowgar) { }

        public override void OnDeath()
        {
            base.OnDeath();
        }
    }

    public class MarrowgarAIAttackAction : AIAttackAction
    {
        public MarrowgarAIAttackAction(NPC marrowgar)
            : base(marrowgar)
        {
            fbasespeed = m_owner.RunSpeed;
        }

        // Spells
        private static Spell BoneSlice, BoneStorm, ColdFlame, ColdFlameBone;
        private int BoneSliceTick, BoneStormTick, BoneStormStopTick, BoneStormWarnTick, ColdFlameTick, ColdFlameBoneTick, BoneStormMove;

        private DateTime timeSinceLastInterval;

        private static float fbasespeed;
        //private static bool IntroDone;
        private static int interval = 1;

        private bool isBoneStorm;
        private static int boneLength;
        private bool bBoneSlice;
        private bool bBoneStormWarn;
        private bool BoneStormWarned;

        [Initialization(InitializationPass.Second)]
        public static void InitMarrowgar()
        {
            BoneSlice = SpellHandler.Get(SpellId.BoneSlice);
            BoneStorm = SpellHandler.Get(SpellId.BoneStorm);
            ColdFlame = SpellHandler.Get(SpellId.Coldflame_3);
            ColdFlameBone = SpellHandler.Get(SpellId.Coldflame_13);
            boneLength = Utility.Random(20, 30);
        }


        public override void Start()
        {
            m_owner.WalkSpeed = m_owner.RunSpeed;
            timeSinceLastInterval = DateTime.Now;

            BoneSliceTick = 0;
            BoneStormTick = 0;
            BoneStormStopTick = 0;
            BoneStormWarnTick = 0;
            ColdFlameTick = 0;
            ColdFlameBoneTick = 0;
            BoneStormMove = 0;

            isBoneStorm = false;
            bBoneStormWarn = false;
            BoneStormWarned = false;

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
            }
            base.Update();
        }

        public void CheckSpellCast()
        {
            if (isBoneStorm)
            {
                ColdFlameBoneTick++;
                BoneStormStopTick++;
                if (ColdFlameTick >= Utility.Random(10, 15))
                {
                    m_owner.SpellCast.Start(ColdFlameBone, false);
                    ColdFlameTick = 0;
                }

                if (BoneStormMove <= boneLength / 3)
                {
                    var target = m_owner.GetNearbyRandomHostileCharacter();
                    if (target != null)
                        m_owner.Movement.MoveTo(target.Position);
                }

                if (BoneStormStopTick >= boneLength + 0.001)
                {
                    isBoneStorm = false;
                    m_owner.RunSpeed = m_owner.RunSpeed / 3.0f;
                    m_owner.Movement.Stop();
                    m_owner.Movement.MoveTo(m_target.Position);
                    bBoneSlice = false;
                    bBoneStormWarn = true;
                    BoneStormWarned = false;
                }
            }
            else
            {
                if (bBoneStormWarn)
                    BoneStormWarnTick++;

                if (BoneStormWarnTick >= Utility.Random(35, 50))
                {
                    BoneStormWarned = true;
                    m_owner.SpellCast.Start(BoneStorm, false);
                }

                if (BoneStormWarned)
                    BoneStormTick++;

                if (BoneStormTick >= 3)
                {
                    var aura = m_owner.Auras[BoneStorm];

                    if (aura != null)
                        aura.TimeLeft = boneLength * 1000;

                    m_owner.RunSpeed = m_owner.RunSpeed * 3.0f;
                    isBoneStorm = true;
                    BoneStormTick = 0;
                }

                if (!bBoneSlice)
                    BoneSliceTick++;

                if (BoneSliceTick >= 10)
                {
                    BoneSliceTick = 0;
                    bBoneSlice = true;
                }
            }

            if (bBoneSlice && !BoneStormWarned)
                m_owner.SpellCast.Start(BoneSlice, false);
        }


    }
    #endregion
}