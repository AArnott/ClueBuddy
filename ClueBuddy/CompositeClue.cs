using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class CompositeClue : Clue {
		Suspicion suspicion = new Suspicion();
		/// <summary>
		/// Which three cards are trying to be disproven.
		/// </summary>
		public Suspicion Suspicion {
			get { return suspicion; }
			set {
				if (suspicion == value) return;
				suspicion = value;
				OnPropertyChanged("Suspicion");
			}
		}

		Dictionary<Player, bool> disprovingPlayers = new Dictionary<Player, bool>();
		/// <summary>
		/// Any players that either proved or disproved a suggestion,
		/// and a bool indicating whether they disproved it.
		/// If a player did not respond either way to <see cref="Suspicion"/>,
		/// that player should not show up in this dictionary.
		/// </summary>
		public Dictionary<Player, bool> DisprovingPlayers {
			get { return disprovingPlayers; }
		}

		IEnumerable<Clue> generateClues() {
			foreach (KeyValuePair<Player, bool> pair in DisprovingPlayers)
				yield return pair.Value ? 
					(Clue)new Disproved(pair.Key, Suspicion) : 
					(Clue)new CannotDisprove(pair.Key, Suspicion);
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			return from clue in generateClues()
				   from constraint in clue.GetConstraints(nodes)
				   select constraint;
		}
	}
}
