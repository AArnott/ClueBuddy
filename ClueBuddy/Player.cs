//-----------------------------------------------------------------------
// <copyright file="Player.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Linq;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// A human player in the game.  Not a suspect.
	/// </summary>
	[Serializable]
	public class Player : ICardHolder, INotifyPropertyChanged {
		/// <summary>
		/// Backing field for the <see cref="CardsHeldCount"/> property.
		/// </summary>
		private int cardsHeldCount;

		/// <summary>
		/// Backing field for the <see cref="Name"/> property.
		/// </summary>
		private string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="Player"/> class.
		/// </summary>
		/// <param name="name">The player's name.</param>
		public Player(string name) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(name));
			this.name = name;
		}

		#region INotifyPropertyChanged Events

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		/// <summary>
		/// Gets or sets the name of the human player.
		/// </summary>
		public string Name {
			get {
				return this.name;
			}

			set {
				if (this.name != value) {
					this.name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		/// <summary>
		/// Gets or sets the number of cards in the player's hand.
		/// </summary>
		public int CardsHeldCount {
			get {
				return this.cardsHeldCount;
			}

			set {
				Contract.Requires<InvalidOperationException>(this.Game == null, Strings.IllegalAfterGameIsStarted);
				if (this.cardsHeldCount != value) {
					this.cardsHeldCount = value;
					this.OnPropertyChanged("CardsHeldCount");
				}
			}
		}

		/// <summary>
		/// Gets the game this player belongs to.
		/// </summary>
		public Game Game { get; internal set; }

		/// <summary>
		/// Gets the set of possibly held cards.
		/// </summary>
		public IEnumerable<Card> PossiblyHeldCards {
			get {
				Contract.Requires<InvalidOperationException>(this.Game != null);
				return from n in this.Game.Nodes
					   where n.CardHolder == this &&
					   (!n.IsSelected.HasValue || n.IsSelected.Value)
					   select n.Card;
			}
		}

		/// <summary>
		/// Determines whether this player holds a given card.
		/// </summary>
		/// <param name="card">The card in question.</param>
		/// <returns>True if the player holds the card, false if not, or null if it is unknown.</returns>
		public bool? HasCard(Card card) {
			return this.Game.Nodes.First(n => n.CardHolder == this && n.Card == card).IsSelected;
		}

		/// <summary>
		/// Determines whether the player has at least one of a specified set of cards.
		/// </summary>
		/// <param name="cards">The cards in question.</param>
		/// <returns>True if the player holds a card, false if not, or null if it is unknown.</returns>
		public bool? HasAtLeastOneOf(IEnumerable<Card> cards) {
			Contract.Requires<ArgumentNullException>(cards != null, "cards");
			Contract.Requires<InvalidOperationException>(this.Game != null);

			var relevantNodes = this.Game.Nodes.Where(n => n.CardHolder == this && cards.Contains(n.Card)).OfType<INode>();

			// If the player is known to not have any of the cards...
			if (relevantNodes.All(n => n.IsSelected.HasValue && !n.IsSelected.Value)) {
				return false; // ...then he cannot possibly disprove the suggestion.
			}

			// If the player is known to have at least one of the cards...
			if (relevantNodes.Any(n => n.IsSelected.HasValue && n.IsSelected.Value)) {
				return true; // ...then he can disprove the suggestion.
			}

			// Maybe they must have at least one of the cards in the list,
			// but we just don't know which one yet.  Test by setting all 
			// indeterminate nodes to unselected, and testing if we have a valid state.
			using (var sim = new NodeSimulation(relevantNodes)) {
				foreach (INode node in relevantNodes.Where(n => !n.IsSelected.HasValue)) {
					node.IsSelected = false;
				}

				CompositeConstraint cc = new CompositeConstraint(this.Game.Constraints);
				if (cc.IsBroken) {
					return true; // Something's got to be selected.
				}
			}

			// Otherwise, we don't know for sure.
			return null;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return this.Name;
		}

		#region INotifyPropertyChanged Methods

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">
		/// Name of the property.
		/// </param>
		protected virtual void OnPropertyChanged(string propertyName) {
			var propertyChanged = this.PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
