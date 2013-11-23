using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Used for parsing.
	/// </summary>
	public class UltimaPacketFilterParser
	{
		#region Descriptors
		/// <summary>
		/// Gets long description for operation..
		/// </summary>
		/// <param name="operation">Operation to describe.</param>
		/// <returns>Operation description.</returns>
		public static string GetLongDescription( UltimaPacketFilterTypeOperation operation )
		{
			string start = "Packet is visible if value of the property {0} ";

			switch ( operation )
			{
				case UltimaPacketFilterTypeOperation.Greater: return start + "is more than {1}";
				case UltimaPacketFilterTypeOperation.Lesser: return start + "is less than {1}";
				case UltimaPacketFilterTypeOperation.Contains: return start + "contains one of the values in {1}";
				case UltimaPacketFilterTypeOperation.In: return start + "is in {1}";
			}

			return null;
		}

		/// <summary>
		/// Gets short description for operation..
		/// </summary>
		/// <param name="operation">Operation to describe.</param>
		/// <returns>Operation description.</returns>
		public static string GetShortDescription( UltimaPacketFilterTypeOperation operation )
		{
			string start = "{0} ";

			switch ( operation )
			{
				case UltimaPacketFilterTypeOperation.Greater: return start + "> {1}";
				case UltimaPacketFilterTypeOperation.Lesser: return start + "< {1}";
				case UltimaPacketFilterTypeOperation.Contains: return start + "\u2208 {1}";
				case UltimaPacketFilterTypeOperation.In: return start + "\u220B {1}";
			}

			return null;
		}

		/// <summary>
		/// Gets first sample for specific type.
		/// </summary>
		/// <param name="code">Type code.</param>
		/// <returns>First sample.</returns>
		public static string GetSampleOne( TypeCode code )
		{
			switch ( code )
			{
				case TypeCode.Boolean: return "True";
				case TypeCode.Char: return "a";
				case TypeCode.Single:
				case TypeCode.Decimal:
				case TypeCode.Double: return "2.35";
				case TypeCode.Byte: return "5";
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: return "100";
				case TypeCode.String: return "awesome";
			}

			return null;
		}

		/// <summary>
		/// Gets second sample for specific type.
		/// </summary>
		/// <param name="code">Type code.</param>
		/// <returns>Second sample.</returns>
		public static string GetSampleTwo( TypeCode code )
		{
			switch ( code )
			{
				case TypeCode.Boolean: return "True";
				case TypeCode.Char: return "\\u0061";
				case TypeCode.Single:
				case TypeCode.Decimal:
				case TypeCode.Double: return "2.35";
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: return "0x44";
				case TypeCode.String: return "justin gayber";
			}

			return null;
		}

		/// <summary>
		/// Gets sample list for specific type.
		/// </summary>
		/// <param name="code">Type code.</param>
		/// <returns>Sample list.</returns>
		public static string GetSampleList( TypeCode code )
		{
			switch ( code )
			{
				case TypeCode.Boolean: return "True,False";
				case TypeCode.Char: return "c,r,\\u0061,p";
				case TypeCode.Single:
				case TypeCode.Decimal:
				case TypeCode.Double: return "2.35,5,3.0";
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: return "100,0xA,7C";
				case TypeCode.String: return "al,bundy,tinkles";
			}

			return null;
		}

		/// <summary>
		/// Gets sample for property.
		/// </summary>
		/// <param name="property">Property definition.</param>
		/// <param name="operation">Property operation.</param>
		/// <param name="alternate">Determines whether to compose alternate sample.</param>
		/// <returns>Composed sample.</returns>
		public static string GetSample( UltimaPacketPropertyDefinition property, UltimaPacketFilterTypeOperation operation, bool alternate )
		{
			TypeCode code = Type.GetTypeCode( property.Info.PropertyType );

			if ( operation == UltimaPacketFilterTypeOperation.Contains || operation == UltimaPacketFilterTypeOperation.In )
				return GetSampleList( code );
			else if ( alternate )
				return GetSampleOne( code );
			else
				return GetSampleTwo( code );
		}

		/// <summary>
		/// Gets sample description for property.
		/// </summary>
		/// <param name="property">Property definition.</param>
		/// <param name="operation">Property operation.</param>
		/// <param name="shorty">Determines whether to compose short sample.</param>
		/// <param name="alternate">Determines whether to compose alternate sample.</param>
		/// <returns>Composed sample.</returns>
		public static string GetComposedSample( UltimaPacketPropertyDefinition property, UltimaPacketFilterTypeOperation operation, bool shorty, bool alternate )
		{
			string format = null;

			if ( shorty )
				format = GetShortDescription( operation );
			else
				format = GetLongDescription( operation );

			if ( format != null )
			{
				TypeCode code = Type.GetTypeCode( property.Info.PropertyType );
				string sample = null;

				if ( operation == UltimaPacketFilterTypeOperation.Contains || operation == UltimaPacketFilterTypeOperation.In )
					sample = GetSampleList( code );
				else if ( alternate )
					sample = GetSampleOne( code );
				else
					sample = GetSampleTwo( code );

				if ( sample != null )
					return String.Format( format, property, sample );
			}

			return null;
		}

		/// <summary>
		/// Gets short composed filter.
		/// </summary>
		/// <param name="property">Filter property.</param>
		/// <param name="operation">Filter operation.</param>
		/// <param name="value">Filter value.</param>
		/// <returns>Composed filter.</returns>
		public static string GetComposedFilter( UltimaPacketPropertyDefinition property, UltimaPacketFilterTypeOperation operation, string value )
		{
			string format = GetShortDescription( operation );

			if ( format != null )
				return String.Format( format, property, value );

			return null;
		}
		#endregion

		#region Operations
		private static UltimaPacketFilterTypeOperation[] _BooleanOperations = new UltimaPacketFilterTypeOperation[]
		{
			UltimaPacketFilterTypeOperation.In
		};

		private static UltimaPacketFilterTypeOperation[] _NumberOperations = new UltimaPacketFilterTypeOperation[]
		{
			UltimaPacketFilterTypeOperation.Greater, 
			UltimaPacketFilterTypeOperation.Lesser,
			UltimaPacketFilterTypeOperation.In
		};
		private static UltimaPacketFilterTypeOperation[] _StringOperations = new UltimaPacketFilterTypeOperation[]
		{
			UltimaPacketFilterTypeOperation.Contains,
			UltimaPacketFilterTypeOperation.In
		};

		/// <summary>
		/// Determines if operation is valid for sepecific property.
		/// </summary>
		/// <param name="property">Property to check for.</param>
		/// <param name="operation">Operation to validate.</param>
		/// <param name="first">First valid operation if operation is not valid.</param>
		/// <returns>True if valid, false otherwise.</returns>
		public static bool IsValidOperation( UltimaPacketPropertyDefinition property, UltimaPacketFilterTypeOperation operation, ref UltimaPacketFilterTypeOperation first )
		{
			UltimaPacketFilterTypeOperation[] validOperations = GetTypeOperations( property );

			if ( validOperations != null )
			{
				foreach ( UltimaPacketFilterTypeOperation typeOperation in validOperations )
				{
					if ( operation == typeOperation )
						return true;
				}

				first = validOperations[ 0 ];
				return false;
			}

			return false;
		}

		/// <summary>
		/// Gets a list of operations supported by specific type.
		/// </summary>
		/// <param name="property">Property to get operations for.</param>
		/// <returns>Array of operations.</returns>
		public static UltimaPacketFilterTypeOperation[] GetTypeOperations( UltimaPacketPropertyDefinition property )
		{
			List<UltimaPacketFilterTypeOperation> operations = new List<UltimaPacketFilterTypeOperation>();
			TypeCode code = Type.GetTypeCode( property.Info.PropertyType );

			switch ( code )
			{
				case TypeCode.Boolean: return _BooleanOperations;
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64: return _NumberOperations;
				case TypeCode.String: return _StringOperations;
			}

			return null;
		}
		#endregion

		#region Value Parsing
		/// <summary>
		/// Tries to parse string into specific type based on operation.
		/// </summary>
		/// <param name="value">Value to parse.</param>
		/// <param name="property">Property to parse to.</param>
		/// <param name="operation">Operation to parse for.</param>
		/// <param name="result">Result.</param>
		/// <returns>True if successful, false otherwise.</returns>
		public static bool TryParse( string value, UltimaPacketPropertyDefinition property, UltimaPacketFilterTypeOperation operation, out object result )
		{
			result = null;

			if ( String.IsNullOrEmpty( value ) )
				return false;

			TypeCode code = Type.GetTypeCode( property.Info.PropertyType );

			switch ( operation )
			{
				case UltimaPacketFilterTypeOperation.Greater:
				case UltimaPacketFilterTypeOperation.Lesser:
				{
					return TryParseSingle( value, code, ref result );
				}
				case UltimaPacketFilterTypeOperation.Contains:
				case UltimaPacketFilterTypeOperation.In:
				{
					string[] split = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

					if ( split.Length > 0 )
					{
						switch ( code )
						{
							case TypeCode.Boolean: result = TryParseList<Boolean>( split ); break;
							case TypeCode.Char: result = TryParseList<Char>( split ); break;
							case TypeCode.Byte: result = TryParseList<Byte>( split ); break;
							case TypeCode.Decimal: result = TryParseList<Decimal>( split ); break;
							case TypeCode.Double: result = TryParseList<Double>( split ); break;
							case TypeCode.Int16: result = TryParseList<Int16>( split ); break;
							case TypeCode.Int32: result = TryParseList<Int32>( split ); break;
							case TypeCode.Int64: result = TryParseList<Int64>( split ); break;
							case TypeCode.SByte: result = TryParseList<SByte>( split ); break;
							case TypeCode.Single: result = TryParseList<Single>( split ); break;
							case TypeCode.String: result = TryParseList<String>( split ); break;
							case TypeCode.UInt16: result = TryParseList<UInt16>( split ); break;
							case TypeCode.UInt32: result = TryParseList<UInt32>( split ); break;
							case TypeCode.UInt64: result = TryParseList<UInt64>( split ); break;
						}

						if ( result != null )
							return true;
					}

					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Tries to parse list of values into specific type.
		/// </summary>
		/// <typeparam name="T">Type to parse into.</typeparam>
		/// <param name="values">Array of values to parse.</param>
		/// <returns>List of parsed values if successul, null otherwise.</returns>
		public static List<T> TryParseList<T>( string[] values )
		{
			List<T> list = new List<T>();
			TypeCode code = Type.GetTypeCode( typeof( T ) );

			foreach ( string value in values )
			{
				object parsed = null;

				if ( !TryParseSingle( value, code, ref parsed ) )
					return null;

				list.Add( (T) parsed );
			}

			return list;
		}

		/// <summary>
		/// Tries to parse value into specific type.
		/// </summary>
		/// <param name="value">Value to parse.</param>
		/// <param name="code">Type code to parse into.</param>
		/// <param name="output">Output.</param>
		/// <returns>True if successful, false otherwise.</returns>
		public static bool TryParseSingle( string value, TypeCode code, ref object output )
		{
			if ( String.IsNullOrEmpty( value ) )
				return false;

			bool valid = false;
			NumberStyles flag = NumberStyles.None;

			if ( value.StartsWith( "0x" ) || value.StartsWith( "0X" ) )
			{
				value = value.Remove( 0, 2 );
				flag = NumberStyles.AllowHexSpecifier;
			}

			switch ( code )
			{
				case TypeCode.Boolean: Boolean o1; valid = Boolean.TryParse( value, out o1 ); output = o1; break;
				case TypeCode.Byte: Byte o2; valid = Byte.TryParse( value, flag, CultureInfo.InvariantCulture, out o2 ); output = o2; break;
				case TypeCode.Char: Char o3; valid = Char.TryParse( value, out o3 ); output = o3; break;
				case TypeCode.Decimal: Decimal o4; valid = Decimal.TryParse( value, out o4 ); output = o4; break;
				case TypeCode.Double: Double o5; valid = Double.TryParse( value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out o5 ); output = o5; break;
				case TypeCode.Int16: Int16 o6; valid = Int16.TryParse( value, flag, CultureInfo.InvariantCulture, out o6 ); output = o6; break;
				case TypeCode.Int32: Int32 o7; valid = Int32.TryParse( value, flag, CultureInfo.InvariantCulture, out o7 ); output = o7; break;
				case TypeCode.Int64: Int64 o8; valid = Int64.TryParse( value, flag, CultureInfo.InvariantCulture, out o8 ); output = o8; break;
				case TypeCode.SByte: SByte o9; valid = SByte.TryParse( value, out o9 ); output = o9; break;
				case TypeCode.Single: Single o10; valid = Single.TryParse( value, out o10 ); output = o10; break;
				case TypeCode.String: valid = true; output = value.ToLower(); break;
				case TypeCode.UInt16: UInt16 o12; valid = UInt16.TryParse( value, flag, CultureInfo.InvariantCulture, out o12 ); output = o12; break;
				case TypeCode.UInt32: UInt32 o13; valid = UInt32.TryParse( value, flag, CultureInfo.InvariantCulture, out o13 ); output = o13; break;
				case TypeCode.UInt64: UInt64 o14; valid = UInt64.TryParse( value, flag, CultureInfo.InvariantCulture, out o14 ); output = o14; break;
			}

			return valid;
		}
		#endregion
	}
}
