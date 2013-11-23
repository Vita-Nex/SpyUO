using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Filter entry.
	/// </summary>
	public class UltimaPacketFilterEntry : DependencyObject, IUltimaPacketFilterEntry
	{
		#region Properties
		/// <summary>
		/// Represents IsVisible property.
		/// </summary>
		public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
			"IsVisible", typeof( bool ), typeof( UltimaPacketFilterEntry ), new PropertyMetadata( false ) );

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
			"IsChecked", typeof( bool ), typeof( UltimaPacketFilterEntry ),
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
			"IsFiltered", typeof( bool ), typeof( UltimaPacketFilterEntry ), new PropertyMetadata( false ) );

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
			"Index", typeof( int ), typeof( UltimaPacketFilterEntry ), new PropertyMetadata( -1 ) );

		/// <summary>
		/// Gets or sets entry index.
		/// </summary>
		public int Index
		{
			get { return (int) GetValue( IndexProperty ); }
			set { SetValue( IndexProperty, value ); }
		}

		/// <summary>
		/// Represents Properties property.
		/// </summary>
		public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(
			"Properties", typeof( List<UltimaPacketFilterProperty> ), typeof( UltimaPacketFilterEntry ), 
			new PropertyMetadata( null, new PropertyChangedCallback( Properties_Changed ) ) );

		/// <summary>
		/// Gets or sets filter properties.
		/// </summary>
		public List<UltimaPacketFilterProperty> Properties
		{
			get { return GetValue( PropertiesProperty ) as List<UltimaPacketFilterProperty>; }
			set { SetValue( PropertiesProperty, value ); }
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
		/// Gets packet parent.
		/// </summary>
		public UltimaPacketFilterTable Parent
		{
			get { return _Parent; }
		}

		private UltimaPacketDefinition _Definition;

		/// <summary>
		/// Gets packet info.
		/// </summary>
		public UltimaPacketDefinition Definition
		{
			get { return _Definition; }
		}

		/// <summary>
		/// Determines whether this entry has definition.
		/// </summary>
		public bool HasDefinition
		{
			get { return _Definition != null; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketFilterEntry.
		/// </summary>
		/// <param name="owner">Owner.</param>
		/// <param name="parent">Entry parent.</param>
		/// <param name="index">Entry index.</param>
		/// <param name="definition">Packet definition.</param>
		public UltimaPacketFilterEntry( UltimaPacketDefinition definition, UltimaPacketFilter owner, UltimaPacketFilterTable parent, int index )
		{
			_Definition = definition;
			_Owner = owner;
			_Parent = parent;
			Index = index;

			if ( definition != null )
			{
				IsVisible = true;
				IsChecked = true;
			}
		}
		#endregion

		#region Methods
		private static void Properties_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterEntry entry = d as UltimaPacketFilterEntry;

			if ( entry != null && entry.Owner != null )
				entry.Owner.OnChange();

		}

		private static void IsChecked_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterEntry entry = d as UltimaPacketFilterEntry;

			if ( entry != null )
				entry.UpdateIsChecked();
		}

		private void UpdateIsChecked()
		{
			if ( _Parent != null && !_Parent.IsBusy )
			{
				_Parent.AreAllChecked();

				if ( _Owner != null )
					_Owner.OnChange();
			}
		}

		/// <summary>
		/// Shows all packets.
		/// </summary>
		public void ShowAll()
		{
			IsVisible = true;
		}

		/// <summary>
		/// Hides unknown packets.
		/// </summary>
		public void HideUnknown()
		{
			if ( _Definition == null )
			{
				IsVisible = false;
				IsChecked = false;
			}
			else
				IsVisible = true;
		}

		/// <summary>
		/// Determines whether this entry is filtered or not.
		/// </summary>
		/// <param name="name">Entry name.</param>
		public void Filter( string query )
		{
			bool filtered = false;

			if ( !String.IsNullOrWhiteSpace( query ) )
			{
				string name = ToString().ToLower();

				if ( !name.Contains( query.ToLower() ) )
					filtered = true;
			}

			IsFiltered = filtered;
		}

		/// <summary>
		/// Saves entry to stream.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		public void Save( BinaryWriter writer )
		{
			writer.Write( (bool) false );
			writer.Write( (bool) IsVisible );
			writer.Write( (bool) IsChecked );

			if ( Properties != null )
			{
				writer.Write( (int) Properties.Count );

				foreach ( UltimaPacketFilterProperty property in Properties )
					property.Save( writer );
			}
			else
				writer.Write( (int) 0 );

			if ( _Definition != null )
				writer.Write( (bool) true );
			else
				writer.Write( (bool) false );
		}

		/// <summary>
		/// Loads entry from stream.
		/// </summary>
		/// <param name="reeader">Reader to read from.</param>
		public void Load( BinaryReader reader )
		{
			IsVisible = reader.ReadBoolean();
			IsChecked = reader.ReadBoolean();

			int propertyCount = reader.ReadInt32();

			if ( propertyCount > 0 )
			{
				List<UltimaPacketFilterProperty> properties = new List<UltimaPacketFilterProperty>();

				for ( int i = 0; i < propertyCount; i++ )
				{
					UltimaPacketFilterProperty property = new UltimaPacketFilterProperty( this );
					
					if ( property.Load( reader ) )
						properties.Add( property );
				}

				Properties = properties;
			}

			bool definition = reader.ReadBoolean();

			if ( !definition && _Definition != null )
			{
				IsVisible = true;
				IsChecked = true;
			}
		}

		/// <summary>
		/// Determines whether packet is displayed.
		/// </summary>
		/// <param name="packet">Packet to check.</param>
		/// <returns>True if displayed, false if not.</returns>
		public bool IsDisplayed( UltimaPacket packet )
		{
			List<UltimaPacketFilterProperty> properties = Properties;

			if ( properties == null )
				return true;

			foreach ( UltimaPacketFilterProperty property in properties )
			{
				if ( property.IsChecked && !property.IsDisplayed( packet ) )
					return false;
			}

			return true;
		}

		/// <summary>
		/// Converts entry to string.
		/// </summary>
		/// <returns>String representing this entry.</returns>
		public override string ToString()
		{
			if ( _Definition != null )
				return String.Format( "0x{0:X2} - {1}", Index, _Definition.Attribute.Name );

			return String.Format( "0x{0:X2} - Unknown Packet", Index );
		}
		#endregion
	}
}
