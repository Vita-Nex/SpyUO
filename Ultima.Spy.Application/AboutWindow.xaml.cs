using System;
using System.Reflection;
using System.Windows;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of AboutWindow.
		/// </summary>
		public AboutWindow()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		#region Event Handlers
		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;

			Version.Text = String.Format( "Version: {0}.{1}.{2}", version.Major, version.MajorRevision, version.Minor );
		}
		#endregion
		#endregion
	}
}
