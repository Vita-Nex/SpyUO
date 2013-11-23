using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for UltimaPacketTextureView.xaml
	/// </summary>
	public partial class UltimaPacketTextureView : UserControl
	{
		#region Properties
		/// <summary>
		/// Represents Texture property.
		/// </summary>
		public static readonly DependencyProperty TextureProperty = DependencyProperty.Register(
			"Texture", typeof( TextureFile ), typeof( UltimaPacketTextureView ), 
			new PropertyMetadata( null, new PropertyChangedCallback( Texture_Changed ) ) );

		/// <summary>
		/// Gets or sets texture file.
		/// </summary>
		public TextureFile Texture
		{
			get { return GetValue( TextureProperty ) as TextureFile; }
			set { SetValue( TextureProperty, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketTextureView.
		/// </summary>
		public UltimaPacketTextureView()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private void Save_Click( object sender, RoutedEventArgs e )
		{
			if ( Texture == null )
				return;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();

				if ( Texture.Data == null )
					dialog.Filter = "PNG Files|*.png|JPG Files|*.jpg|BMP Files|*.bmp";
				else
					dialog.Filter = "DDS Files|*.dds|PNG Files|*.png|JPG Files|*.jpg|BMP Files|*.bmp";

				dialog.CheckPathExists = true;
				dialog.Title = "Save File";
				dialog.FileName = Texture.Name;

				if ( dialog.ShowDialog() == true )
				{
					int filter = dialog.FilterIndex;

					if ( Texture.Data == null )
						filter += 1;

					if ( dialog.FilterIndex == 0 )
					{
						using ( FileStream stream = File.OpenWrite( dialog.FileName ) )
						{
							stream.Write( Texture.Data, 0, Texture.Data.Length );
						}
					}
					else
					{
						BitmapEncoder encoder = null;

						switch ( dialog.FilterIndex )
						{
							case 1: encoder = new PngBitmapEncoder(); break;
							case 2: encoder = new JpegBitmapEncoder(); break;
							case 3: encoder = new BmpBitmapEncoder(); break;
						}

						if ( encoder != null )
						{
							encoder.Frames.Add( BitmapFrame.Create( Texture.Image ) );

							using ( FileStream file = File.OpenWrite( dialog.FileName ) )
							{
								encoder.Save( file );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		#region Event Handlers
		private static void Texture_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketTextureView view = d as UltimaPacketTextureView;

			if ( view != null )
			{
				if ( view.Texture != null )
				{
					view.Image.Source = view.Texture.Image;
					view.ImageName.Text = view.Texture.Name;
					view.SaveButton.IsEnabled = true;
				}
				else
				{
					view.Image.Source = null;
					view.ImageName.Text = null;
					view.SaveButton.IsEnabled = false;
				}
			}
		}
		#endregion
		#endregion
	}
}
