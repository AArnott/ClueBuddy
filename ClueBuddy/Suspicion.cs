//-----------------------------------------------------------------------
// <copyright file="Suspicion.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Linq;

	/// <summary>
	/// The suspicion.
	/// </summary>
	[Serializable]
	public class Suspicion : INotifyPropertyChanged {
		#region Constants and Fields

		/// <summary>
		/// The place.
		/// </summary>
		private Place place;

		/// <summary>
		/// The suspect.
		/// </summary>
		private Suspect suspect;

		/// <summary>
		/// The weapon.
		/// </summary>
		private Weapon weapon;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Suspicion"/> class.
		/// </summary>
		public Suspicion() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Suspicion"/> class.
		/// </summary>
		/// <param name="suspect">The suspect.</param>
		/// <param name="weapon">The weapon.</param>
		/// <param name="place">The place.</param>
		public Suspicion(Suspect suspect, Weapon weapon, Place place) {
			Contract.Requires<ArgumentNullException>(suspect != null, "suspect");
			Contract.Requires<ArgumentNullException>(weapon != null, "weapon");
			Contract.Requires<ArgumentNullException>(place != null, "place");

			this.suspect = suspect;
			this.weapon = weapon;
			this.place = place;

			Contract.Assume(this.Cards.Count() == 3);
		}

		#endregion

		#region Events

		/// <summary>
		/// The property changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the elements of the suggestion.
		/// </summary>
		public IEnumerable<Card> Cards {
			get {
				yield return this.Suspect;
				yield return this.Weapon;
				yield return this.Place;
			}
		}

		/// <summary>
		/// Gets or sets the place.
		/// </summary>
		/// <value>The place.</value>
		public Place Place {
			get {
				return this.place;
			}

			set {
				Contract.Requires<ArgumentNullException>(value != null, "value");
				if (this.place != value) {
					this.place = value;
					this.OnPropertyChanged("Place");
				}
			}
		}

		/// <summary>
		/// Gets or sets the suspect.
		/// </summary>
		/// <value>The suspect.</value>
		public Suspect Suspect {
			get {
				return this.suspect;
			}

			set {
				Contract.Requires<ArgumentNullException>(value != null, "value");
				if (this.suspect != value) {
					this.suspect = value;
					this.OnPropertyChanged("Suspect");
				}
			}
		}

		/// <summary>
		/// Gets or sets the weapon.
		/// </summary>
		public Weapon Weapon {
			get {
				return this.weapon;
			}

			set {
				Contract.Requires<ArgumentNullException>(value != null, "value");
				if (this.weapon != value) {
					this.weapon = value;
					this.OnPropertyChanged("Weapon");
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format("[{0}, {1}, {2}]", this.Place, this.Suspect, this.Weapon);
		}

		#endregion

		#region Methods

		/// <summary>
		/// The on property changed.
		/// </summary>
		/// <param name="propertyName">
		/// The property name.
		/// </param>
		protected virtual void OnPropertyChanged(string propertyName) {
			var propertyChanged = this.PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Cards.Count() == 3);
		}
	}
}
