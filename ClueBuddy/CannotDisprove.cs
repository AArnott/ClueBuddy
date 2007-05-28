using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class CannotDisprove : Clue {
		public CannotDisprove(Player unablePlayer, Suspicion suspicion)
			: base(unablePlayer) {
			if (suspicion == null) throw new ArgumentNullException("suspicion");
			this.Suspicion = suspicion;
		}

		private Suspicion suspicion;

		public Suspicion Suspicion {
			get { return suspicion; }
			private set { suspicion = value; }
		}

		public override string ToString() {
			return string.Format("{0} could not disprove {1}", Player, Suspicion);
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (nodes == null) throw new ArgumentNullException("nodes");
			var constrainedNodes = nodes.Where(n => n.CardHolder == Player && Suspicion.Cards.Contains(n.Card)).OfType<INode>();
			if (constrainedNodes.Count() != Suspicion.Cards.Count())
				throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
			yield return SelectionCountConstraint.MaxSelected(0, constrainedNodes);
		}
	}
}
