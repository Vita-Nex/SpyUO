using System;
using System.Collections.Generic;
using System.IO;
using Ultima.Package;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima item generator.
	/// </summary>
	public class UltimaItemGenerator
	{
		#region Properties
		private const int StackableCliloc = 1050039;
		private const int OldItemCliloc = 1020000;
		private const int NewItemCliloc = 1078872;

		private static readonly int[] StringClilocs = { 1042971, 1070722 };
		private static readonly int[] DamageClilocs = { 1060403, 1060405, 1060404, 1060406, 1060407, 1072846, 1079978 };
		private static readonly int[] ContainerClilocs = { 1073841, 1072241, 1050044 };
		#endregion

		#region Methods
		/// <summary>
		/// Generates class and saves it to stream.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="item">ItemID of item to generate.</param>
		/// <param name="hue">Hue of item to generate.</param>
		/// <param name="amount">Stackable amount of item to generate.</param>
		/// <param name="properties">Item properties packet.</param>
		public static void Generate( Stream stream, int itemID, int hue = 0, int amount = 0, QueryPropertiesResponsePacket properties = null )
		{
			UltimaItemDefinitions itemDefinitions = Globals.Instance.ItemDefinitions;

			if ( itemDefinitions == null )
			{
				App.Window.ShowNotification( NotificationType.Error, "Item definitions not initialized" );
				return;
			}

			UltimaItemProperties itemProperties = Globals.Instance.ItemProperties;

			if ( itemProperties == null )
			{
				App.Window.ShowNotification( NotificationType.Error, "Item properties not initialized" );
				return;
			}

			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			using ( UltimaClassWriter writer = new UltimaClassWriter( stream ) )
			{
				writer.WriteUsing( "System" );
				writer.WriteUsing( "Server" );
				writer.WriteUsing( "Server.Items" );
				writer.WriteLine();

				writer.BeginNamespace( "Server.Items" );

				// Get item definition
				UltimaItemDefinition itemDefinition = null;

				if ( itemDefinitions.Items.ContainsKey( itemID ) )
					itemDefinition = itemDefinitions.Items[ itemID ];

				string className = "GenericClass";
				string baseClass = null;
				int nameCliloc = 0;
				string nameClilocText = null;
				string nameText = null;
				bool stackable = false;

				if ( itemDefinition != null )
					baseClass = itemDefinition.Class;

				// Get class name
				if ( properties != null && properties.Properties.Count > 0 )
				{
					// Get name from name property
					QueryPropertiesProperty nameProperty = properties.Properties[ 0 ];

					if ( nameProperty.Cliloc == StackableCliloc )
					{
						// Get name from stackable cliloc
						UltimaClilocArgumentParser nameArguments = new UltimaClilocArgumentParser( nameProperty.Arguments );
						nameCliloc = nameArguments.GetCliloc( 0 );

						if ( nameCliloc > 0 )
						{
							if ( clilocs != null )
							{
								nameClilocText = clilocs.GetString( nameCliloc );

								if ( !String.IsNullOrEmpty( nameClilocText ) )
									className = UltimaClassWriter.BuildClassName( nameClilocText );
							}
						}
						else
						{
							nameText = nameArguments[ 0 ];

							if ( !String.IsNullOrEmpty( nameText ) )
								className = UltimaClassWriter.BuildClassName( nameText );
						}

						stackable = true;
					}
					else
					{
						// Get name from name cliloc
						nameCliloc = nameProperty.Cliloc;

						if ( !IsString( nameCliloc ) )
						{
							if ( clilocs != null )
							{
								nameClilocText = clilocs.GetString( nameCliloc );

								if ( !String.IsNullOrEmpty( nameClilocText ) )
									className = UltimaClassWriter.BuildClassName( nameClilocText );
							}
						}
						else
						{
							nameText = nameProperty.Arguments;

							if ( !String.IsNullOrEmpty( nameText ) )
								className = UltimaClassWriter.BuildClassName( nameText );
						}
					}
				}
				else if ( clilocs != null )
				{
					// Get name from itemID cliloc
					int itemIDCliloc = 0;

					if ( itemID < 0x4000 )
						itemIDCliloc = OldItemCliloc + itemID;
					else
						itemIDCliloc = NewItemCliloc + itemID;

					string clilocText = clilocs.GetString( itemIDCliloc );

					if ( !String.IsNullOrEmpty( clilocText ) )
						className = UltimaClassWriter.BuildClassName( clilocText );
				}

				// Check if container
				bool isContainer = false;

				if ( baseClass == null && properties != null && properties.Properties.Count > 0 )
				{
					for ( int i = 1; i < properties.Properties.Count; i++ )
					{
						QueryPropertiesProperty property = properties.Properties[ i ];

						if ( IsContainer( property.Cliloc ) )
						{
							baseClass = "BaseContainer";
							isContainer = true;
							break;
						}
					}
				}

				if ( baseClass == null )
					baseClass = "Item";

				writer.BeginClass( className, baseClass );

				// Name cliloc
				bool anyOverrides = false;

				if ( nameCliloc > 0 && IsSpecial( itemID, nameCliloc ) )
				{
					if ( !String.IsNullOrEmpty( nameClilocText ) )
						writer.OverrideProperty( "public", "int", "LabelNumber", nameCliloc.ToString(), nameClilocText );
					else
						writer.OverrideProperty( "public", "int", "LabelNumber", nameCliloc.ToString() );

					anyOverrides = true;
				}

				// Properties
				if ( properties != null && properties.Properties.Count > 0 )
				{
					Dictionary<int, UltimaItemProperty> constructorPropertyDefinitions = new Dictionary<int, UltimaItemProperty>();
					List<QueryPropertiesProperty> constructorProperties = new List<QueryPropertiesProperty>();
					List<QueryPropertiesProperty> unknownProperties = new List<QueryPropertiesProperty>();
					List<QueryPropertiesProperty> damageProperties = new List<QueryPropertiesProperty>();

					for ( int i = 1; i < properties.Properties.Count; i++ )
					{
						QueryPropertiesProperty property = properties.Properties[ i ];

						if ( IsDamage( property.Cliloc ) )
						{
							damageProperties.Add( property );
							continue;
						}

						UltimaItemProperty propertyDefinition = null;

						if ( itemDefinition != null )
						{
							Dictionary<int, UltimaItemProperty> groupProperties = null;
							UltimaItemDefinitionGroup group = itemDefinition.Parent;

							while ( group.Parent != null )
								group = group.Parent;

							if ( itemProperties.GroupProperties.TryGetValue( group.Name, out groupProperties ) )
							{
								if ( !groupProperties.TryGetValue( property.Cliloc, out propertyDefinition ) )
									itemProperties.Properties.TryGetValue( property.Cliloc, out propertyDefinition );
							}
							else
								itemProperties.Properties.TryGetValue( property.Cliloc, out propertyDefinition );
						}
						else
							itemProperties.Properties.TryGetValue( property.Cliloc, out propertyDefinition );

						if ( propertyDefinition != null )
						{
							UltimaClilocArgumentParser arguments = new UltimaClilocArgumentParser( property.Arguments );
							bool isConstructor = false;
							string propertyValue = null;

							if ( propertyDefinition.Switch != null )
								propertyDefinition.Switch.TryGetValue( property.Cliloc, out propertyValue );

							foreach ( UltimaItemPropertySetter setter in propertyDefinition.Setters )
							{
								if ( !isConstructor && !setter.Overrides )
								{
									constructorProperties.Add( property );
									constructorPropertyDefinitions.Add( property.Cliloc, propertyDefinition );
									isConstructor = true;
								}

								if ( setter.Overrides )
								{
									string argument = arguments[ setter.Index ];
									string value = propertyValue;

									if ( propertyValue == null )
										value = GetPropertyValue( property.Cliloc, argument, setter );

									writer.OverrideProperty( "public", setter.ReturnType, setter.Name, value );
									anyOverrides = true;
								}
							}
						}
						else
							unknownProperties.Add( property );
					}

					if ( anyOverrides )
						writer.WriteLine();

					writer.WriteLineWithIndent( "[Constructable]" );

					if ( isContainer )
						writer.BeginConstructor( "public", className, null, String.Format( "0x{0:X}", itemID ) );
					else
						writer.BeginConstructor( "public", className, null, "" );

					// Name
					if ( nameText != null )
						writer.WriteLineWithIndent( "Name = \"{0}\";", nameText );

					// ItemID
					if ( !isContainer && itemDefinition == null )
						writer.WriteLineWithIndent( "ItemID = 0x{0:X};", itemID );

					// Hue
					if ( hue > 0 )
						writer.WriteLineWithIndent( "Hue = 0x{0:X};", hue );

					// Stackable
					if ( stackable )
						writer.WriteLineWithIndent( "Stackable = true;" );

					if ( amount > 1 )
						writer.WriteLineWithIndent( "Amount = {0};", amount );

					// Constructor variables
					foreach ( QueryPropertiesProperty property in constructorProperties )
					{
						UltimaItemProperty propertyDefinition = constructorPropertyDefinitions[ property.Cliloc ];
						UltimaClilocArgumentParser arguments = new UltimaClilocArgumentParser( property.Arguments );
						bool isSlayerSet = false;
						string propertyValue = null;

						if ( propertyDefinition.Switch != null )
							propertyDefinition.Switch.TryGetValue( property.Cliloc, out propertyValue );

						foreach ( UltimaItemPropertySetter setter in propertyDefinition.Setters )
						{
							if ( !setter.Overrides )
							{
								string argument = arguments[ setter.Index ];
								string value = propertyValue;

								if ( value == null )
									value = GetPropertyValue( property.Cliloc, argument, setter );

								// Check if slayer 2
								string setterName = setter.Name;

								if ( isSlayerSet )
									setterName += "2";

								if ( setter.IsSlayer )
									isSlayerSet = true;

								// Write
								writer.WriteLineWithIndent( String.Format( "{0} = {1};", setterName, value ) );
							}
						}
					}

					// Write unknowns
					if ( unknownProperties.Count > 0 )
					{
						writer.WriteLine();
						writer.WriteLineWithIndent( "// Unknown properties" );

						foreach ( QueryPropertiesProperty property in unknownProperties )
						{
							string clilocText = null;
							string format = "Found unknown property '{0}' with parameters '{1}'";
							string notification = null;

							if ( clilocs != null )
								clilocText = GetPropertyDescription( property.Cliloc );

							if ( clilocText != null )
							{
								writer.WriteLineWithIndent( String.Format( "//{0} = {1}; // Cliloc: {2}", property.Cliloc, property.Arguments, clilocText ) );
								notification = String.Format( format, clilocText, property.Arguments );
							}
							else
							{
								writer.WriteLineWithIndent( String.Format( "//{0} = {1};", property.Cliloc, property.Arguments ) );
								notification = String.Format( format, property.Cliloc, property.Arguments );
							}

							App.Window.ShowNotification( NotificationType.Warning, notification );
						}
					}

					writer.EndConstructor();

					// Write damage overwrite
					if ( damageProperties.Count > 0 )
					{
						int physicalDamage = 0;
						int fireDamage = 0;
						int coldDamage = 0;
						int poisonDamage = 0;
						int energyDamage = 0;
						int chaosDamage = 0;
						int directDamage = 0;

						foreach ( QueryPropertiesProperty property in damageProperties )
						{
							UltimaClilocArgumentParser arguments = new UltimaClilocArgumentParser( property.Arguments );

							if ( arguments.Length > 0 )
							{
								int integer = 0;

								if ( Int32.TryParse( arguments[ 0 ], out integer ) )
								{
									if ( property.Cliloc == 1060403 )
										physicalDamage = integer;
									else if ( property.Cliloc == 1060405 )
										fireDamage = integer;
									else if ( property.Cliloc == 1060404 )
										coldDamage = integer;
									else if ( property.Cliloc == 1060406 )
										poisonDamage = integer;
									else if ( property.Cliloc == 1060407 )
										energyDamage = integer;
									else if ( property.Cliloc == 1072846 )
										chaosDamage = integer;
									else if ( property.Cliloc == 1079978 )
										directDamage = integer;
								}
							}
						}

						if ( physicalDamage != 100 )
						{
							writer.WriteLine();
							writer.BeginOverrideMethod( "public", "void", "GetDamageTypes", "Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct" );

							writer.WriteLineWithIndent( "phys = {0};", physicalDamage );
							writer.WriteLineWithIndent( "fire = {0};", fireDamage );
							writer.WriteLineWithIndent( "cold = {0};", coldDamage );
							writer.WriteLineWithIndent( "pois = {0};", poisonDamage );
							writer.WriteLineWithIndent( "nrgy = {0};", energyDamage );
							writer.WriteLineWithIndent( "chaos = {0};", chaosDamage );
							writer.WriteLineWithIndent( "direct = {0};", directDamage );

							writer.EndMethod();
						}
					}
				}
				else
				{
					writer.BeginConstructor( "public", className );

					// Name
					if ( nameText != null )
						writer.WriteLineWithIndent( "Name = \"{0}\";", nameText );

					// ItemID
					writer.WriteLineWithIndent( "ItemID = 0x{0:X};", itemID );

					// Hue
					if ( hue > 0 )
						writer.WriteLineWithIndent( "Hue = 0x{0:X};", hue );

					// Stackable
					if ( stackable )
						writer.WriteLineWithIndent( "Stackable = true;" );

					if ( amount > 1 )
						writer.WriteLineWithIndent( "Amount = {0};", amount );

					writer.EndConstructor();
				}

				writer.WriteLine();
				writer.WriteSerialConstructor( className );
				writer.WriteLine();
				writer.WriteSerialize();
				writer.WriteLine();
				writer.WriteDeserialize();
				writer.EndClass();
				writer.EndNamespace();

				App.Window.ShowNotification( NotificationType.Info, "Item generation complete" );
			}
		}

		private static string GetPropertyValue( int cliloc, string argument, UltimaItemPropertySetter setter )
		{
			string propertyDescription = GetPropertyDescription( cliloc );
			string value = null;

			if ( !String.IsNullOrEmpty( argument ) )
			{
				if ( setter.Switch != null )
				{
					int integer = 0;

					if ( Int32.TryParse( argument, out integer ) )
					{
						if ( !setter.Switch.TryGetValue( integer, out value ) )
						{
							string format = "Cannot find parameter '{0}' in switch setter at index '{1}' in property '{2}'. Using parameter for value.";
							string notification = String.Format( format, argument, setter.Index, propertyDescription );
							App.Window.ShowNotification( NotificationType.Warning, notification );
							value = argument;
						}
					}
					else
					{
						string format = "Switch setter with argument '{0}' at index '{1}' in property '{2}' is not integer. Using parameter for value.";
						string notification = String.Format( format, argument, setter.Index, propertyDescription );
						App.Window.ShowNotification( NotificationType.Warning, notification );
						value = argument;
					}
				}
				else
				{
					try
					{
						value = String.Format( setter.Format, argument );
					}
					catch
					{
						string format = "Error building setter value  at index '{0}' property '{1}' is not integer. Using format for value.";
						string notification = String.Format( format, setter.Index, propertyDescription );
						App.Window.ShowNotification( NotificationType.Warning, notification );
						value = setter.Format;
					}
				}
			}
			else
				value = "true"; // Assume boolean for properties without arguments.

			return value;
		}

		/// <summary>
		/// Determines whether item uses ItemID cliloc or special cliloc.
		/// </summary>
		/// <param name="itemID">ItemID.</param>
		/// <param name="nameCliloc">Name cliloc.</param>
		/// <returns>True if special, false otherwise.</returns>
		public static bool IsSpecial( int itemID, int nameCliloc )
		{
			int itemIDCliloc = 0;

			if ( itemID < 0x4000 )
				itemIDCliloc = OldItemCliloc + itemID;
			else
				itemIDCliloc = NewItemCliloc + itemID;

			if ( itemIDCliloc == nameCliloc )
				return false;

			return true;
		}

		/// <summary>
		/// Determines whether cliloc contains string only.
		/// </summary>
		/// <param name="cliloc">Cliloc to check.</param>
		/// <returns>True for string clilocs, false otherwise.</returns>
		public static bool IsString( int cliloc )
		{
			for ( int i = 0; i < StringClilocs.Length; i++ )
				if ( StringClilocs[ i ] == cliloc )
					return true;

			return false;
		}

		/// <summary>
		/// Determines whether cliloc is stackable.
		/// </summary>
		/// <param name="cliloc">Cliloc to check.</param>
		/// <returns>True for stackable clilocs, false otherwise.</returns>
		public static bool IsStackable( int cliloc )
		{
			return cliloc == StackableCliloc;
		}

		/// <summary>
		/// Determines whether cliloc is damage.
		/// </summary>
		/// <param name="cliloc">Cliloc to check.</param>
		/// <returns>True for damage clilocs, false otherwise.</returns>
		public static bool IsDamage( int cliloc )
		{
			for ( int i = 0; i < DamageClilocs.Length; i++ )
				if ( DamageClilocs[ i ] == cliloc )
					return true;

			return false;
		}

		/// <summary>
		/// Determines whether cliloc is container.
		/// </summary>
		/// <param name="cliloc">Cliloc to check.</param>
		/// <returns>True for contaienr clilocs, false otherwise.</returns>
		public static bool IsContainer( int cliloc )
		{
			for ( int i = 0; i < ContainerClilocs.Length; i++ )
				if ( ContainerClilocs[ i ] == cliloc )
					return true;

			return false;
		}

		/// <summary>
		/// Gets cliloc from item ID.
		/// </summary>
		/// <param name="itemID">Item ID.</param>
		/// <returns>Cliloc.</returns>
		public static int GetClilocFromItemID( int itemID )
		{
			int itemIDCliloc = 0;

			if ( itemID < 0x4000 )
				itemIDCliloc = OldItemCliloc + itemID;
			else
				itemIDCliloc = NewItemCliloc + itemID;

			return itemIDCliloc;
		}

		/// <summary>
		/// Gets property description.
		/// </summary>
		/// <param name="property">Property definition.</param>
		/// <returns>Property description.</returns>
		public static string GetPropertyDescription( UltimaItemProperty property )
		{
			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			if ( property.Switch != null )
			{
				// Use first setter name
				if ( property.Setters.Count > 0 )
					return property.Setters[ 0 ].Name;
			}
			else if ( property.Clilocs.Count > 0 )
			{
				int cliloc = property.Clilocs.Count;
				string text = clilocs.GetString( cliloc );

				if ( text != null )
				{
					if ( property.Clilocs.Count > 1 )
						return String.Format( "{0} ({1}...)", cliloc, text );

					return String.Format( "{0} ({1})", cliloc, text );
				}

				return String.Format( "{0} (Cannot find cliloc)", cliloc );
			}

			if ( property.Setters.Count > 0 )
				return property.Setters[ 0 ].Name;

			return "NoName";
		}

		/// <summary>
		/// Gets property description.
		/// </summary>
		/// <param name="cliloc">Cliloc number.</param>
		/// <returns>Property description.</returns>
		public static string GetPropertyDescription( int cliloc )
		{
			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			if ( clilocs != null )
			{
				string text = clilocs.GetString( cliloc );

				if ( text != null )
					return String.Format( "{0} ({1})", cliloc, text );

				return String.Format( "{0} (Cannot find cliloc)", cliloc );
			}

			return cliloc.ToString();
		}
		#endregion
	}
}
