using System;
using System.IO;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima mobile generator.
	/// </summary>
	public class UltimaMobileGenerator
	{
		#region Methods
		/// <summary>
		/// Generates class and saves it to stream.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="item">ItemID of item to generate.</param>
		/// <param name="hue">Hue of item to generate.</param>
		/// <param name="amount">Stackable amount of item to generate.</param>
		/// <param name="properties">Item properties packet.</param>
		public static void Generate( Stream stream, MobileIncommingPacket mobile, MobileNamePacket name = null )
		{
			using ( UltimaClassWriter writer = new UltimaClassWriter( stream ) )
			{
				writer.WriteUsing( "System" );
				writer.WriteUsing( "Server" );
				writer.WriteUsing( "Server.Items" );
				writer.WriteUsing( "Server.Mobiles" );
				writer.WriteLine();

				writer.BeginNamespace( "Server.Mobiles" );

				string className = "GenericMobile";
				string baseClass = "BaseCreature";
				string baseClassParameters = String.Format( "AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4" );
				bool isVendor = false;

				if ( name != null )
					className = UltimaClassWriter.BuildClassName( name.MobileName );

				if ( mobile.HasYellowHealthBar )
				{
					// Most likely
					baseClass = "BaseVendor";

					if ( name != null )
						baseClassParameters = String.Format( "\"{0}\"", name.MobileName );
					else
						baseClassParameters = String.Format( "\"{0}\"", className );

					isVendor = true;
				}

				if ( name != null )
					writer.WriteLineWithIndent( "[CorpseName( \"{0}\" corpse )]", name.MobileName.ToLower() );
				else
					writer.WriteLineWithIndent( "[CorpseName( \"{0}\" corpse )]", className );

				writer.BeginClass( className, baseClass );
				writer.WriteLineWithIndent( "[Constructable]" );
				writer.BeginConstructor( "public", className, baseClassParameters );

				if ( !isVendor )
				{
					if ( name != null )
						writer.WriteLineWithIndent( "Name = \"{0}\";", name.MobileName );
					else
						writer.WriteLineWithIndent( "Name = \"{0}\";", className );

					writer.WriteLineWithIndent( "Body = 0x{0:X};", mobile.Body );
					writer.WriteLineWithIndent( "BaseSoundID = 0; // TODO" );

					if ( mobile.IsFemale )
						writer.WriteLineWithIndent( "Female = true;" );

					writer.WriteLine();
					writer.WriteLineWithIndent( "SetDamageType( ResistanceType.Physical, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetDamageType( ResistanceType.Fire, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetDamageType( ResistanceType.Cold, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetDamageType( ResistanceType.Poison, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetDamageType( ResistanceType.Energy, 25 ); // TODO " );
					writer.WriteLine();
					writer.WriteLineWithIndent( "SetResistance( ResistanceType.Physical, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetResistance( ResistanceType.Fire, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetResistance( ResistanceType.Cold, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetResistance( ResistanceType.Poison, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetResistance( ResistanceType.Energy, 25 ); // TODO " );
					writer.WriteLine();
					writer.WriteLineWithIndent( "SetSkill( SkillName.MagicResist, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetSkill( SkillName.Tactics, 25 ); // TODO " );
					writer.WriteLineWithIndent( "SetSkill( SkillName.Wrestling, 25 ); // TODO " );
					writer.WriteLine();
					writer.WriteLineWithIndent( "Fame = 0; // TODO" );
					writer.WriteLineWithIndent( "Karma = 0; // TODO" );
					writer.WriteLineWithIndent( "VirtualArmor = 0; // TODO" );
				}
				else
				{
					if ( mobile.IsFemale )
						writer.WriteLineWithIndent( "Female = true;" );
				}
				writer.WriteLine();

				// Items
				UltimaItemDefinitions itemDefinitions = Globals.Instance.ItemDefinitions;

				if ( itemDefinitions != null )
				{
					bool hasItemDeclaration = false;

					foreach ( MobileItem item in mobile.Items )
					{
						if ( itemDefinitions.Items.ContainsKey( item.ItemID ) )
						{
							UltimaItemDefinition itemDefinition = itemDefinitions.Items[ item.ItemID ];

							if ( !hasItemDeclaration )
							{
								writer.WriteLineWithIndent( "Item item = null;" );
								writer.WriteLine();
								hasItemDeclaration = true;
							}

							writer.WriteLineWithIndent( "item = new {0}();", itemDefinition.Class );

							if ( item.Hue > 0 )
								writer.WriteLineWithIndent( "item.Hue = 0x{0:X};", item.Hue );

							writer.WriteLineWithIndent( "AddItem( item );" );
							writer.WriteLine();
						}
						else
							App.Window.ShowNotification( NotificationType.Warning, String.Format( "Cannot find definition for item ID '0x{0:X}'. Skipping", item.ItemID ) );
					}
				}
				else
					App.Window.ShowNotification( NotificationType.Warning, "Item definitions not initialized. Skipping mobile items" );

				writer.EndConstructor();
				writer.WriteSerialConstructor( className );
				writer.WriteLine();
				writer.BeginOverrideMethod( "public", "void", "GenerateLoot" );
				writer.WriteLineWithIndent( "AddLoot( LootPack.Average );" );
				writer.EndMethod();
				writer.WriteSerialize();
				writer.WriteLine();
				writer.WriteDeserialize();
				writer.EndClass();
				writer.EndClass();
				writer.EndNamespace();

				App.Window.ShowNotification( NotificationType.Info, "Mobile generation complete" );
			}
		}
		#endregion
	}
}
