using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ClueBuddy {
	/// <summary>
	/// A human player in the game.  Not a suspect.
	/// </summary>
	[Serializable]
	public class Player : ICardHolder, INotifyPropertyChanged {
		public Player(string name) {
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this.name = name;
		}

		private string name;
		/// <summary>
		/// The name of the human player.
		/// </summary>
		public string Name {
			get { return name; }
			set {
				if (name == value) return;
				name = value;
				OnPropertyChanged("Name");
			}
		}

		public override string ToString() {
			return Name;
		}

		private int cardsHeldCount;
		/// <summary>
		/// The number of cards in the player's hand.
		/// </summary>
		public int CardsHeldCount {
			get { return cardsHeldCount; }
			set {
				if (Game != null) throw new InvalidOperationException(Strings.IllegalAfterGameIsStarted);
				if (cardsHeldCount == value) return;
				cardsHeldCount = value;
				OnPropertyChanged("CardsHeldCount");
			}
		}

		private Game game;
		/// <summary>
		/// The game this player belongs to.
		/// </summary>
		public Game Game {
			get { return game; }
			internal set { game = value; }
		}

		public bool? HasCard(Card card) {
			return Game.Nodes.First(n => n.CardHolder == this && n.Card == card).IsSelected;
		}

		public bool? HasAtLeastOneOf(IEnumerable<Card> cards) {
			IEnumerable<INode> relevantNodes = Game.Nodes.Where(n => n.CardHolder == this
				&& cards.Contains(n.Card)).OfType<INode>();
			// If the player is known to not have any of the cards...
			if (relevantNodes.All(n => n.IsSelected.HasValue && !n.IsSelected.Value))
				return false; // ...then he cannot possibly disprove the suggestion.
			// If the player is known to have at least one of the cards...
			if (relevantNodes.Any(n => n.IsSelected.HasValue && n.IsSelected.Value))
				return true; // ...then he can disprove the suggestion.
			// Maybe they must have at least one of the cards in the list,
			// but we just don't know which one yet.  Test by setting all 
			// indeterminate nodes to unselected, and testing if we have a valid state.
			using (NodeSimulation sim = new NodeSimulation(relevantNodes)) {
				foreach (INode node in relevantNodes.Where(n => !n.IsSelected.HasValue)) {
					node.IsSelected = false;
				}
				CompositeConstraint cc = new CompositeConstraint(game.Constraints);
				if (cc.IsBroken) return true; // Something's got to be selected.
			}
			// Otherwise, we don't know for sure.
			return null;
		}

		public IEnumerable<Card> PossiblyHeldCards {
			get {
				return from n in Game.Nodes
					   where n.CardHolder == this &&
					   (!n.IsSelected.HasValue || n.IsSelected.Value)
					   select n.Card;
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
