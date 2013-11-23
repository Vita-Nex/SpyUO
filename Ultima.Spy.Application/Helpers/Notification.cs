using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes notification type.
	/// </summary>
	public enum NotificationType
	{
		Info,
		Warning,
		Error,
	}

	/// <summary>
	/// Describes notification.
	/// </summary>
	public class Notification
	{
		#region Properties
		private NotificationType _Type;

		/// <summary>
		/// Gets notification type.
		/// </summary>
		public NotificationType Type
		{
			get { return _Type; }
		}

		private string _Title;

		/// <summary>
		/// Gets notification title.
		/// </summary>
		public string Title
		{
			get { return _Title; }
		}

		private string _Message;

		/// <summary>
		/// Gets notification message.
		/// </summary>
		public string Message
		{
			get { return _Message; }
		}

		private DateTime _DateTime;

		/// <summary>
		/// Gets notification date and time.
		/// </summary>
		public DateTime DateTime
		{
			get { return _DateTime; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of Notification.
		/// </summary>
		/// <param name="type">Notification type.</param>
		/// <param name="ex">Exception.</param>
		public Notification( NotificationType type, Exception ex ) : this( type, ex.Message, ex.StackTrace )
		{
		}

		/// <summary>
		/// Constructs a new instance of Notification.
		/// </summary>
		/// <param name="type">Notification type.</param>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		public Notification( NotificationType type, string title, string message )
		{
			_Type = type;
			_Title = title;
			_Message = message;
			_DateTime = DateTime.Now;
		}
		#endregion
	}
}
