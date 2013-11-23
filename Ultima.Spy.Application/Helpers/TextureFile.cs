using System.Windows;
using System.Windows.Media.Imaging;
using Ultima.Package;
using System.IO;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes texture file.
	/// </summary>
	public class TextureFile : DependencyObject
	{
		#region Properties
		/// <summary>
		/// Represents ID property.
		/// </summary>
		public static readonly DependencyProperty IDProperty = DependencyProperty.Register(
			"ID", typeof( int ), typeof( TextureFile ), new PropertyMetadata( 0 ) );

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
			"Data", typeof( byte[] ), typeof( TextureFile ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets data.
		/// </summary>
		public byte[] Data
		{
			get { return GetValue( DataProperty ) as byte[]; }
			set { SetValue( DataProperty, value ); }
		}

		/// <summary>
		/// Represents Image property.
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image", typeof( BitmapSource ), typeof( TextureFile ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets image.
		/// </summary>
		public BitmapSource Image
		{
			get { return GetValue( ImageProperty ) as BitmapSource; }
			set { SetValue( ImageProperty, value ); }
		}

		/// <summary>
		/// Represents Name property.
		/// </summary>
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name", typeof( string ), typeof( TextureFile ), new PropertyMetadata( null ) );

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
		/// Cosntructs a new instance of TextureFile.
		/// </summary>
		public TextureFile()
		{
		}
		#endregion
	}
}
