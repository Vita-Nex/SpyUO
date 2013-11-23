using System.Windows;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes audio file.
	/// </summary>
	public class AudioFile : DependencyObject
	{
		#region Properties
		/// <summary>
		/// Represents ID property.
		/// </summary>
		public static readonly DependencyProperty IDProperty = DependencyProperty.Register(
			"ID", typeof( int ), typeof( AudioFile ), new PropertyMetadata( 0 ) );

		/// <summary>
		/// Gets or sets ID.
		/// </summary>
		public int ID
		{
			get { return (int) GetValue( IDProperty ); }
			set { SetValue( IDProperty, value ); }
		}

		/// <summary>
		/// Represents Data property.
		/// </summary>
		public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
			"Data", typeof( byte[] ), typeof( AudioFile ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets data.
		/// </summary>
		public byte[] Data
		{
			get { return GetValue( DataProperty ) as byte[]; }
			set { SetValue( DataProperty, value ); }
		}

		/// <summary>
		/// Represents Name property.
		/// </summary>
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name", typeof( string ), typeof( AudioFile ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets name.
		/// </summary>
		public string Name
		{
			get { return GetValue( NameProperty ) as string; }
			set { SetValue( NameProperty, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of AudioFile.
		/// </summary>
		/// <param name="id">Audio ID.</param>
		/// <param name="data">Audio data.</param>
		/// <param name="name">Audio name.</param>
		public AudioFile( int id, byte[] data, string name )
		{
			ID = id;
			Data = data;
			Name = name;
		}
		#endregion
	}
}
