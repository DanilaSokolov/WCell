using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Skills;
using WCell.RealmServer.Database;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Tools.Code;
using WCell.Util;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Tools.Domi;
using WCell.Util.Toolshed;
using NLog;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;

namespace WCell.Tools.Spells
{
	public class SpellLineWriter
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static CodeFileWriter writer;
		public static Dictionary<string, HashSet<Spell>>[] Maps
		{
			get;
			private set;
		}

		private static HashSet<SkillId> LineSkills = new HashSet<SkillId>();

		[Tool]
		public static void WriteSpellLines()
		{
			WriteSpellLines(ToolConfig.RealmServerRoot + "Spells/SpellLines.Def.cs");
		}

		public static void WriteSpellLines(string outputFileName)
		{
			WCellEnumWriter.WriteSpellLinesEnum();

			using (writer = new CodeFileWriter(outputFileName, "WCell.RealmServer.Spells", "SpellLines", "static partial class", "",
				"WCell.Constants",
				"WCell.Constants.Spells"))
			{
				writer.WriteMethod("private", "static void", "SetupSpellLines", "", WriteSpellLinesMethod);
			}
		}

		private static void WriteSpellLinesMethod()
		{
			writer.WriteLine("SpellLine[] lines;");
			writer.WriteLine();

			for (var i = 0; i < Maps.Length; i++)
			{
				var map = Maps[i];
				var clss = (ClassId)i;

				if (map != null)
				{
					var listCount = map.Count;
					var s = 0;

					writer.WriteRegion((clss != 0 ? clss.ToString() : "Other") + " (" + listCount + ")");
					writer.WriteLine("lines = new SpellLine[]");
					writer.OpenBracket();

					foreach (var set in map.Values)
					{
						var spells = set.ToList();
						var lineName = GetSpellLineName(spells.First());

						spells.Sort((a, b) => a.Id.CompareTo(b.Id));		// first sort by rank and then by id
						spells.Sort((a, b) => a.Rank.CompareTo(b.Rank));

						writer.WriteLine("new SpellLine(SpellLineId." + lineName + ", ");
						writer.IndentLevel++;
						var j = 0;
						foreach (var spell in spells)
						{
							j++;
							writer.WriteIndent("SpellHandler.Get(SpellId." + spell.SpellId + ")");
							if (j < set.Count)
							{
								writer.WriteLine(",");
							}
						}
						writer.IndentLevel--;
						writer.Write(")");
						if (s < listCount - 1)
						{
							writer.WriteLine(",");
						}
						++s;
					}
					writer.CloseBracket(true);
					writer.WriteLine("AddSpellLines(ClassId.{0}, lines);", clss);
					writer.WriteEndRegion();
					writer.WriteLine();
				}
			}
		}

		public static string GetSpellLineName(Spell spell)
		{
			var name = spell.Name;
			
			if (spell.Skill != null && spell.Talent != null)
			{
				name = spell.Skill.Name + name;
			}

			var clss = spell.ClassId;
			if (clss == 0)
			{
				var ids = spell.Ability.ClassMask.GetIds();
				if (ids.Length == 1)
				{
					clss = ids.First();
				}
			}
			if (clss == ClassId.PetTalents && spell.Talent != null)
			{
				name = "HunterPet" + name;
			}
			else
			{
				name = spell.ClassId + name;
			}

			return WCellEnumWriter.BeautifyName(name);
		}

		public static void CreateMaps()
		{
			if (Maps != null)
			{
				return;
			}

			RealmDBUtil.Initialize();
			ContentHandler.Initialize();
			SpellHandler.LoadSpells();
			SpellHandler.Initialize2();

			World.InitializeWorld();
			World.LoadDefaultRegions();
			ArchetypeMgr.EnsureInitialize();		// for default spells

			NPCMgr.LoadNPCDefs();					// for trainers

			Maps = new Dictionary<string, HashSet<Spell>>[(int)ClassId.End];

			FindTalents();
			FindAbilities();

			foreach (var spell in SpellHandler.ById)
			{
				if (spell != null && spell.Skill != null && LineSkills.Contains(spell.Skill.Id))
				{
					AddSpell(spell, true);
				}
			}
		}

		/// <summary>
		/// Find all class abilities that Trainers have to offer
		/// </summary>
		private static void FindAbilities()
		{
			// Add initial abilities
			foreach (var arr in ArchetypeMgr.Archetypes)
			{
				if (arr != null)
				{
					foreach (var archetype in arr)
					{
						if (archetype == null)
						{
							continue;
						}

						foreach (var spell in archetype.Spells)
						{
							AddSpell(spell);
						}
					}
				}
			}

			// Add trainable abilities
			foreach (var npc in NPCMgr.GetAllEntries())
			{
				if (npc.TrainerEntry != null)
				{
					foreach (var spellEntry in npc.TrainerEntry.Spells.Values)
					{
						var spell = spellEntry.Spell;
						if (spell.Ability != null && spell.Skill.Category == SkillCategory.ClassSkill)
						{
							AddSpell(spell);
						}
					}
				}
			}

			foreach (var npc in NPCMgr.GetAllEntries())
			{
				if (npc.TrainerEntry != null)
				{
					foreach (var spellEntry in npc.TrainerEntry.Spells.Values)
					{
						var spell = spellEntry.Spell;
						if (spell.Ability != null && spell.Skill.Category == SkillCategory.ClassSkill)
						{
							AddSpell(spell);
						}
					}
				}
			}
		}

		/// <summary>
		/// Add all Talents
		/// </summary>
		private static void FindTalents()
		{
			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null ||
					((spell.Talent == null || spell.ClassId == 0) && (spell.Skill == null || spell.Rank == 0 || spell.Skill.Category != SkillCategory.Profession)) ||
					spell.IsTriggeredSpell ||
					spell.HasEffectWith(effect => effect.EffectType == SpellEffectType.LearnPetSpell || effect.EffectType == SpellEffectType.LearnSpell))
				{
					continue;
				}

				var clss = spell.ClassId;
				if (spell.Ability == null || spell.Ability.ClassMask == 0 || spell.Ability.ClassMask.HasAnyFlag(clss))
				{
					//if (spell.SpellId.ToString().Contains("_"))
					//{
					//    log.Warn("Duplicate: " + spell);
					//    continue;
					//}
					if (string.IsNullOrEmpty(spell.Description))
					{
						continue;
					}

					AddSpell(spell);
				}
			}
		}

		private static void AddSpell(Spell spell, bool force = false)
		{
			if (!force)
			{
				if (spell.Skill == null ||
				    (spell.Skill.Category != SkillCategory.ClassSkill && spell.Skill.Category != SkillCategory.Invalid))
				{
					return;
				}

				LineSkills.Add(spell.Skill.Id);
			}

			var clss = spell.ClassId;
			if (clss == 0)
			{
				var ids = spell.Ability.ClassMask.GetIds();
				if (ids.Length == 1)
				{
					clss = ids.First();
				}
				else
				{
					// not a single class spell
					return;
				}
			}
			if (clss == ClassId.PetTalents && spell.Talent != null)
			{
				clss = ClassId.Hunter;
			}

			var name = GetSpellLineName(spell);
			var map = Maps[(int)clss];
			if (map == null)
			{
				Maps[(int)clss] = map = new Dictionary<string, HashSet<Spell>>(100);
			}
			var line = map.GetOrCreate(name);

			if (spell.IsTriggeredSpell && line.Count > 0 && !line.Any(spll => spll.IsTriggeredSpell))
			{
				// This one is not part of the group
				return;
			}

			line.Add(spell);
		}
	}
}