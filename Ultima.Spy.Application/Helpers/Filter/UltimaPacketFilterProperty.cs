using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.IO;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Property filter operation.
	/// </summary>
	public enum UltimaPacketFilterTypeOperation
	{
		/// <summary>
		/// Determines if property value is greater than.
		/// </summary>
		Greater,

		/// <summary>
		/// Determines if property value is lesser than.
		/// </summary>
		Lesser,

		/// <summary>
		/// Determines if property value contains substring (can be a comma separated string).
		/// </summary>
		Contains,

		/// <summary>
		/// Determines if property value equals to one of the items in comma separated list.
		/// </summary>
		In,
	}

	/// <summary>
	/// Represents filter property.
	/// </summary>
	public class UltimaPacketFilterProperty : DependencyObject
	{
		#region Properties
		/// <summary>
		/// Represents IsChecked property.
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
			"IsChecked", typeof( bool ), typeof( UltimaPacketFilterProperty ), 
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
		/// Represents Definition property.
		/// </summary>
		public static readonly DependencyProperty DefinitionProperty = DependencyProperty.Register(
			"Definition", typeof( UltimaPacketPropertyDefinition ), typeof( UltimaPacketFilterProperty ),
			new PropertyMetadata( null, new PropertyChangedCallback( Definition_Changed ) ) );

		/// <summary>
		/// Gets or sets property definition.
		/// </summary>
		public UltimaPacketPropertyDefinition Definition
		{
			get { return (UltimaPacketPropertyDefinition) GetValue( DefinitionProperty ); }
			set { SetValue( DefinitionProperty, value ); }
		}

		/// <summary>
		/// Represents Operation property.
		/// </summary>
		public static readonly DependencyProperty OperationProperty = DependencyProperty.Register(
			"Operation", typeof( UltimaPacketFilterTypeOperation ), typeof( UltimaPacketFilterProperty ), 
			new PropertyMetadata( UltimaPacketFilterTypeOperation.Greater, new PropertyChangedCallback( Operation_Changed ) ) );

		/// <summary>
		/// Gets or sets property operation.
		/// </summary>
		public UltimaPacketFilterTypeOperation Operation
		{
			get { return (UltimaPacketFilterTypeOperation) GetValue( OperationProperty ); }
			set { SetValue( OperationProperty, value ); }
		}

		/// <summary>
		/// Represents Value property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value", typeof( string ), typeof( UltimaPacketFilterProperty ),
			new PropertyMetadata( null, new PropertyChangedCallback( Value_Changed ) ) );

		/// <summary>
		/// Gets or sets property value.
		/// </summary>
		public string Value
		{
			get { return GetValue( ValueProperty ) as string; }
			set { SetValue( ValueProperty, value ); }
		}

		/// <summary>
		/// Represents Text property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof( string ), typeof( UltimaPacketFilterProperty ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets property display text.
		/// </summary>
		public string Text
		{
			get { return GetValue( TextProperty ) as string; }
			set { SetValue( TextProperty, value ); }
		}

		/// <summary>
		/// Represents IsValid property.
		/// </summary>
		public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(
			"IsValid", typeof( bool ), typeof( UltimaPacketFilterProperty ), new PropertyMetadata( false ) );

		/// <summary>
		/// Determines whether value is valid.
		/// </summary>
		public bool IsValid
		{
			get { return (bool) GetValue( IsValidProperty ); }
			set { SetValue( IsValidProperty, value ); }
		}

		private UltimaPacketFilterEntry _Parent;

		/// <summary>
		/// Gets property parent.
		/// </summary>
		public UltimaPacketFilterEntry Parent
		{
			get { return _Parent; }
		}

		private TypeCode _Code;
		private object _Value;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketFilterProperty.
		/// </summary>
		/// <param name="parent">Property parent.</param>
		public UltimaPacketFilterProperty( UltimaPacketFilterEntry parent )
		{
			_Parent = parent;
		}
		#endregion

		#region Methods
		private static void IsChecked_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterProperty property = d as UltimaPacketFilterProperty;

			if ( property != null && property.Parent != null && property.Parent.Owner != null )
				property.Parent.Owner.OnChange();
		}

		private static void Definition_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterProperty property = d as UltimaPacketFilterProperty;

			if ( property != null )
			{
				property._Code = Type.GetTypeCode( property.Definition.Info.PropertyType );

				// Check operation
				if ( property.Definition != null )
				{
					UltimaPacketFilterTypeOperation operation = UltimaPacketFilterTypeOperation.Greater;

					if ( !UltimaPacketFilterParser.IsValidOperation( property.Definition, property.Operation, ref operation ) )
						property.Operation = operation;
				}

				property.Validate();
			}
		}

		private static void Operation_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterProperty property = d as UltimaPacketFilterProperty;

			if ( property != null )
				property.Validate();
		}

		private static void Value_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilterProperty property = d as UltimaPacketFilterProperty;

			if ( property != null )
				property.Validate();
		}

		private void Validate()
		{
			try
			{
				IsValid = UltimaPacketFilterParser.TryParse( Value, Definition, Operation, out _Value );
			}
			catch
			{
				IsValid = false;
			}

			Text = ToString();
		}

		/// <summary>
		/// Determines whether packet is displayed.
		/// </summary>
		/// <param name="packet">Packet to check.</param>
		/// <returns>True if displayed, false otherwise.</returns>
		public bool IsDisplayed( UltimaPacket packet )
		{
			object value = Definition.Getter( packet );

			switch ( _Code )
			{
				case TypeCode.Boolean:
				{
					Boolean actualValue = (Boolean) value;

					if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Boolean> list = (List<Boolean>) _Value;

						foreach ( Boolean v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Byte:
				{
					Byte actualValue = (Byte) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Byte compareWith = (Byte) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Byte compareWith = (Byte) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Byte> list = (List<Byte>) _Value;

						foreach ( Byte v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Char:
				{
					Char actualValue = (Char) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Char compareWith = (Char) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Char compareWith = (Char) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Char> list = (List<Char>) _Value;

						foreach ( Char v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Decimal:
				{
					Decimal actualValue = (Decimal) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Decimal compareWith = (Decimal) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Decimal compareWith = (Decimal) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Decimal> list = (List<Decimal>) _Value;

						foreach ( Decimal v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Double:
				{
					Double actualValue = (Double) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Double compareWith = (Double) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Double compareWith = (Double) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Double> list = (List<Double>) _Value;

						foreach ( Double v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Int16:
				{
					Int16 actualValue = (Int16) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Int16 compareWith = (Int16) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Int16 compareWith = (Int16) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Int16> list = (List<Int16>) _Value;

						foreach ( Int16 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Int32:
				{
					Int32 actualValue = (Int32) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Int32 compareWith = (Int32) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Int32 compareWith = (Int32) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Int32> list = (List<Int32>) _Value;

						foreach ( Int32 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Int64:
				{
					Int64 actualValue = (Int64) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Int64 compareWith = (Int64) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Int64 compareWith = (Int64) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Int64> list = (List<Int64>) _Value;

						foreach ( Int64 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.SByte:
				{
					SByte actualValue = (SByte) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						SByte compareWith = (SByte) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						SByte compareWith = (SByte) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<SByte> list = (List<SByte>) _Value;

						foreach ( SByte v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.Single:
				{
					Single actualValue = (Single) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						Single compareWith = (Single) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						Single compareWith = (Single) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<Single> list = (List<Single>) _Value;

						foreach ( Single v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.String:
				{
					String actualValue = ( (String) value ).ToLower();

					if ( Operation == UltimaPacketFilterTypeOperation.Contains )
					{
						List<String> list = (List<String>) _Value;

						foreach ( String v in list )
							if ( actualValue.Contains( v ) )
								return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<String> list = (List<String>) _Value;

						foreach ( String v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.UInt16:
				{
					UInt16 actualValue = (UInt16) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						UInt16 compareWith = (UInt16) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						UInt16 compareWith = (UInt16) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<UInt16> list = (List<UInt16>) _Value;

						foreach ( UInt16 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.UInt32:
				{
					UInt32 actualValue = (UInt32) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						UInt32 compareWith = (UInt32) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						UInt32 compareWith = (UInt32) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<UInt32> list = (List<UInt32>) _Value;

						foreach ( UInt32 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
				case TypeCode.UInt64:
				{
					UInt64 actualValue = (UInt64) value;

					if ( Operation == UltimaPacketFilterTypeOperation.Greater )
					{
						UInt64 compareWith = (UInt64) _Value;

						if ( actualValue > compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.Lesser )
					{
						UInt64 compareWith = (UInt64) _Value;

						if ( actualValue < compareWith )
							return true;
					}
					else if ( Operation == UltimaPacketFilterTypeOperation.In )
					{
						List<UInt64> list = (List<UInt64>) _Value;

						foreach ( UInt64 v in list )
							if ( actualValue == v )
								return true;
					}

					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns string representaion of this class.
		/// </summary>
		/// <returns>String representaion.</returns>
		public override string ToString()
		{
			if ( _Value == null || !IsValid )
				return "Invalid";

			return UltimaPacketFilterParser.GetComposedFilter( Definition, Operation, Value );
		}

		/// <summary>
		/// Shallow clone.
		/// </summary>
		/// <returns>Clone.</returns>
		public UltimaPacketFilterProperty Clone()
		{
			return (UltimaPacketFilterProperty) MemberwiseClone();
		}

		/// <summary>
		/// Saves entry to stream.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		public void Save( BinaryWriter writer )
		{
			writer.Write( (bool) IsChecked );
			writer.Write( (string) Definition.Info.Name );
			writer.Write( (int) Operation );
			writer.Write( (string) Value );
		}

		/// <summary>
		/// Loads entry from stream.
		/// </summary>
		/// <param name="reeader">Reader to read from.</param>
		/// <returns>True if packet property was found, false otherwise.</returns>
		public bool Load( BinaryReader reader )
		{
			IsChecked = reader.ReadBoolean();

			string propName = reader.ReadString();

			foreach ( UltimaPacketPropertyDefinition definition in _Parent.Definition.Properties )
			{
				if ( propName == definition.Info.Name )
				{
					Definition = definition;
					break;
				}
			}

			Operation = (UltimaPacketFilterTypeOperation) reader.ReadInt32();
			Value = reader.ReadString();

			return Definition != null;
		}
		#endregion
	}
}
