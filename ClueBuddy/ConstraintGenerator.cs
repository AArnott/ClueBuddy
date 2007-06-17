using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	internal class ConstraintGenerator {
		/// <summary>
		/// Analyzes a set of constraints and generates any additional constraints that may be inferred.
		/// </summary>
		/// <remarks>
		/// The strategy is to enumerate through each indeterminate node and experiment with each of its
		/// possible selection states.  With each value, we shake out the repercussions of the selection
		/// and notice if it ends up with the constraints in a broken state.  If so, then we know the node
		/// must adopt the opposite selection state.
		/// </remarks>
		public static IEnumerable<IConstraint> AnalyzeConstraints(IEnumerable<IConstraint> constraints, int depth) {
			CompositeConstraint cc = new CompositeConstraint(constraints);
			if (cc.IsBroken)
				throw new BrokenConstraintException();

			// These lists will record which nodes are locked into a new state.
			var deducedSelectedNodes = new List<INode>();
			var deducedUnselectedNodes = new List<INode>();

			// Query for those nodes we'll be playing with this round.
			var indeterminateNodes = cc.Nodes.Where(n => !n.IsSelected.HasValue).ToArray();
			foreach (INode n in indeterminateNodes) {
				bool selectedValid = cc.SimulateSelection(indeterminateNodes, n, true, depth, false);
				if (!selectedValid) {
					deducedUnselectedNodes.Add(n);
				} 
				//else {
				//    bool unselectedValid = cc.SimulateSelection(indeterminateNodes, n, false, depth, false);
				//    if (!unselectedValid)
				//        deducedSelectedNodes.Add(n);
				//}
				//// else both are valid, and there is nothing we can deduce here.
			}

			// Build up new constraints that force the nodes to behave as needed.
			if (deducedSelectedNodes.Count > 0) {
				yield return SelectionCountConstraint.ExactSelected(deducedSelectedNodes.Count, deducedSelectedNodes);
			}
			if (deducedUnselectedNodes.Count > 0) {
				yield return SelectionCountConstraint.ExactSelected(0, deducedUnselectedNodes);
			}
		}
	}
}
