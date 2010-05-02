//-----------------------------------------------------------------------
// <copyright file="Clue.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// Some clue observed as part of game play.
	/// </summary>
	[Serializable]
	[ContractClass(typeof(ClueContract))]
	public abstract class Clue : INotifyPropertyChanged {
		/// <summary>
		/// Backing field for the <see cref="Player"/> property.
		/// </summary>
		private Player player;

		/// <summary>
		/// Initializes a new instance of the <see cref="Clue"/> class.
		/// </summary>
		protected Clue() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Clue"/> class.
		/// </summary>
		/// <param name="player">The player.</param>
		protected Clue(Player player) {
			Contract.Requires<ArgumentNullException>(player != null, "player");
			this.player = player;
		}

		/// <summary>
		/// Occurs when the set of constraints have changed.
		/// </summary>
		public event EventHandler ConstraintsChanged;

		/// <summary>
		/// Gets or sets the player the clue is regarding.
		/// </summary>
		/// <value>
		/// Either the player making the suggestion or the player whose card was seen.
		/// </value>
		public virtual Player Player {
			get {
				return this.player;
			}

			set {
				if (this.player != value) {
					this.player = value;
					this.OnPropertyChanged("Player");
				}
			}
		}

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>A sequence of constraints that the clue creates.</returns>
		internal abstract IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes);

		/// <summary>
		/// Fires the <see cref="ConstraintsChanged"/> event.
		/// </summary>
		protected virtual void OnConstraintsChanged() {
			var constraintsChanged = this.ConstraintsChanged;
			if (constraintsChanged != null) {
				constraintsChanged(this, null);
			}
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">
		/// Name of the property.
		/// </param>
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
