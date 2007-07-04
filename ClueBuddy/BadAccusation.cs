using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddy {
	/// <summary>
	/// Represents an accusation made by another player that ended up being wrong.
	/// </summary>
	[Serializable]
	public class BadAccusation : Clue {
		public BadAccusation(Suspicion suggestion, CaseFile caseFile) {
			this.caseFile = caseFile;
			this.suggestion = suggestion;
		}

		CaseFile caseFile;
		Suspicion suggestion;

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			yield return new SelectionCountConstraint(0, 2, true,
				nodes.Where(n => n.CardHolder == caseFile && suggestion.Cards.Contains(n.Card))
				.OfType<INode>()
			);
		}
	}
}
