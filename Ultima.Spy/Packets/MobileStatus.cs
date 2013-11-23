using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	public enum MobileRace
	{
		Human		= 1,
		Elf			= 2,
		Gargoyle	= 3,
	}

	[UltimaPacket( "Mobile Status", UltimaPacketDirection.FromServer, 0x11 )]
	public class MobileStatusPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private string _MobileName;

		[UltimaPacketProperty( "Name" )]
		public string MobileName
		{
			get { return _MobileName; }
		}

		private bool _AllowNameChange;

		[UltimaPacketProperty( "Allow Name Change" )]
		public bool AllowNameChange
		{
			get { return _AllowNameChange; }
		}

		private bool _IsFemale;

		[UltimaPacketProperty( "Female" )]
		public bool IsFemale
		{
			get { return _IsFemale; }
		}

		private int _Strength;

		[UltimaPacketProperty]
		public int Strength
		{
			get { return _Strength; }
		}

		private int _HitPoints;

		[UltimaPacketProperty( "Hit Points" )]
		public int HitPoints
		{
			get { return _HitPoints; }
		}

		private int _MaxHitPoints;

		[UltimaPacketProperty( "Max. Hit Points" )]
		public int MaxHitPoints
		{
			get { return _MaxHitPoints; }
		}

		private int _Dexterity;

		[UltimaPacketProperty]
		public int Dexterity
		{
			get { return _Dexterity; }
		}

		private int _Stamina;

		[UltimaPacketProperty]
		public int Stamina
		{
			get { return _Stamina; }
		}

		private int _MaxStamina;

		[UltimaPacketProperty( "Max. Stamina" )]
		public int MaxStamina
		{
			get { return _MaxStamina; }
		}

		private int _Intelligence;

		[UltimaPacketProperty( "Intelligence" )]
		public int Intelligence
		{
			get { return _Intelligence; }
		}

		private int _Mana;

		[UltimaPacketProperty]
		public int Mana
		{
			get { return _Mana; }
		}

		private int _MaxMana;

		[UltimaPacketProperty( "Max. Mana" )]
		public int MaxMana
		{
			get { return _MaxMana; }
		}

		private int _Gold;

		[UltimaPacketProperty]
		public int Gold
		{
			get { return _Gold; }
		}

		private int _ArmorRating;

		[UltimaPacketProperty( "Armor Rating" )]
		public int ArmorRating
		{
			get { return _ArmorRating; }
		}

		private int _Weight;

		[UltimaPacketProperty]
		public int Weight
		{
			get { return _Weight; }
		}

		private int _MaxWeight;

		[UltimaPacketProperty( "Max. Weight" )]
		public int MaxWeight
		{
			get { return _MaxWeight; }
		}

		private MobileRace _Race;

		[UltimaPacketProperty( "Race", "{0:D} - {0}" )]
		public MobileRace Race
		{
			get { return _Race; }
		}

		private int _StatCap;

		[UltimaPacketProperty( "Stat Cap" )]
		public int StatCap
		{
			get { return _StatCap; }
		}

		private int _Followers;

		[UltimaPacketProperty]
		public int Followers
		{
			get { return _Followers; }
		}

		private int _MaxFollowers;

		[UltimaPacketProperty( "Max. Followers" )]
		public int MaxFollowers
		{
			get { return _MaxFollowers; }
		}

		private int _FireResistance;

		[UltimaPacketProperty( "Fire Resistance" )]
		public int FireResistance
		{
			get { return _FireResistance; }
		}

		private int _ColdResistance;

		[UltimaPacketProperty( "Cold Resistance" )]
		public int ColdResistance
		{
			get { return _ColdResistance; }
		}

		private int _PoisonResistance;

		[UltimaPacketProperty( "Poison Resistance" )]
		public int PoisonResistance
		{
			get { return _PoisonResistance; }
		}

		private int _EnergyResistance;

		[UltimaPacketProperty( "Energy Resistance" )]
		public int EnergyResistance
		{
			get { return _EnergyResistance; }
		}

		private int _Luck;

		[UltimaPacketProperty]
		public int Luck
		{
			get { return _Luck; }
		}

		private int _MinWeaponDamage;

		[UltimaPacketProperty( "Min. Weapon Damage" )]
		public int MinWeaponDamage
		{
			get { return _MinWeaponDamage; }
		}

		private int _MaxWeaponDamage;

		[UltimaPacketProperty( "Max. Weapon Damage" )]
		public int MaxWeaponDamage
		{
			get { return _MaxWeaponDamage; }
		}

		private int _TithingPoints;

		[UltimaPacketProperty( "Tithing Points" )]
		public int TithingPoints
		{
			get { return _TithingPoints; }
		}

		private int _HitChanceIncrease;

		[UltimaPacketProperty( "Hit Chance Increase" )]
		public int HitChanceIncrease
		{
			get { return _HitChanceIncrease; }
		}

		private int _SwingSpeedIncrease;

		[UltimaPacketProperty( "Swing Speed Increase" )]
		public int SwingSpeedIncrease
		{
			get { return _SwingSpeedIncrease; }
		}

		private int _DamageChanceIncrease;

		[UltimaPacketProperty( "Damage Chance Increase" )]
		public int DamageChanceIncrease
		{
			get { return _DamageChanceIncrease; }
		}

		private int _LowerReagentCost;

		[UltimaPacketProperty( "Lower Reagent Cost" )]
		public int LowerReagentCost
		{
			get { return _LowerReagentCost; }
		}

		private int _HitPointRegeneration;

		[UltimaPacketProperty( "Hit Point Regeneration" )]
		public int HitPointRegeneration
		{
			get { return _HitPointRegeneration; }
		}

		private int _StaminaRegeneration;

		[UltimaPacketProperty( "Stamina Regeneration" )]
		public int StaminaRegeneration
		{
			get { return _StaminaRegeneration; }
		}

		private int _ManaRegeneration;

		[UltimaPacketProperty( "Mana Regeneration" )]
		public int ManaRegeneration
		{
			get { return _ManaRegeneration; }
		}

		private int _ReflectPhysicalDamage;

		[UltimaPacketProperty( "Reflect Physical Damage" )]
		public int ReflectPhysicalDamage
		{
			get { return _ReflectPhysicalDamage; }
		}

		private int _EnhancePotions;

		[UltimaPacketProperty( "Enhance Potions" )]
		public int EnhancePotions
		{
			get { return _EnhancePotions; }
		}

		private int _DefenseChanceIncrease;

		[UltimaPacketProperty( "Defense Chance Increase" )]
		public int DefenseChanceIncrease
		{
			get { return _DefenseChanceIncrease; }
		}

		private int _SpellDamageIncrease;

		[UltimaPacketProperty( "Spell Damage Increase " )]
		public int SpellDamageIncrease
		{
			get { return _SpellDamageIncrease; }
		}

		private int _FasterCastRecovery;

		[UltimaPacketProperty( "Faster Cast Recovery" )]
		public int FasterCastRecovery
		{
			get { return _FasterCastRecovery; }
		}

		private int _FasterCasting;

		[UltimaPacketProperty( "Faster Casting" )]
		public int FasterCasting
		{
			get { return _FasterCasting; }
		}

		private int _LowerManaCost;

		[UltimaPacketProperty( "Lower Mana Cost" )]
		public int LowerManaCost
		{
			get { return _LowerManaCost; }
		}

		private int _StrengthIncrease;

		[UltimaPacketProperty( "Strength Increase" )]
		public int StrengthIncrease
		{
			get { return _StrengthIncrease; }
		}

		private int _DexterityIncrease;

		[UltimaPacketProperty( "Dexterity Increase" )]
		public int DexterityIncrease
		{
			get { return _DexterityIncrease; }
		}

		private int _IntelligenceIncrease;

		[UltimaPacketProperty( "Intelligence Increase" )]
		public int IntelligenceIncrease
		{
			get { return _IntelligenceIncrease; }
		}

		private int _HitPointsIncrease;

		[UltimaPacketProperty( "Hit Points Increase" )]
		public int HitPointsIncrease
		{
			get { return _HitPointsIncrease; }
		}

		private int _StaminaIncrease;

		[UltimaPacketProperty( "Stamina Increase" )]
		public int StaminaIncrease
		{
			get { return _StaminaIncrease; }
		}

		private int _ManaIncrease;

		[UltimaPacketProperty( "Mana Increase" )]
		public int ManaIncrease
		{
			get { return _ManaIncrease; }
		}

		private int _MaximumHitPointsIncrease;

		[UltimaPacketProperty( "Maximum Hit Points Increase" )]
		public int MaximumHitPointsIncrease
		{
			get { return _MaximumHitPointsIncrease; }
		}

		private int _MaximumStaminaIncrease;

		[UltimaPacketProperty( "Maximum Stamina Increase" )]
		public int MaximumStaminaIncrease
		{
			get { return _MaximumStaminaIncrease; }
		}

		private int _MaximumManaIncrease;

		[UltimaPacketProperty( "Maximum Mana Increase" )]
		public int MaximumManaIncrease
		{
			get { return _MaximumManaIncrease; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_MobileName = reader.ReadAsciiString( 30 );

			_HitPoints = reader.ReadInt16();
			_MaxHitPoints = reader.ReadInt16();
			_AllowNameChange = reader.ReadByte() == 1 ? true : false;
			byte features = reader.ReadByte();
			_IsFemale = reader.ReadByte() == 1 ? true : false;
			_Strength = reader.ReadInt16();
			_Dexterity = reader.ReadInt16();
			_Intelligence = reader.ReadInt16();
			_Stamina = reader.ReadInt16();
			_MaxStamina = reader.ReadInt16();
			_Mana = reader.ReadInt16();
			_MaxMana = reader.ReadInt16();
			_Gold = reader.ReadInt32();
			_ArmorRating = reader.ReadInt16();
			_Weight = reader.ReadInt16();

			if ( ( features & 0x5 ) == 0x5 )
			{
				_MaxWeight = reader.ReadInt16();
				_Race = (MobileRace) reader.ReadByte();
			}

			if ( ( features & 0x2 ) == 0x2 )
			{
				_StatCap = reader.ReadInt16();
			}

			if ( ( features & 0x3 ) == 0x3 )
			{
				_Followers = reader.ReadByte();
				_MaxFollowers = reader.ReadByte();
			}

			if ( ( features & 0x4 ) == 0x4 )
			{
				_FireResistance = reader.ReadInt16();
				_ColdResistance = reader.ReadInt16();
				_PoisonResistance = reader.ReadInt16();
				_EnergyResistance = reader.ReadInt16();
				_Luck = reader.ReadInt16();
				_MinWeaponDamage = reader.ReadInt16();
				_MaxWeaponDamage = reader.ReadInt16();
				_TithingPoints = reader.ReadInt32();
			}

			if ( ( features & 0x6 ) == 0x6 )
			{
				_HitChanceIncrease = reader.ReadInt16();
				_SwingSpeedIncrease = reader.ReadInt16();
				_DamageChanceIncrease = reader.ReadInt16();
				_LowerReagentCost = reader.ReadInt16();
				_HitPointRegeneration = reader.ReadInt16();
				_StaminaRegeneration = reader.ReadInt16();
				_ManaRegeneration = reader.ReadInt16();
				_ReflectPhysicalDamage = reader.ReadInt16();
				_EnhancePotions = reader.ReadInt16();
				_DefenseChanceIncrease = reader.ReadInt16();
				_SpellDamageIncrease = reader.ReadInt16();
				_FasterCastRecovery = reader.ReadInt16();
				_FasterCasting = reader.ReadInt16();
				_LowerManaCost = reader.ReadInt16();
				_StrengthIncrease = reader.ReadInt16();
				_DexterityIncrease = reader.ReadInt16();
				_IntelligenceIncrease = reader.ReadInt16();
				_HitPointsIncrease = reader.ReadInt16();
				_StaminaIncrease = reader.ReadInt16();
				_ManaIncrease = reader.ReadInt16();
				_MaximumHitPointsIncrease = reader.ReadInt16();
				_MaximumStaminaIncrease = reader.ReadInt16();
				_MaximumManaIncrease = reader.ReadInt16();
			}
		}
	}
}
