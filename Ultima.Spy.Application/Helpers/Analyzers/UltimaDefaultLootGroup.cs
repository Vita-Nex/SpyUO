using System.Collections.Generic;
using Ultima.Spy.Packets;
using Ultima.Package;
using System;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Descrbies ultima loot group.
	/// </summary>
	public class UltimaDefaultLootGroup
	{
		#region Properties
		private UltimaSimpleCounter _Counter;

		/// <summary>
		/// Gets item counter.
		/// </summary>
		public UltimaSimpleCounter Counter
		{
			get { return _Counter; }
		}

		private Dictionary<string, UltimaItemCounter> _Items;

		/// <summary>
		/// Gets items by name.
		/// </summary>
		public Dictionary<string, UltimaItemCounter> Items
		{
			get { return _Items; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaDefaultLootGroup.
		/// </summary>
		public UltimaDefaultLootGroup()
		{
			_Counter = new UltimaSimpleCounter();
			_Items = new Dictionary<string, UltimaItemCounter>();
		}
		#endregion

		#region Analyze
		/// <summary>
		/// Analyzes item.
		/// </summary>
		/// <param name="serial">Item serial.</param>
		/// <param name="itemID">Item ID.</param>
		/// <param name="hue">Item hue.</param>
		/// <param name="amount">Item amount.</param>
		/// <param name="properties">Item properties.</param>
		public void AnalyzeItem( uint serial, int itemID, int hue, int amount, QueryPropertiesResponsePacket properties )
		{
			// Get item cliloc
			int cliloc = 0;
			string name = null;

			if ( properties != null && properties.Properties.Count > 0 )
			{
				// Get name from name property
				QueryPropertiesProperty nameProperty = properties.Properties[ 0 ];

				if ( UltimaItemGenerator.IsStackable( nameProperty.Cliloc ) )
				{
					// Get name from stackable cliloc
					UltimaClilocArgumentParser nameArguments = new UltimaClilocArgumentParser( nameProperty.Arguments );
					int nameCliloc = nameArguments.GetCliloc( 1 );

					if ( nameCliloc > 0 )
						cliloc = nameCliloc;
					else
						name = nameArguments[1 ];
				}
				else
				{
					// Get name from name cliloc
					if ( !UltimaItemGenerator.IsString( nameProperty.Cliloc ) )
						cliloc = nameProperty.Cliloc;
					else
						name = nameProperty.Arguments;
				}
			}
			else
			{
				// Get name from item ID
				cliloc = UltimaItemGenerator.GetClilocFromItemID( itemID );
			}

			// Count
			UltimaItemCounter counter = null;
			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			if ( name == null )
			{
				if ( clilocs != null )
					name = clilocs.GetString( cliloc );

				if ( String.IsNullOrEmpty( name ) )
					name = cliloc.ToString();
			}

			if ( !_Items.TryGetValue( name, out counter ) )
			{
				counter = new UltimaItemCounter( serial, name );
				_Items.Add( name, counter );
			}

			if ( UltimaItemGenerator.IsStackable( cliloc ) )
				amount = Math.Max( amount, 1 );
			else
				amount = 1;
			
			counter.Gotcha( hue, amount );
			_Counter.Gotcha( 1 );
		}

		/// <summary>
		/// Starts analyzing corpse.
		/// </summary>
		public void StartAnalyzingCorpse()
		{
			_Counter.StartAnalyzing();

			foreach ( KeyValuePair<string, UltimaItemCounter> kvp in _Items )
				kvp.Value.StartAnalyzing();
		}

		/// <summary>
		/// Ends analyzing corpse.
		/// </summary>
		public void EndAnalyzingCorpse()
		{
			_Counter.EndAnalyzing();

			foreach ( KeyValuePair<string, UltimaItemCounter> kvp in _Items )
				kvp.Value.EndAnalyzing();
		}
		#endregion
	}
}
