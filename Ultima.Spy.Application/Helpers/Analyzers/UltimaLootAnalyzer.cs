using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Ultima.Spy.Packets;
using Ultima.Package;
using System.Diagnostics;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes loot analyzer.
	/// </summary>
	public class UltimaLootAnalyzer
	{
		#region Properties
		private List<string> _Names;

		/// <summary>
		/// Gets list mobile names.
		/// </summary>
		public List<string> Names
		{
			get { return _Names; }
		}

		private Dictionary<UltimaItemDefinitionGroup, UltimaSimpleCounter> _Groups;

		/// <summary>
		/// Gets groups.
		/// </summary>
		public Dictionary<UltimaItemDefinitionGroup, UltimaSimpleCounter> Groups
		{
			get { return _Groups; }
		}

		private UltimaDefaultLootGroup _DefaultGroup;

		/// <summary>
		/// Gets default group.
		/// </summary>
		public UltimaDefaultLootGroup DefaultGroup
		{
			get { return _DefaultGroup; }
		}

		private int _CorpseCount;
		private UltimaEnumPropertyCounter _HueCounter;
		private UltimaSimpleCounter _GoldCounter;
		private List<UltimaSimpleCounter> _InstrumentCounters;
		private List<UltimaSimpleCounter> _PropertiesPerItem;
		private Dictionary<int, UltimaPropertyCounter> _Properties;
		private List<UltimaSimpleCounter> _EquipmentCounters;
		private List<int> _MinPropertyCounts; // Max property counts sorted by descending item probabilities 
		private List<int> _MaxPropertyCounts; // Max property counts sorted by descending item probabilities 
		private List<int> _MaxPropertyValues; // Max values sorted by descending item probabilities 
		private List<int> _MinPropertyValues; // Max values sorted by descending item probabilities 
		private int _ValidCorpseCount; // Number of corpses with max equipment count
		private int _MinPropertyCount; // Min global property count
		private int _MaxPropertyCount; // Max global property count
		private int _MinPropertyValue; // Min global property value
		private int _MaxPropertyValue; // Max global property value
		#endregion

		#region Methods
		/// <summary>
		/// Constructs a new instance of UltimaLootAnalyzer.
		/// </summary>
		/// <param name="names">List of the mobile names to analyze.</param>
		public UltimaLootAnalyzer( ObservableCollection<UltimaPacket> packets, List<string> names )
		{
			_Names = names;
			_CorpseCount = 0;
			_HueCounter = new UltimaEnumPropertyCounter();
			_GoldCounter = new UltimaSimpleCounter();
			_InstrumentCounters = new List<UltimaSimpleCounter>();
			_EquipmentCounters = new List<UltimaSimpleCounter>();

			UltimaItemDefinitions itemDefinitions = Globals.Instance.ItemDefinitions;

			if ( itemDefinitions == null )
				throw new Exception( "Item definitions not initialized" );

			UltimaItemProperties propertyDefinitions = Globals.Instance.ItemProperties;

			if ( propertyDefinitions == null )
				throw new Exception( "Item property definitions not initialized" );

			// Initialize group
			UltimaItemDefinitionGroup goldGroup = null;
			UltimaItemDefinitionGroup instrumentsGroup = null;

			_Groups = new Dictionary<UltimaItemDefinitionGroup, UltimaSimpleCounter>();
			_DefaultGroup = new UltimaDefaultLootGroup();
			_PropertiesPerItem = new List<UltimaSimpleCounter>();
			_Properties = new Dictionary<int, UltimaPropertyCounter>();

			foreach ( UltimaItemDefinitionGroup group in itemDefinitions.Groups )
			{
				if ( group.Analyze )
					_Groups.Add( group, new UltimaSimpleCounter() );

				if ( String.Equals( group.Name, "Gold", StringComparison.InvariantCultureIgnoreCase ) )
					goldGroup = group;
				else if ( String.Equals( group.Name, "Instruments", StringComparison.InvariantCultureIgnoreCase ) )
					instrumentsGroup = group;
			}

			// Analyze packets
			Dictionary<uint, MobileIncommingPacket> mobiles = new Dictionary<uint, MobileIncommingPacket>();
			Dictionary<uint, uint> mobilesToCorpses = new Dictionary<uint, uint>();
			Dictionary<uint, ContainerContentPacket> corpsesToContainers = new Dictionary<uint, ContainerContentPacket>();
			Dictionary<uint, QueryPropertiesResponsePacket> itemsToProperties = new Dictionary<uint, QueryPropertiesResponsePacket>();

			foreach ( UltimaPacket packet in packets )
			{
				if ( packet is MobileIncommingPacket )
				{
					MobileIncommingPacket mobile = (MobileIncommingPacket) packet;

					if ( !mobiles.ContainsKey( mobile.Serial ) )
						mobiles.Add( mobile.Serial, mobile );
				}
				else if ( packet is DeathAnimationPacket )
				{
					DeathAnimationPacket deathAnimation = (DeathAnimationPacket) packet;

					if ( !mobilesToCorpses.ContainsKey( deathAnimation.Serial ) )
						mobilesToCorpses.Add( deathAnimation.Serial, deathAnimation.Corpse );
				}
				else if ( packet is ContainerContentPacket )
				{
					ContainerContentPacket containerContent = (ContainerContentPacket) packet;

					if ( !corpsesToContainers.ContainsKey( containerContent.Serial ) )
						corpsesToContainers.Add( containerContent.Serial, containerContent );
				}
				else if ( packet is QueryPropertiesResponsePacket )
				{
					QueryPropertiesResponsePacket properties = (QueryPropertiesResponsePacket) packet;

					if ( !itemsToProperties.ContainsKey( properties.Serial ) )
						itemsToProperties.Add( properties.Serial, properties );
				}
			}

			Dictionary<ContainerContentPacket, List<ItemStatistics>> validCorpses = new Dictionary<ContainerContentPacket, List<ItemStatistics>>();
			_MinPropertyCount = int.MaxValue;
			_MaxPropertyCount = int.MinValue;
			_MinPropertyValue = int.MaxValue;
			_MaxPropertyValue = int.MinValue;

			foreach ( KeyValuePair<uint, uint> kvp in mobilesToCorpses )
			{
				MobileIncommingPacket mobile = null;
				ContainerContentPacket corpseContainer = null;
				ContainerContentPacket container = null;
				QueryPropertiesResponsePacket mobileProperties = null;

				if ( !mobiles.TryGetValue( kvp.Key, out mobile ) )
					continue;

				if ( !itemsToProperties.TryGetValue( kvp.Key, out mobileProperties ) || mobileProperties.Properties.Count == 0 )
					continue;

				if ( !corpsesToContainers.TryGetValue( kvp.Value, out corpseContainer ) )
					continue;

				if ( corpseContainer.Items.Count > 0 )
				{
					ContainerItem corpse = corpseContainer.Items[ 0 ];

					if ( !corpsesToContainers.TryGetValue( corpse.Serial, out container ) )
						continue;
				}
				else
					continue;

				string mobileName = GetMobileName( mobileProperties.Properties[ 0 ] );

				if ( names.Contains( mobileName ) )
				{
					// Analyze corpse
					StartAnalyzingCorpse();

					List<ItemStatistics> validItems = new List<ItemStatistics>();
					int equipmentCount = 0;
					int instrumentCount = 0;
					bool foundGold = false;

					_HueCounter.Gotcha( mobile.Hue );
					_CorpseCount += 1;

					Trace.WriteLine( "" );
					Trace.WriteLine( "Found corpse with " + container.Items.Count );

					foreach ( ContainerItem item in container.Items )
					{
						QueryPropertiesResponsePacket properties = null;
						UltimaItemDefinition itemDefinition = null;
						UltimaArmorDefinition armorDefinition = null;
						bool analyzed = false;

						if ( itemDefinitions.Items.ContainsKey( item.ItemID ) )
						{
							itemDefinition = itemDefinitions.Items[ item.ItemID ];
							armorDefinition = itemDefinition as UltimaArmorDefinition;
						}

						if ( itemsToProperties.ContainsKey( item.Serial ) )
							properties = itemsToProperties[ item.Serial ];

						string name = GetItemName( properties.Properties[ 0 ] );

						// EA always generates gold last
						if ( !foundGold )
						{
							if ( itemDefinition != null )
							{
								UltimaItemDefinitionGroup group = itemDefinition.Parent;

								while ( group.Parent != null )
									group = group.Parent;

								if ( properties != null )
								{
									QueryPropertiesProperty nameProperty = properties.Properties[ 0 ];

									// Treat stackable items as special
									if ( !UltimaItemGenerator.IsStackable( nameProperty.Cliloc ) &&
										!UltimaItemGenerator.IsString( nameProperty.Cliloc ) &&
										!UltimaItemGenerator.IsSpecial( item.ItemID, nameProperty.Cliloc ) )
									{
										if ( _Groups.ContainsKey( group ) )
										{
											_Groups[ group ].Gotcha();
											analyzed = true;

											int propertiesPerItem = 0;
											int minPropertyValue = int.MaxValue;
											int maxPropertyValue = int.MinValue;
											bool isValidMinMax = false;
											bool hasSpecialDamage = false;

											foreach ( QueryPropertiesProperty property in properties.Properties )
											{
												string propertyName = Globals.Instance.Clilocs.GetString( property.Cliloc );

												if ( UltimaItemGenerator.IsDamage( property.Cliloc ) )
												{
													if ( property.Cliloc != 1060403 )
													{
														if ( !hasSpecialDamage )
															propertiesPerItem += 1;

														hasSpecialDamage = true;
													}
												}
												else if ( propertyDefinitions.Properties.ContainsKey( property.Cliloc ) )
												{
													UltimaItemProperty propertyDefinition = propertyDefinitions.Properties[ property.Cliloc ];

													if ( propertyDefinition.IsRunic )
													{
														UltimaClilocArgumentParser arguments = new UltimaClilocArgumentParser( property.Arguments );
														bool ignore = false;

														if ( arguments.Length > 0 )
														{
															int integer = 0;

															if ( arguments.TryGetInteger( 0, out integer ) )
															{
																if ( armorDefinition != null )
																	ignore = CheckIgnoreProperty( armorDefinition, propertyDefinition, ref integer );

																if ( !ignore )
																{
																	GetPropertyCounter( property.Cliloc, 1 ).Gotcha( integer );

																	if ( propertyDefinition.Max > propertyDefinition.Min )
																	{
																		int percentage = ( integer - propertyDefinition.Min ) * 100 / ( propertyDefinition.Max - propertyDefinition.Min );

																		if ( percentage > maxPropertyValue )
																			maxPropertyValue = percentage;

																		if ( percentage > _MaxPropertyValue )
																			_MaxPropertyValue = percentage;

																		if ( percentage < minPropertyValue )
																			minPropertyValue = percentage;

																		if ( percentage < _MinPropertyValue )
																			_MinPropertyValue = percentage;

																		isValidMinMax = true;
																	}
																}
															}
															else
																GetPropertyCounter( property.Cliloc, 2 ).Gotcha( arguments[ 0 ] );
														}
														else
															GetPropertyCounter( property.Cliloc ).Gotcha( null );

														if ( !ignore )
															propertiesPerItem++;
													}
												}
											}

											// Count number of properties
											UltimaSimpleCounter counter = null;

											while ( propertiesPerItem >= _PropertiesPerItem.Count )
												_PropertiesPerItem.Add( counter = new UltimaSimpleCounter() );

											counter = _PropertiesPerItem[ propertiesPerItem ];
											counter.StartAnalyzing();
											counter.Gotcha();
											counter.EndAnalyzing();

											if ( propertiesPerItem < _MinPropertyCount )
												_MinPropertyCount = propertiesPerItem;

											if ( propertiesPerItem > _MaxPropertyCount )
												_MaxPropertyCount = propertiesPerItem;

											equipmentCount += 1;

											if ( isValidMinMax )
												validItems.Add( new ItemStatistics( item, propertiesPerItem, minPropertyValue, maxPropertyValue ) );
											else
												validItems.Add( new ItemStatistics( item, propertiesPerItem ) );
										}
									}
								}
							}
							else
								Trace.WriteLine( "Cannot find item definition for:" + String.Format( "0x{0:X}", item.ItemID ) + "," + name );

							// Check if special item
							if ( itemDefinition != null && !analyzed )
							{
								UltimaItemDefinitionGroup group = itemDefinition.Parent;

								if ( group == goldGroup )
								{
									_GoldCounter.Gotcha( item.Amount );
									analyzed = true;
									foundGold = true;
								}
								else if ( group == instrumentsGroup )
								{
									analyzed = true;
								}
							}
						}

						if ( !analyzed )
							_DefaultGroup.AnalyzeItem( item.Serial, item.ItemID, item.Hue, item.Amount, properties );
					}

					Trace.WriteLine( equipmentCount );

					// Count equipment
					UltimaSimpleCounter equipmentCounter = null;

					while ( equipmentCount >= _EquipmentCounters.Count )
						_EquipmentCounters.Add( equipmentCounter = new UltimaSimpleCounter() );

					equipmentCounter = _EquipmentCounters[ equipmentCount ];
					equipmentCounter.StartAnalyzing();
					equipmentCounter.Gotcha();
					equipmentCounter.EndAnalyzing();

					// Count instruments
					UltimaSimpleCounter instrumentCounter = null;

					while ( instrumentCount >= _InstrumentCounters.Count )
						_InstrumentCounters.Add( instrumentCounter = new UltimaSimpleCounter() );

					instrumentCounter = _InstrumentCounters[ instrumentCount ];
					instrumentCounter.StartAnalyzing();
					instrumentCounter.Gotcha();
					instrumentCounter.EndAnalyzing();

					if ( validItems.Count > 0 )
						validCorpses.Add( container, validItems );

					// Count corpses
					EndAnalyzingCorpse();
				}
			}

			// Analyze items with max properties to determine property probabilities
			_MinPropertyCounts = new List<int>();
			_MaxPropertyCounts = new List<int>();
			_MinPropertyValues = new List<int>();
			_MaxPropertyValues = new List<int>();
			_ValidCorpseCount = 0;

			foreach ( KeyValuePair<ContainerContentPacket, List<ItemStatistics>> kvp in validCorpses )
			{
				// Only ones with max items are valid
				if ( kvp.Value.Count == _EquipmentCounters.Count - 1 )
				{
					bool isValidMinMax = true;

					foreach ( ItemStatistics item in kvp.Value )
					{
						if ( !item.IsValidMinMax )
						{
							isValidMinMax = false;
							break;
						}
					}

					kvp.Value.Sort( SortByCount );

					if ( _MinPropertyCounts.Count == 0 )
					{
						foreach ( ItemStatistics item in kvp.Value )
							_MinPropertyCounts.Add( item.PropertyCount );
					}
					else
					{
						for ( int i = 0; i < kvp.Value.Count; i++ )
						{
							int count = kvp.Value[ i ].PropertyCount;

							if ( count < _MinPropertyCounts[ i ] )
								_MinPropertyCounts[ i ] = count;
						}
					}

					if ( _MaxPropertyCounts.Count == 0 )
					{
						foreach ( ItemStatistics item in kvp.Value )
							_MaxPropertyCounts.Add( item.PropertyCount );
					}
					else
					{
						for ( int i = 0; i < kvp.Value.Count; i++ )
						{
							int count = kvp.Value[ i ].PropertyCount;

							if ( count > _MaxPropertyCounts[ i ] )
								_MaxPropertyCounts[ i ] = count;
						}
					}

					if ( isValidMinMax )
					{
						kvp.Value.Sort( SortByMin );

						if ( _MinPropertyValues.Count == 0 )
						{
							foreach ( ItemStatistics item in kvp.Value )
								_MinPropertyValues.Add( item.MinPropertyValue );
						}
						else
						{
							for ( int i = 0; i < kvp.Value.Count; i++ )
							{
								int min = kvp.Value[ i ].MinPropertyValue;

								if ( min < _MinPropertyValues[ i ] )
									_MinPropertyValues[ i ] = min;
							}
						}

						kvp.Value.Sort( SortByMax );

						if ( _MaxPropertyValues.Count == 0 )
						{
							foreach ( ItemStatistics item in kvp.Value )
								_MaxPropertyValues.Add( item.MaxPropertyValue );
						}
						else
						{
							for ( int i = 0; i < kvp.Value.Count; i++ )
							{
								int max = kvp.Value[ i ].MaxPropertyValue;

								if ( max > _MaxPropertyValues[ i ] )
									_MaxPropertyValues[ i ] = max;
							}
						}
					}

					_ValidCorpseCount += 1;
				}
			}
		}

		private static int SortByCount( ItemStatistics a, ItemStatistics b )
		{
			if ( a.PropertyCount < b.PropertyCount )
				return -1;
			else if ( a.PropertyCount > b.PropertyCount )
				return 1;

			return 0;
		}

		private static int SortByMin( ItemStatistics a, ItemStatistics b )
		{
			if ( a.MinPropertyValue < b.MinPropertyValue )
				return -1;
			else if ( a.MinPropertyValue > b.MinPropertyValue )
				return 1;

			return 0;
		}

		private static int SortByMax( ItemStatistics a, ItemStatistics b )
		{
			if ( a.MaxPropertyValue < b.MaxPropertyValue )
				return -1;
			else if ( a.MaxPropertyValue > b.MinPropertyValue )
				return 1;

			return 0;
		}

		private void StartAnalyzingCorpse()
		{
			_DefaultGroup.StartAnalyzingCorpse();
			_GoldCounter.StartAnalyzing();

			foreach ( KeyValuePair<UltimaItemDefinitionGroup, UltimaSimpleCounter> kvp in _Groups )
				kvp.Value.StartAnalyzing();
		}

		private void EndAnalyzingCorpse()
		{
			_DefaultGroup.EndAnalyzingCorpse();
			_GoldCounter.EndAnalyzing();

			foreach ( KeyValuePair<UltimaItemDefinitionGroup, UltimaSimpleCounter> kvp in _Groups )
				kvp.Value.EndAnalyzing();
		}

		private UltimaPropertyCounter GetPropertyCounter( int cliloc, int type = 0 )
		{
			UltimaPropertyCounter counter = null;

			if ( !_Properties.ContainsKey( cliloc ) )
			{
				switch ( type )
				{
					default: counter = new UltimaPropertyCounter( cliloc ); break;
					case 1: counter = new UltimaPropertyRangeCounter( cliloc ); break;
					case 2: counter = new UltimaPropertyCounterString( cliloc ); break;
				}

				_Properties.Add( cliloc, counter );
			}
			else
				counter = _Properties[ cliloc ];

			return counter;
		}

		private bool CheckIgnoreProperty( UltimaArmorDefinition armorDefinition, UltimaItemProperty propertyDefinition, ref int integer )
		{
			bool ignore = false;

			if ( propertyDefinition.IsBasePhysical )
			{
				if ( integer == armorDefinition.BasePhysical )
					ignore = true;
				else
					integer -= armorDefinition.BasePhysical;
			}
			else if ( propertyDefinition.IsBaseFire )
			{
				if ( integer == armorDefinition.BaseFire )
					ignore = true;
				else
					integer -= armorDefinition.BaseFire;
			}
			else if ( propertyDefinition.IsBaseCold )
			{
				if ( integer == armorDefinition.BaseCold )
					ignore = true;
				else
					integer -= armorDefinition.BaseCold;
			}
			else if ( propertyDefinition.IsBasePoison )
			{
				if ( integer == armorDefinition.BasePoison )
					ignore = true;
				else
					integer -= armorDefinition.BasePoison;
			}
			else if ( propertyDefinition.IsBaseEnergy )
			{
				if ( integer == armorDefinition.BaseEnergy )
					ignore = true;
				else
					integer -= armorDefinition.BaseEnergy;
			}
			else if ( propertyDefinition.IsBaseChaos )
			{
				if ( integer == armorDefinition.BaseChaos )
					ignore = true;
				else
					integer -= armorDefinition.BaseChaos;
			}
			else if ( propertyDefinition.IsBaseDirect )
			{
				if ( integer == armorDefinition.BaseDirect )
					ignore = true;
				else
					integer -= armorDefinition.BaseDirect;
			}

			return ignore;
		}

		/// <summary>
		/// Builds analysis report.
		/// </summary>
		/// <returns>Analysis report.</returns>
		public string BuildReport()
		{
			if ( _CorpseCount == 0 )
			{
				return "No corpses found...";
			}

			StringBuilder builder = new StringBuilder( (int) Math.Pow( 2, 10 ) );

			builder.AppendFormat( "Mobile names: {0} \n", String.Join( ",", _Names ) );
			builder.AppendFormat( "Corpse count: {0} \n", _CorpseCount );
			builder.AppendFormat( "Mobile hues: {0}\n", _HueCounter );
			builder.AppendFormat( "Mobile gold: Avg/Min/Max\n", _HueCounter );
			builder.AppendFormat( "             {0}/{1}/{2}\n", _GoldCounter.Total / _CorpseCount, _GoldCounter.Min, _GoldCounter.Max );

			// Equipment
			int totalItems = 0;

			foreach ( KeyValuePair<UltimaItemDefinitionGroup, UltimaSimpleCounter> kvp in _Groups )
				totalItems += kvp.Value.Total;

			if ( totalItems > 0 )
			{
				builder.AppendLine();
				builder.AppendLine( "Equipment By Groups" );
				builder.AppendLine( "Group: Equipment/Total Equipment = Percentage" );

				foreach ( KeyValuePair<UltimaItemDefinitionGroup, UltimaSimpleCounter> kvp in _Groups )
					builder.AppendFormat( "{0}: {1}/{2} = {3}%\n", kvp.Key.Name, kvp.Value.Total, totalItems, Math.Round( kvp.Value.Total * 100.0 / totalItems, 3 ) );

				builder.AppendLine();
				builder.AppendLine( "RunUO loot pack item distribution:" );
				builder.AppendLine( "public static readonly LootPackItem[] MagicItems = new LootPackItem[]" );
				builder.AppendLine( "{" );

				foreach ( KeyValuePair<UltimaItemDefinitionGroup, UltimaSimpleCounter> kvp in _Groups )
					builder.AppendFormat( "\tnew LootPackItem( typeof( {0} ), {1} ),\n", kvp.Key.Class, kvp.Value.Total * 1000 / totalItems );

				builder.AppendLine( "};" );
				builder.AppendLine();
				builder.AppendFormat( "Equipment by Corpses" );
				builder.AppendLine( "Number Per Corpse: Equipment/Total Equipment = Percentage" );

				for ( int i = 0; i < _EquipmentCounters.Count; i++ )
				{
					UltimaSimpleCounter counter = _EquipmentCounters[ i ];

					builder.AppendFormat( "Corpses with {0} items: {1}/{2} = {3}%\n", i, counter.Total, _CorpseCount, Math.Round( counter.Total * 100.0 / _CorpseCount, 3 ) );
				}
			}

			// Properties
			int totalPropertiesItems = 0;

			foreach ( UltimaSimpleCounter counter in _PropertiesPerItem )
				totalPropertiesItems += counter.Total;

			if ( totalPropertiesItems > 0 )
			{
				builder.AppendLine();
				builder.AppendFormat( "Runic properties Per Equipment" );
				builder.AppendLine( "Number Per Item: Items/Total Items = Percentage" );

				for ( int i = 0; i < _PropertiesPerItem.Count; i++ )
				{
					UltimaSimpleCounter counter = _PropertiesPerItem[ i ];

					builder.AppendFormat( "Items with {0} runic properties: {1}/{2} = {3}%\n", i, counter.Total, totalPropertiesItems, Math.Round( counter.Total * 100.0 / totalPropertiesItems, 3 ) );
				}
			}

			// Gold dice
			double min = _GoldCounter.Min;
			double max = _GoldCounter.Max;
			int factor = 1;

			if ( min > 1000 )
			{
				min = Math.Floor( min / 100 ) * 100;
				factor = 100;
			}
			else if ( min > 100 )
			{
				min = Math.Floor( min / 10 ) * 10;
				factor = 10;
			}
			
			max = Math.Ceiling( max / factor ) * factor;

			string goldDice = String.Format( "{0}d{1}+{2}", ( max - min ) / factor, factor, min );

			// Loot pack
			builder.AppendLine();
			builder.AppendLine( "RunUO loot pack items:" );
			builder.AppendLine( "Number of corpses with maximum equipment: " + _ValidCorpseCount );

			if ( _ValidCorpseCount < 50 )
				builder.AppendFormat( "Number of corpses with maximum equipment is only {0}. Do more killing\n", _ValidCorpseCount );

			builder.AppendLine( "public static readonly LootPack GenericLootPack = new LootPack( new LootPackEntry[]" );
			builder.AppendLine( "{" );
			builder.AppendFormat( "\tnew LootPackEntry(  false, Gold,                                         100.00, \"{0}\" ),\n", goldDice );
			 
			if ( _EquipmentCounters.Count > 1 )
			{
				for ( int i = 1; i < _EquipmentCounters.Count; i++ )
				{
					UltimaSimpleCounter counter = _EquipmentCounters[ i ];

					// Probability of i-th item is sum of subsequent probabilites
					double chance = 0;

					for ( int j = i; j < _EquipmentCounters.Count; j++ )
						chance += _EquipmentCounters[ j ].Total * 100.0;

					chance /= _CorpseCount;

					// Check global values
					int minPropertyCount = 0;
					int maxPropertyCount = 0;
					int minPropertyValue = _MinPropertyValue;
					int maxPropertyValue = _MaxPropertyValue;

					if ( _MinPropertyCounts.Count >= i )
					{
						minPropertyCount = _MinPropertyCounts[ i - 1 ];

						if ( i == 1 && _MinPropertyCount < minPropertyCount )
							minPropertyCount = _MinPropertyCount;
					}

					if ( _MaxPropertyCounts.Count >= i )
					{
						maxPropertyCount = _MaxPropertyCounts[ i - 1 ];

						if ( i == _EquipmentCounters.Count && _MaxPropertyCount > maxPropertyCount )
							maxPropertyCount = _MaxPropertyCount;
					}

					if ( _MinPropertyValues.Count >= i )
					{
						minPropertyValue = _MinPropertyValues[ i - 1 ];

						if ( i == 0 && _MinPropertyValue < minPropertyValue )
							maxPropertyCount = _MinPropertyValue;
					}

					if ( _MaxPropertyValues.Count >= i )
					{
						maxPropertyValue = _MaxPropertyCounts[ i - 1 ];

						if ( i == _EquipmentCounters.Count && _MaxPropertyValue < maxPropertyValue )
							maxPropertyCount = _MaxPropertyValue;
					}

					builder.AppendFormat( "\tnew LootPackEntry(  true, MagicItems,                                   {0}, {1}, {2}, {3}, {4} ),\n", 
						Math.Round( chance, 2 ), minPropertyCount, maxPropertyCount, minPropertyValue, maxPropertyValue );
				}
			}

			if ( _InstrumentCounters.Count > 1 )
			{
				for ( int i = 1; i < _InstrumentCounters.Count; i++ )
				{
					UltimaSimpleCounter counter = _InstrumentCounters[ i ];

					// Probability of i-th item is sum of subsequent probabilites
					double chance = 0;

					for ( int j = i; j < _InstrumentCounters.Count; j++ )
						chance += _InstrumentCounters[ j ].Total * 1.0 / _CorpseCount;

					builder.AppendFormat( "\tnew LootPackEntry(  true, Instruments,                                  {0}, 1 ),\n", Math.Round( chance, 2 ) );
				}
			}

			builder.Remove( builder.Length - 2, 2 );
			builder.AppendLine();
			builder.AppendLine( "} );" );

			return builder.ToString();
		}

		/// <summary>
		/// Gets available mobile names.
		/// </summary>
		/// <returns>List of available mobile names.</returns>
		public static List<string> GetAvailableMobiles( ObservableCollection<UltimaPacket> packets )
		{
			List<string> names = new List<string>();
			Dictionary<uint,bool> mobiles = new Dictionary<uint, bool>();
			Dictionary<string, bool> nameDictionary = new Dictionary<string, bool>();

			foreach ( UltimaPacket packet in packets )
			{
				MobileIncommingPacket mobile = packet as MobileIncommingPacket;

				if ( mobile != null && !mobiles.ContainsKey( mobile.Serial ) )
					mobiles.Add( mobile.Serial, true );
			}

			foreach ( UltimaPacket packet in packets )
			{
				QueryPropertiesResponsePacket properties = packet as QueryPropertiesResponsePacket;

				if ( properties != null && properties.Properties.Count > 0 && mobiles.ContainsKey( properties.Serial ) )
				{
					string name = GetMobileName( properties.Properties[ 0 ] );

					if ( !nameDictionary.ContainsKey( name ) )
					{
						nameDictionary.Add( name, true );
						names.Add( name );
					}
				}
			}

			return names;
		}

		/// <summary>
		/// Gets item name from name property.
		/// </summary>
		/// <param name="nameProperty">Name property.</param>
		/// <returns>Item name.</returns>
		public static string GetItemName( QueryPropertiesProperty nameProperty )
		{
			int cliloc = 0;
			string name = null;

			if ( UltimaItemGenerator.IsStackable( nameProperty.Cliloc ) )
			{
				// Get name from stackable cliloc
				UltimaClilocArgumentParser nameArguments = new UltimaClilocArgumentParser( nameProperty.Arguments );
				int nameCliloc = nameArguments.GetCliloc( 1 );

				if ( nameCliloc > 0 )
					cliloc = nameCliloc;
				else
					name = nameArguments[ 1 ];
			}
			else
			{
				// Get name from name cliloc
				if ( !UltimaItemGenerator.IsString( nameProperty.Cliloc ) )
					cliloc = nameProperty.Cliloc;
				else
					name = nameProperty.Arguments;
			}

			if ( name == null )
			{
				UltimaStringCollection clilocs = Globals.Instance.Clilocs;

				if ( clilocs != null )
					name = clilocs.GetString( cliloc );

				if ( String.IsNullOrEmpty( name ) )
					name = cliloc.ToString();
			}

			return name;
		}

		/// <summary>
		/// Gets mobile name from name property.
		/// </summary>
		/// <param name="nameProperty">Name property.</param>
		/// <returns>Mobile name.</returns>
		public static string GetMobileName( QueryPropertiesProperty nameProperty )
		{
			// Get item cliloc
			int cliloc = 0;
			string name = null;

			if ( UltimaItemGenerator.IsStackable( nameProperty.Cliloc ) )
			{
				// Get name from stackable cliloc
				UltimaClilocArgumentParser nameArguments = new UltimaClilocArgumentParser( nameProperty.Arguments );
				int nameCliloc = nameArguments.GetCliloc( 0 );

				if ( nameCliloc > 0 )
					cliloc = nameCliloc;
				else
					name = nameArguments[ 0 ];
			}
			else
			{
				// Get name from name cliloc
				if ( nameProperty.Cliloc == 1050045 )
				{
					UltimaClilocArgumentParser arguments = new UltimaClilocArgumentParser( nameProperty.Arguments );

					if ( arguments.Length > 2 )
						name = arguments[ 1 ];
				}
				else
					cliloc = nameProperty.Cliloc;
			}

			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			if ( name == null )
			{
				if ( clilocs != null )
					name = clilocs.GetString( cliloc );

				if ( String.IsNullOrEmpty( name ) )
					name = cliloc.ToString();
			}

			return name;
		}

		/// <summary>
		/// Describes item statistics.
		/// </summary>
		private struct ItemStatistics
		{
			#region Properties
			/// <summary>
			/// Gets item.
			/// </summary>
			public ContainerItem Item;

			/// <summary>
			/// Gets runic property count.
			/// </summary>
			public int PropertyCount;

			/// <summary>
			/// Gets min runic property value.
			/// </summary>
			public int MinPropertyValue;

			/// <summary>
			/// Gets max runic property value.
			/// </summary>
			public int MaxPropertyValue;

			/// <summary>
			/// Determines whether property values are valid.
			/// </summary>
			public bool IsValidMinMax;
			#endregion

			#region Constructors
			/// <summary>
			/// Constructs a new instance of ItemStatistics.
			/// </summary>
			/// <<param name="item">Item in question.</param>
			/// <param name="propertyCount">Runic property count.</param>
			/// <param name="minPropertyValue">Min runic property value.</param>
			/// <param name="maxPropertyValue">Max runic property value.</param>
			public ItemStatistics( ContainerItem item, int propertyCount , int minPropertyValue, int maxPropertyValue)
			{
				Item = item;
				PropertyCount = propertyCount;
				MinPropertyValue = minPropertyValue;
				MaxPropertyValue = maxPropertyValue;
				IsValidMinMax = true;
			}

			/// <summary>
			/// Constructs a new instance of ItemStatistics.
			/// </summary>
			/// <<param name="item">Item in question.</param>
			/// <param name="propertyCount">Runic property count.</param>
			public ItemStatistics( ContainerItem item, int propertyCount )
			{
				Item = item;
				PropertyCount = propertyCount;
				MinPropertyValue = -1;
				MaxPropertyValue = -1;
				IsValidMinMax = false;
			}
			#endregion
		}
		#endregion
	}
}
