using System;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes simple counter.
	/// </summary>
	public class UltimaSimpleCounter
	{
		#region Properties
		private int _Total;

		/// <summary>
		/// Gets total item count.
		/// </summary>
		public int Total
		{
			get { return _Total; }
		}

		private int _Min;

		/// <summary>
		/// Gets min items.
		/// </summary>
		public int Min
		{
			get { return _Min; }
		}

		private int _Max;

		/// <summary>
		/// Gets max.
		/// </summary>
		public int Max
		{
			get { return _Max; }
		}

		protected int _InternalCounter;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaSimpleCounter.
		/// </summary>
		public UltimaSimpleCounter()
		{
			_Total = 0;
			_Min = Int32.MaxValue;
			_Max = Int32.MinValue;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Starts analyzing corpse.
		/// </summary>
		public virtual void StartAnalyzing()
		{
			_InternalCounter = 0;
		}

		/// <summary>
		/// Ends analyzing corpse.
		/// </summary>
		public virtual void EndAnalyzing()
		{
			_Total += _InternalCounter;

			if ( _InternalCounter < _Min )
				_Min = _InternalCounter;

			if ( _InternalCounter > _Max )
				_Max = _InternalCounter;
		}

		/// <summary>
		/// Counts item.
		/// </summary>
		/// <param name="amount">Item amount.</param>
		public virtual void Gotcha( int amount = 1 )
		{
			_InternalCounter += amount;
		}
		#endregion
	}

	/// <summary>
	/// Describes item counter.
	/// </summary>
	public class UltimaItemCounter : UltimaSimpleCounter
	{
		#region Properties
		private uint _Serial;

		/// <summary>
		/// Gets first item serial.
		/// </summary>
		public uint Serial
		{
			get { return _Serial; }
		}

		private string _Name;

		/// <summary>
		/// Gets item name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}

		private int _MinAmountPerCorpse;

		/// <summary>
		/// Gets min amount per corpse.
		/// </summary>
		public int MinAmountPerCorpse
		{
			get { return _MinAmountPerCorpse; }
		}

		private int _MaxAmountPerCorpse;

		/// <summary>
		/// Gets max amount per corpse.
		/// </summary>
		public int MaxAmountPerCorpse
		{
			get { return _MaxAmountPerCorpse; }
		}

		private UltimaEnumPropertyCounter _Hues;

		/// <summary>
		/// Gets hue counter.
		/// </summary>
		public UltimaEnumPropertyCounter Hues
		{
			get { return _Hues; }
		}

		private UltimaPropertyRangeCounter _Amount;

		/// <summary>
		/// Gets amount counter.
		/// </summary>
		public UltimaPropertyRangeCounter Amount
		{
			get { return _Amount; }
		}

		private double _Chance;

		/// <summary>
		/// Gets chance 
		/// </summary>
		public double Chance
		{
			get { return _Chance; }
		}

		private int _ChanceCounter;
		private int _TotalChanceCounter;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaItemCounter.
		/// </summary>
		/// <param name="serial">First item serial.</param>
		public UltimaItemCounter( uint serial, string name ) : base()
		{
			_Serial = serial;
			_Name = name;
			_Chance = 0;
			_MinAmountPerCorpse = Int32.MaxValue;
			_MaxAmountPerCorpse = Int32.MinValue;
			_Hues = new UltimaEnumPropertyCounter( 0 );
			_Amount = new UltimaPropertyRangeCounter( 0 );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Ends analyzing corpse.
		/// </summary>
		public override void EndAnalyzing()
		{
			base.EndAnalyzing();

			_TotalChanceCounter += 1;

			if ( _InternalCounter > 0 )
				_ChanceCounter += 1;

			_Chance = _ChanceCounter * 100.0 / _TotalChanceCounter;
		}

		/// <summary>
		/// Counts item.
		/// </summary>
		/// <param name="hue">Item hue.</param>
		/// <param name="amount">Item amount.</param>
		public void Gotcha( int hue, int amount )
		{
			base.Gotcha( amount );

			_Hues.Gotcha( hue );
			_Amount.Gotcha( amount );
		}
		#endregion
	}
}
