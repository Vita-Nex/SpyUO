using System;
using System.Collections.Generic;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Descrbies property counter.
	/// </summary>
	public class UltimaPropertyCounter
	{
		#region Properties
		private int _Cliloc;

		/// <summary>
		/// Gets property cliloc.
		/// </summary>
		public int Cliloc
		{
			get { return _Cliloc; }
		}

		private int _Count;

		/// <summary>
		/// Gets property count.
		/// </summary>
		public int Count
		{
			get { return _Count; }
		}

		private double _Average;

		/// <summary>
		/// Gets average.
		/// </summary>
		public double Average
		{
			get { return _Average; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPropertyCounter.
		/// </summary>
		/// <param name="cliloc">Property cliloc.</param>
		public UltimaPropertyCounter( int cliloc )
		{
			_Cliloc = cliloc;
			_Count = 0;
			_Average = 0;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Counts property.
		/// </summary>
		/// <param name="value">Property value.</param>
		public virtual void Gotcha( object value )
		{
			_Count += 1;

			_Average *= ( 1.0 / _Count );
		}

		/// <summary>
		/// Converts object to string.
		/// </summary>
		/// <returns>Object string.</returns>
		public override string ToString()
		{
			if ( Globals.Instance.Clilocs != null )
			{
				string text = Globals.Instance.Clilocs.GetString( _Cliloc );

				if ( text != null )
					return String.Format( "{0}: {1}", text, _Count );
			}

			return String.Format( "{0}: {1}", Cliloc, _Count );
		}
		#endregion
	}

	/// <summary>
	/// Descrbies property range counter.
	/// </summary>
	public class UltimaPropertyRangeCounter : UltimaPropertyCounter
	{
		#region Properties
		private int _Min;

		/// <summary>
		/// Gets min property value.
		/// </summary>
		public int Min
		{
			get { return _Min; }
		}

		private int _Max;

		/// <summary>
		/// Gets max property value.
		/// </summary>
		public int Max
		{
			get { return _Max; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPropertyRangeCounter.
		/// </summary>
		/// <param name="cliloc">Property cliloc.</param>
		public UltimaPropertyRangeCounter( int cliloc ) : base( cliloc )
		{
			_Min = Int32.MaxValue;
			_Max = Int32.MinValue;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Counts property.
		/// </summary>
		/// <param name="value">Property value.</param>
		public override void Gotcha( object value )
		{
			base.Gotcha( value );

			if ( value is int )
			{
				int integer = (int) value;

				if ( integer < _Min )
					_Min = integer;

				if ( integer > _Max )
					_Max = integer;
			}
		}
		#endregion
	}

	/// <summary>
	/// Descrbies string property counter.
	/// </summary>
	public class UltimaPropertyCounterString : UltimaPropertyCounter
	{
		#region Properties
		private List<string> _Values;

		/// <summary>
		/// Gets a list of distinct property values.
		/// </summary>
		public List<string> Values
		{
			get { return _Values; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPropertyCounterString.
		/// </summary>
		/// <param name="cliloc">Property cliloc.</param>
		public UltimaPropertyCounterString( int cliloc ) : base( cliloc )
		{
			_Values = new List<string>();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Counts property.
		/// </summary>
		/// <param name="value">Property value.</param>
		public override void Gotcha( object value )
		{
			base.Gotcha( value );

			// Check if already have
			string valueString = value.ToString();
			bool alreadyHave = false;

			foreach ( string have in _Values )
			{
				if ( String.Equals( have, valueString, StringComparison.InvariantCultureIgnoreCase ) )
				{
					alreadyHave = true;
					break;
				}
			}

			if ( !alreadyHave )
				_Values.Add( valueString );
		}

		/// <summary>
		/// Returns a string representation of this counter.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString()
		{
			return String.Join( ",", _Values );
		}
		#endregion
	}

	/// <summary>
	/// Descrbies enum property counter.
	/// </summary>
	public class UltimaEnumPropertyCounter : UltimaPropertyCounter
	{
		#region Properties
		private List<int> _Values;

		/// <summary>
		/// Gets a list of distinct property values.
		/// </summary>
		public List<int> Values
		{
			get { return _Values; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaEnumPropertyCounter.
		/// </summary>
		/// <param name="cliloc">Property cliloc.</param>
		public UltimaEnumPropertyCounter( int cliloc = 0 ) : base( cliloc )
		{
			_Values = new List<int>();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Counts property.
		/// </summary>
		/// <param name="value">Property value.</param>
		public override void Gotcha( object value )
		{
			base.Gotcha( value );

			// Check if already have
			if ( value is int )
			{
				int integer = (int) value;
				bool alreadyHave = false;

				foreach ( int have in _Values )
				{
					if ( integer == have )
					{
						alreadyHave = true;
						break;
					}
				}

				if ( !alreadyHave )
					_Values.Add( integer );
			}
		}

		/// <summary>
		/// Returns a string representation of this counter.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString()
		{
			return String.Join( ",", _Values );
		}
		#endregion
	}
}
