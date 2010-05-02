//-----------------------------------------------------------------------
// <copyright file="SuggestionResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy
{
	using System;
	using System.ComponentModel;

	/// <summary>
	/// The suggestion response.
	/// </summary>
	[Serializable]
	public class SuggestionResponse : INotifyPropertyChanged {
		#region Constants and Fields

		/// <summary>
		/// The alabi.
		/// </summary>
		private Card alabi;

		/// <summary>
		/// The disproved.
		/// </summary>
		bool? disproved;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the alabi.
		/// </summary>
		/// <value>The alabi.</value>
		public Card Alabi {
			get {
				return this.alabi;
			}

			set {
				if (this.alabi != value) {
					this.alabi = value;
					this.OnPropertyChanged("Alabi");
				}
			}
		}

		/// <summary>
		/// Gets or sets Disproved.
		/// </summary>
		public bool? Disproved {
			get { return this.disproved; }
			set {
				if (this.disproved != value) {
					this.disproved = value;
					this.OnPropertyChanged("Disproved");
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">
		/// Name of the property.
		/// </param>
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}