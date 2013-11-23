using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
		#region Properties
		/// <summary>
		/// Gets main window reference.
		/// </summary>
		public static MainWindow Window
		{
			get { return Current.MainWindow as MainWindow; }
		}
		#endregion
	}
}
