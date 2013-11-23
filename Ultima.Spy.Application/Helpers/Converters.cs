using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Ultima.Package;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Extracts values from packet.
	/// </summary>
	public class UltimaPacketValueConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			UltimaPacket packet = value as UltimaPacket;

			if ( packet != null )
				return new UltimaPacketValue( packet );

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Converts property value to string.
	/// </summary>
	public class UltimaPacketPropertyValueConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			UltimaPacketPropertyValue property = value as UltimaPacketPropertyValue;

			if ( property != null )
			{
				if ( property.Definition.Attribute.Format != null )
					return String.Format( property.Definition.Attribute.Format, property.Value );

				if ( property.Value != null )
					return property.Value.ToString();
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Gets packet title.
	/// </summary>
	public class UltimaPacketTitleConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			UltimaPacketValue packetValue = value as UltimaPacketValue;

			if ( packetValue != null )
			{
				UltimaPacket packet = packetValue.Object as UltimaPacket;

				if ( packet != null )
					return String.Format( "{0} - {1}", packet.Ids, packet.Name );
			}

			return "Packet Properties";
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Packet direction converter.
	/// </summary>
	public class UltimaPacketDirectionConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is bool )
			{
				if ( (bool) value )
					return "From Client";
				else
					return "From Server";
			}

			return String.Empty;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Packet direction converter.
	/// </summary>
	public class UltimaPacketTimeConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is DateTime )
			{
				DateTime actual = (DateTime) value;

				return actual.ToString( "H:mm:ss fff" );
			}

			return String.Empty;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Gets music file from musicID.
	/// </summary>
	public class UltimaPacketMusicConverter : IValueConverter
	{
		#region Properties
		private SimpleCache<int, AudioFile> _Music;
		#endregion

		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is int == false )
				return null;

			if ( _Music == null )
			{
				_Music = new SimpleCache<int, AudioFile>( 10 );
				_Music.Getter += new SimpleCacheGetter<int, AudioFile>( Music_Getter );
			}

			return _Music.Get( (int) value );
		}

		private AudioFile Music_Getter( int musicID )
		{
			try
			{
				if ( Globals.Instance.EnhancedAssets != null )
				{
					byte[] data = Globals.Instance.EnhancedAssets.GetMusic( musicID );
					string name = Globals.Instance.EnhancedAssets.GetMusicName( musicID );

					if ( data != null )
						return new AudioFile( musicID, data, name );
				}
				else if ( Globals.Instance.LegacyAssets != null )
				{
					byte[] data = Globals.Instance.LegacyAssets.GetMusic( musicID );
					string name = Globals.Instance.LegacyAssets.GetMusicName( musicID );

					if ( data != null )
						return new AudioFile( musicID, data, name );
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return null;
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Converts property value to music title.
	/// </summary>
	public class UltimaPacketMusicTitleConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is UltimaPacketPropertyValue == false )
				return null;

			UltimaPacketPropertyValue property = value as UltimaPacketPropertyValue;

			if ( property != null && property.Value is int )
			{
				string name = null;
				
				if ( Globals.Instance.EnhancedAssets != null )
					name = Globals.Instance.EnhancedAssets.GetMusicName( (int) property.Value );
				else if ( Globals.Instance.LegacyAssets != null )
					name = Globals.Instance.LegacyAssets.GetMusicName( (int) property.Value );

				if ( name == null )
					name = "Unknown";

				return String.Format( "ID: {0}, Name: {1}", property.Value, name );
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	/// <summary>
	/// Converts property value to sound title.
	/// </summary>
	public class UltimaPacketSoundConverter : IValueConverter
	{
		#region Properties
		private SimpleCache<int, AudioFile> _Sounds;
		#endregion

		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is int == false )
				return null;

			if ( _Sounds == null )
			{
				_Sounds = new SimpleCache<int, AudioFile>( 50 );
				_Sounds.Getter += new SimpleCacheGetter<int, AudioFile>( Sound_Getter );
			}

			return _Sounds.Get( (int) value );
		}

		private AudioFile Sound_Getter( int soundID )
		{
			try
			{
				if ( Globals.Instance.EnhancedAssets != null )
				{
					byte[] data = Globals.Instance.EnhancedAssets.GetSound( soundID );
					string name = Globals.Instance.EnhancedAssets.GetSoundName( soundID );

					if ( data != null )
						return new AudioFile( soundID, data, name );
				}
				else if ( Globals.Instance.LegacyAssets != null )
				{
					byte[] data = Globals.Instance.LegacyAssets.GetSound( soundID );
					string name = String.Format( "0x{0:X}", soundID );

					if ( data != null )
						return new AudioFile( soundID, data, name );
					
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return null;
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Converts null to bool.
	/// </summary>
	public class UltimaPacketSoundTitleConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is UltimaPacketPropertyValue == false )
				return null;

			UltimaPacketPropertyValue property = value as UltimaPacketPropertyValue;

			if ( property != null && property.Value is int )
			{
				if ( Globals.Instance.EnhancedAssets != null )
					return String.Format( "ID: {0}, Name: {1}", property.Value, Globals.Instance.EnhancedAssets.GetSoundName( (int) property.Value ) );
				else if ( Globals.Instance.LegacyAssets != null )
					return String.Format( "ID: {0}", property.Value );
			}

			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Gets texture from textureID.
	/// </summary>
	public class UltimaPacketTextureConverter : IValueConverter
	{
		#region Properties
		private SimpleCache<int, TextureFile> _EnhancedTexture;
		private SimpleCache<int, TextureFile> _LegacyTexture;
		private BitmapImage _DefaultBitmap;
		#endregion

		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is int == false )
				return null;

			if ( _DefaultBitmap == null )
			{
				_DefaultBitmap = new BitmapImage();
				_DefaultBitmap.BeginInit();
				_DefaultBitmap.UriSource = new Uri("pack://application:,,,/SpyUO;component/Images/Missing.png");
				_DefaultBitmap.EndInit();
			}

			if ( _EnhancedTexture != null )
			{
				if ( Globals.Instance.EnhancedAssets == null )
					_EnhancedTexture.Clear();
				else
					return _EnhancedTexture.Get( (int) value );
			}
			else
			{
				if ( Globals.Instance.EnhancedAssets != null )
				{
					_EnhancedTexture = new SimpleCache<int, TextureFile>( 100 );
					_EnhancedTexture.Getter += new SimpleCacheGetter<int, TextureFile>( EnhancedTexture_Getter );
					return _EnhancedTexture.Get( (int) value );
				}
			}

			if ( _LegacyTexture != null )
			{
				if ( Globals.Instance.LegacyAssets == null )
					_LegacyTexture.Clear();
				else
					return _LegacyTexture.Get( (int) value );
			}
			else
			{
				if ( Globals.Instance.LegacyAssets != null )
				{
					_LegacyTexture = new SimpleCache<int, TextureFile>( 100 );
					_LegacyTexture.Getter += new SimpleCacheGetter<int, TextureFile>( LegacyTexture_Getter );
					return _LegacyTexture.Get( (int) value );
				}
			}

			return GetDefaultTexture( (int) value );
		}

		protected virtual TextureFile EnhancedTexture_Getter( int textureID )
		{
			try
			{
				if ( Globals.Instance.EnhancedAssets != null )
				{
					byte[] data = Globals.Instance.EnhancedAssets.GetTexture( textureID );

					if ( data != null )
					{
						string name = String.Format( "0x{0:X}", textureID );

						if ( Globals.Instance.Clilocs != null )
						{
							int cliloc = UltimaItemGenerator.GetClilocFromItemID( textureID );
							string clilocText = Globals.Instance.Clilocs.GetString( cliloc );

							if ( !String.IsNullOrEmpty( clilocText ) )
								name = String.Format( "{0} - {1}", name, clilocText );
						}

						TextureFile texture = new TextureFile();
						texture.ID = textureID;
						texture.Name = name;
						texture.Data = data;
						texture.Image = DDS.FromMemory( data ).GetTextureAsBitmapSource();

						return texture;
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return GetDefaultTexture( textureID, true );
		}

		protected virtual TextureFile LegacyTexture_Getter( int textureID )
		{
			try
			{
				if ( Globals.Instance.LegacyAssets != null )
				{
					byte[] data = Globals.Instance.LegacyAssets.GetTexture( textureID );

					if ( data != null )
					{
						string name = String.Format( "0x{0:X}", textureID );

						if ( Globals.Instance.Clilocs != null )
						{
							int cliloc = UltimaItemGenerator.GetClilocFromItemID( textureID );
							string clilocText = Globals.Instance.Clilocs.GetString( cliloc );

							if ( !String.IsNullOrEmpty( clilocText ) )
								name = String.Format( "{0} - {1}", name, clilocText );
						}

						TextureFile texture = new TextureFile();
						texture.ID = textureID;
						texture.Name = name;
						texture.Image = UltimaLegacyArt.FromMemory( data, false ).GetImageAsBitmapSource();

						return texture;
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return GetDefaultTexture( textureID, true );
		}

		protected TextureFile GetDefaultTexture( int textureID, bool hexName = false )
		{
			TextureFile defaultTexture = new TextureFile();
			defaultTexture.ID = textureID;

			if ( !hexName )
				defaultTexture.Name = String.Format( "{0} - Missing texture", textureID );
			else
				defaultTexture.Name = String.Format( "0x{0:X} - Missing texture", textureID );

			defaultTexture.Image = _DefaultBitmap;

			return defaultTexture;
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Converts cliloc number to text.
	/// </summary>
	public class UltimaPacketClilocConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( Globals.Instance.Clilocs == null || value is int == false )
				return null;

			string text = Globals.Instance.Clilocs.GetString( (int) value );

			if ( text != null )
				return text;

			return "Not Found";
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Gets texture from bodyID.
	/// </summary>
	public class UltimaPacketBodyConverter : UltimaPacketTextureConverter
	{
		#region IValueConverter Members
		protected override TextureFile EnhancedTexture_Getter( int bodyID )
		{
			try
			{
				if ( Globals.Instance.EnhancedAssets != null )
				{
					byte[] data = null;
					int move = 0;

					while ( data == null && move < 99 )
						data = Globals.Instance.EnhancedAssets.GetAnimation( bodyID, move++, 6 );

					string name = Globals.Instance.EnhancedAssets.GetAnimationName( bodyID );

					if ( data != null )
					{
						UltimaAnimation animation = UltimaAnimation.FromMemory( data );

						if ( animation.Frames != null && animation.Frames.Count > 0 )
						{
							TextureFile file = new TextureFile();
							file.ID = bodyID;
							file.Image = animation.GetImageAsBitmapSource();
							file.Name = name;

							return file;
						}
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return GetDefaultTexture( bodyID );
		}

		protected override TextureFile LegacyTexture_Getter( int bodyID )
		{
			try
			{
				if ( Globals.Instance.LegacyAssets != null )
				{
					byte[] data = null;
					int move = 0;

					while ( data == null && move < 99 )
						data = Globals.Instance.LegacyAssets.GetAnimation( bodyID, move++, 6 );

					if ( data != null )
					{
						UltimaAnimation animation = UltimaAnimation.FromMemory( data, true );

						if ( animation.Frames != null && animation.Frames.Count > 0 )
						{
							TextureFile file = new TextureFile();
							file.ID = bodyID;
							file.Image = animation.GetImageAsBitmapSource();
							file.Name = String.Format( "{0}", bodyID );

							return file;
						}
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return GetDefaultTexture( bodyID );
		}
		#endregion
	}

	/// <summary>
	/// Converts null to bool.
	/// </summary>
	public class NullConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return value != null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// Packet visibility converter.
	/// </summary>
	public class VisibilityConverter : IValueConverter
	{
		#region Properties
		private bool _IsInverted;

		/// <summary>
		/// Determines whether converter is inverted.
		/// </summary>
		public bool IsInverted
		{
			get { return _IsInverted; }
			set { _IsInverted = value; }
		}
		#endregion

		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( _IsInverted )
			{
				if ( value is Visibility )
				{
					if ( (Visibility) value == Visibility.Visible )
						return true;
					else
						return false;
				}

				return Visibility.Collapsed;
			}
			else
			{
				if ( value is bool )
				{
					if ( (bool) value )
						return Visibility.Visible;
					else
						return Visibility.Collapsed;
				}

				return false;
			}
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( !_IsInverted )
			{
				if ( value is Visibility )
				{
					if ( (Visibility) value == Visibility.Visible )
						return true;
					else
						return false;
				}

				return Visibility.Collapsed;
			}
			else
			{
				if ( value is bool )
				{
					if ( (bool) value )
						return Visibility.Visible;
					else
						return Visibility.Collapsed;
				}

				return false;
			}
		}
		#endregion
	}

	/// <summary>
	/// Converts notification type to image.
	/// </summary>
	public class NotificationImageConverter : IValueConverter
	{
		#region Properties
		private Dictionary<NotificationType, BitmapImage> _Resources = new Dictionary<NotificationType, BitmapImage>();
		#endregion

		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is NotificationType )
			{
				NotificationType type = (NotificationType) value;

				if ( _Resources.ContainsKey( type ) )
					return _Resources[ type ];

				BitmapImage image = LoadImage( type );

				if ( image != null )
					_Resources.Add( type, image );

				return image;
			}

			return null;
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}

		private BitmapImage LoadImage( NotificationType type )
		{
			try
			{

				BitmapImage image = new BitmapImage();
				image.BeginInit();

				switch ( type )
				{
					case NotificationType.Info: image.UriSource = new Uri( "Images/NotificationInfo.png", UriKind.Relative ); break;
					case NotificationType.Warning: image.UriSource = new Uri( "Images/NotificationWarning.png", UriKind.Relative ); break;
					case NotificationType.Error: image.UriSource = new Uri( "Images/NotificationError.png", UriKind.Relative ); break;
				}

				image.EndInit();
				return image;
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			return null;
		}
		#endregion
	}

	/// <summary>
	/// Gets list of operations for specific type.
	/// </summary>
	public class FilterTypeOperationsConverter : IValueConverter
	{
		#region IValueConverter Members
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			UltimaPacketPropertyDefinition definition = parameter as UltimaPacketPropertyDefinition;

			if ( definition != null )
				return UltimaPacketFilterParser.GetTypeOperations( definition );

			return null;
		}

		public object ConvertBack( object value, Type targetTypes, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
