using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Ultima.Package;
using Microsoft.Win32;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for ClilocWindow.xaml
	/// </summary>
	public partial class ClilocWindow : Window
	{
		#region Properties
		/// <summary>
		/// Represents Clilocs property.
		/// </summary>
		public static readonly DependencyProperty ClilocsProperty = DependencyProperty.Register(
			"Clilocs", typeof( UltimaStringCollection ), typeof( ClilocWindow ), 
			new PropertyMetadata( null, new PropertyChangedCallback( Clilocs_Changed ) ) );

		/// <summary>
		/// Gets or sets clilocs.
		/// </summary>
		public UltimaStringCollection Clilocs
		{
			get { return GetValue( ClilocsProperty ) as UltimaStringCollection; }
			set { SetValue( ClilocsProperty, value ); }
		}

		/// <summary>
		/// Represents SearchQuery property.
		/// </summary>
		public static readonly DependencyProperty SearchQueryProperty = DependencyProperty.Register(
			"SearchQuery", typeof( string ), typeof( ClilocWindow ),
			new PropertyMetadata( null, new PropertyChangedCallback( SearchQuery_Changed ) ) );

		/// <summary>
		/// Gets or sets search query.
		/// </summary>
		public string SearchQuery
		{
			get { return GetValue( SearchQueryProperty ) as string; }
			set { SetValue( SearchQueryProperty, value ); }
		}

		private ICollectionView _View;
		private string _SearchQueryLower;
		private int _SearchQueryInteger;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of ClilocWindow.
		/// </summary>
		public ClilocWindow()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private void UpdateClilocs()
		{
			if ( _View != null )
				_View.Filter -= new Predicate<object>( Filter_Displayed );

			UltimaStringCollection clilocs = Clilocs;

			if ( clilocs == null )
			{
				_View = null;
				return;
			}

			_View = CollectionViewSource.GetDefaultView( clilocs.List );

			if ( _View != null )
			{
				_View.Filter += new Predicate<object>( Filter_Displayed );
				_View.Refresh();
			}
		}

		private void UpdateSearchQuery()
		{
			try
			{
				_SearchQueryLower = null;

				if ( !String.IsNullOrWhiteSpace( SearchQuery ) )
					_SearchQueryLower = SearchQuery.ToLowerInvariant();

				_SearchQueryInteger = 0;

				if ( !Int32.TryParse( SearchQuery, out _SearchQueryInteger ) )
					_SearchQueryInteger = 0;

				if ( _View != null )
					_View.Refresh();
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private bool Filter_Displayed( object o )
		{
			if ( _SearchQueryLower == null )
				return true;

			UltimaStringCollectionItem item = o as UltimaStringCollectionItem;

			if ( item != null )
			{
				if ( item.Text != null && item.Text.ToLowerInvariant().Contains( _SearchQueryLower ) )
					return true;
				else if ( item.Number == _SearchQueryInteger )
					return true;
				else if ( item.Number.ToString().Contains( _SearchQueryLower ) )
					return true;

				return false;
			}

			return true;
		}

		private void OpenClilocs_Click( object sender, RoutedEventArgs e )
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "All Files|*.*";
			dialog.CheckPathExists = true;
			dialog.Title = "Open Clilocs";

			if ( dialog.ShowDialog() == true )
			{
				try
				{
					Clilocs = UltimaStringCollection.FromFile( dialog.FileName );
				}
				catch ( Exception ex )
				{
					ErrorWindow.Show( ex );
				}
			}
		}

		private static void Clilocs_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ClilocWindow entry = d as ClilocWindow;

			if ( entry != null )
				entry.UpdateClilocs();
		}

		private static void SearchQuery_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ClilocWindow entry = d as ClilocWindow;

			if ( entry != null )
				entry.UpdateSearchQuery();
		}
		#endregion
	}
}
