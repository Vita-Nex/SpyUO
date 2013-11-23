using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;

namespace Ultima.Spy.Application
{
	/// <summary> 
	/// Filter table.
	/// </summary>
	public class UltimaPacketFilterTable : DependencyObject, IUltimaPacketFilterEntry
	{
		#region Properties
		/// <summary>
		/// Represents IsVisible property.
		/// </summary>
		public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
			"IsVisible", typeof( bool ), typeof( UltimaPacketFilterTable ), new PropertyMetadata( false ) );

		/// <summary>
		/// Gets or sets filter represented by this control.
		/// </summary>
		public bool IsVisible
		{
			get { return (bool) GetValue( IsVisibleProperty ); }
			set { SetValue( IsVisibleProperty, value ); }
		}

		/// <summary>
		/// Represents IsChecked property.
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
			"IsChecked", typeof( bool ), typeof( UltimaPacketFilterTable ),
			new PropertyMetadata( false, new PropertyChangedCallback( IsChecked_Changed ) ) );

		/// <summary>
		/// Gets or sets checked represented by this control.
		/// </summary>
		public bool IsChecked
		{
			get { return (bool) GetValue( IsCheckedProperty ); }
			set { SetValue( IsCheckedProperty, value ); }
		}

		/// <summary>
		/// Represents IsFiltered property.
		/// </summary>
		public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.Register(
			"IsFiltered", typeof( bool ), typeof( UltimaPacketFilterTable ), new PropertyMetadata( false ) );

		/// <summary>
		/// Determines whether this entry is filtered out by search expression.
		/// </summary>
		public bool IsFiltered
		{
			get { return (bool) GetValue( IsFilteredProperty ); }
			set { SetValue( IsFilteredProperty, value ); }
		}

		/// <summary>
		/// Represents Index property.
		/// </summary>
		public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
			"Index", typeof( int ), typeof( UltimaPacketFilterTable ), new PropertyMetadata( -1 ) );

		/// <summary>
		/// Gets or sets entry index.
		/// </summary>
		public int Index
		{
			get { return (int) GetValue( IndexProperty ); }
			set { SetValue( IndexProperty, value ); }
		}

		private UltimaPacketFilter _Owner;

		/// <summary>
		/// Gets or sets owner.
		/// </summary>
		public UltimaPacketFilter Owner
		{
			get { return _Owner; }
		}

		private UltimaPacketFilterTable _Parent;

		/// <summary>
		/// Gets parent table.
		/// </summary>
		public UltimaPacketFilterTable Parent
		{
			get { return _Parent; }
		}

		private UltimaPacketTable _Definition;

		/// <summary>
		/// Gets packet info.
		/// </summary>
		public UltimaPacketTable Definition
		{
			get { return _Definition; }
		}

		private IUltimaPacketFilterEntry[] _Children;

		/// <summary>
		/// Gets children.
		/// </summary>
		public IUltimaPacketFilterEntry[] Children
		{
			get { return _Children; }
			set { _Children = value; }
		}

		/// <summary>
		/// Returns i-th child.
		/// </summary>
		/// <param name="index">Chid index.</param>
		/// <returns>Child at specific index.</returns>
		public IUltimaPacketFilterEntry this[ int index ]
		{
			get { return _Children[ index ]; }
			set { _Children[ index ] = value; }
		}

		/// <summary>
		/// Children count.
		/// </summary>
		public int Size
		{
			get { return _Children.Length; }
		}

		private bool _IsBusy;

		/// <summary>
		/// Determines whether children can update parents.
		/// </summary>
		public bool IsBusy
		{
			get { return _IsBusy; }
			set { _IsBusy = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketFilterTable.
		/// </summary>
		/// <param name="definition">Table definition.</param>
		/// <param name="owner">Owner.</param>
		/// <param name="parent">Table parent.</param>
		/// <param name="index">Table index.</param>
		public UltimaPacketFilterTable( UltimaPacketTable definition, UltimaPacketFilter owner = null, UltimaPacketFilterTable parent = null, int index = - 1 )
		{
			_Definition = definition;
			_Owner = owner;
			_Parent = parent;
			Index = index;

			if ( parent == null )
				IsVisible = true;

			// Initialize children
			IsBusy = true;

			if ( definition != null )
			{
				_Children = new IUltimaPacketFilterEntry[ definition.Length ];

				for ( int i = 0; i < definition.Length; i++ )
				{
					object item = definition[ i ];

					if ( item != null )
					{
						UltimaPacketTableEntry entry = item as UltimaPacketTableEntry;

						if ( entry != null )
						{
							if ( entry.FromServer != null )
								_Children[ i ] = new UltimaPacketFilterEntry( entry.FromServer, owner, this, i );
							else if ( entry.FromClient != null )
								_Children[ i ] = new UltimaPacketFilterEntry( entry.FromClient, owner, this, i );
							else
								_Children[ i ] = new UltimaPacketFilterEntry( null, owner, this, i );
						}
						else
							_Children[ i ] = new UltimaPacketFilterTable( (UltimaPacketTable) item, owner, this, i );
					}
					else
						_Children[ i ] = new UltimaPacketFilterEntry( null, owner, this, i );
				}
			}
			else
				_Children = new IUltimaPacketFilterEntry[ Byte.MaxValue + 1 ];

			IsBusy = false;
		}
		#endregion

		#region Methods
		private bool EntryFilter( object item )
		{
			if ( item != null )
			{
				IUltimaPacketFilterEntry entry = item as IUltimaPacketFilterEntry;

				if ( entry != null )
					return entry.IsVisible && !entry.IsFiltered;
			}

			return true;
		}

		private static void IsChecked_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterTable table = d as UltimaPacketFilterTable;

			if ( table != null && !table.IsBusy )
				table.UpdateIsChecked();
		}

		private void UpdateIsChecked()
		{
			_IsBusy = true;

			foreach ( IUltimaPacketFilterEntry o in _Children )
			{
				if ( o.IsVisible )
					o.IsChecked = IsChecked;
			}

			if ( _Parent != null && !_Parent.IsBusy )
				_Parent.AreAllChecked();

			if ( _Owner != null )
				_Owner.OnChange();

			_IsBusy = false;
		}

		/// <summary>
		/// Shows all packets.
		/// </summary>
		public void ShowAll()
		{
			_IsBusy = true;

			foreach ( IUltimaPacketFilterEntry o in _Children )
				o.ShowAll();

			IsVisible = true;
			AreAllChecked();
			_IsBusy = false;
		}

		/// <summary>
		/// Hides unknown packets.
		/// </summary>
		public void HideUnknown()
		{
			_IsBusy = true;
			bool isAnyVisible = false;
			
			foreach ( IUltimaPacketFilterEntry o in _Children )
			{
				o.HideUnknown();

				if ( o.IsVisible )
					isAnyVisible = true;
			}

			if ( _Parent != null )
				IsVisible = isAnyVisible;

			if ( isAnyVisible )
				AreAllChecked();

			_IsBusy = false;
		}

		/// <summary>
		/// Checks if all children are checked.
		/// </summary>
		public void AreAllChecked()
		{
			bool areAllChecked = true;

			for ( int i = 1; i < _Children.Length; i++ )
			{
				IUltimaPacketFilterEntry entry = _Children[ i ];

				if ( entry.IsVisible && !entry.IsChecked )
				{
					areAllChecked = false;
					break;
				}
			}

			_IsBusy = true;
			IsChecked = areAllChecked;
			_IsBusy = false;
		}

		/// <summary>
		/// Determines whether this entry is filtered or not.
		/// </summary>
		/// <param name="name">Entry name.</param>
		public void Filter( string query )
		{
			bool allFiltered = true;
			
			foreach ( IUltimaPacketFilterEntry o in _Children )
			{
				if ( o.IsVisible )
				{
					o.Filter( query );

					if ( !o.IsFiltered )
						allFiltered = false;
				}
			}

			string str = ToString();

			if ( str.Contains( query ) )
				allFiltered = false;

			IsFiltered = allFiltered;
		}

		/// <summary>
		/// Saves entry to stream.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		public void Save( BinaryWriter writer )
		{
			writer.Write( (bool) true );
			writer.Write( (bool) IsVisible );
			writer.Write( (bool) IsChecked );

			foreach ( IUltimaPacketFilterEntry o in _Children )
				o.Save( writer );
		}

		/// <summary>
		/// Loads entry from stream.
		/// </summary>
		/// <param name="reeader">Reader to read from.</param>
		public void Load( BinaryReader reader )
		{
			try
			{
				// Supress changed event
				_IsBusy = true;

				// Read
				if ( _Parent == null )
					reader.ReadBoolean();

				IsVisible = reader.ReadBoolean();
				IsChecked = reader.ReadBoolean();

				foreach ( IUltimaPacketFilterEntry o in _Children )
				{
					bool type = reader.ReadBoolean();

					if ( type )
					{
						UltimaPacketFilterTable table = o as UltimaPacketFilterTable;

						if ( table == null )
							table = new UltimaPacketFilterTable( null, null, null, -1 ); // Dummy

						table.Load( reader );
					}
					else
					{
						UltimaPacketFilterEntry entry = o as UltimaPacketFilterEntry;

						if ( entry == null )
							entry = new UltimaPacketFilterEntry( null, null, null, -1 ); // Dummy

						entry.Load( reader );
					}
				}
			}
			finally
			{
				_IsBusy = false;
			}
		}

		/// <summary>
		/// Converts entry to string.
		/// </summary>
		/// <returns>String representing this entry.</returns>
		public override string ToString()
		{
			string name = "Packet Table";

			if ( _Definition != null )
				name = _Definition.Name;

			return String.Format( "0x{0:X2} - {1}", Index, name );
		}
		#endregion
	}
}
