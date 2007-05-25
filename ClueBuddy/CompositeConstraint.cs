using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	class CompositeConstraint : IConstraint {
		public CompositeConstraint(IEnumerable<IConstraint> constraints) {
			if (constraints == null) throw new ArgumentNullException("constraints");
			if (constraints.Count() == 0) throw new ArgumentException(Strings.ListCannotBeEmpty, "constraints");
			this.constraints = constraints;
		}

		readonly IEnumerable<IConstraint> constraints;
		public IEnumerable<IConstraint> Constraints {
			get { return constraints; }
		}

		/// <summary>
		/// Whether any contained constraints can resolve.
		/// </summary>
		public bool CanResolvePartially {
			get { return constraints.Any(c => c.CanResolve); }
		}

		/// <summary>
		/// Resolves any contained constraints that can be.
		/// </summary>
		/// <returns>
		/// Whether any resolving took place.
		/// </returns>
		/// <remarks>
		/// Whenever a constraint resolves its nodes, other constraints can potentially be affected.
		/// Because of this, this method gives all unresolved constraints a chance to resolve repeatedly
		/// until none of them can be resolved.
		/// </remarks>
		public bool ResolvePartially() {
			bool anyResolved = false;
			while (constraints.Any(c => c.CanResolve && c.Resolve())) anyResolved = true;
			return anyResolved;
		}

		/// <summary>
		/// Finds whether there is actually a solution given the current state of nodes (which may
		/// already be in simulation mode), rather than just test whether the contained constraints 
		/// can individually be satisfied, which leaves error where the constraints may be mutually exclusive.
		/// </summary>
		public bool IsSatisfiableDeep(int depth) {
			if (IsSatisfied) return true;
			if (IsBroken) return false;
			if (depth <= 0) return IsSatisfiable; // if we've run out of analysis depth, just take a shallow guess.
			IEnumerable<INode> indeterminateNodes = Nodes.Where(n => !n.IsSelected.HasValue);
			// Consider each indeterminate node's selection states, deeply.
			// When we find one that can satisfy every single constraint, return true.
			foreach (INode n in indeterminateNodes) {
				INode[] indeterminateNodesArray = indeterminateNodes.ToArray();
				if (SimulateSelection(indeterminateNodesArray, n, true, depth - 1) ||
					SimulateSelection(indeterminateNodesArray, n, false, depth - 1)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Simulates an individual node's selection state and tests whether it could lead to a solution to the game.
		/// </summary>
		/// <returns>Whether the given node and selection state leads to a valid solution.</returns>
		internal bool SimulateSelection(INode[] indeterminateNodes, INode testNode, bool testState, int depth) {
			// Future optimization: Only recurse into testing further nodes that share a constraint with the testNode.
			// Reasoning:  I'm interested in whether setting testNode can invalidate a future solution.
			//             The only constraints that testNode could possibly invalidate are the ones that contain testNode.
			//             Therefore instead of testing _every_ indeterminate node recursively after setting this node,
			//             all we need to do is test each "nearby" node and see that no constraints have been broken.
			// Discussion: But when the affected constraints are partially resolved though, the "nearby" nodes will be affected.
			//             Since those nodes could potentially belong to additional constraints which will also partially
			//             resolve and affect others in a daisy chain effect, 
			//             Will we be missing important cascade effects if we implement this optimization???
			//var constraintsContainingTestNode = new CompositeConstraint(Constraints.Where(c => c.Nodes.Contains(testNode)));
			//IEnumerable<INode> testNodeAndCousins = constraintsContainingTestNode.Nodes;

			try {
				Debug.Assert(indeterminateNodes.Contains(testNode));
				beginSimulation(indeterminateNodes);
				testNode.IsSelected = testState;
				ResolvePartially();
				INode[] indeterminateNodesNow = Nodes.Where(n => !n.IsSelected.HasValue).ToArray();
				INode[] resolvedNodes = indeterminateNodes.Where(n => !indeterminateNodesNow.Contains(n)).ToArray();
				//Debug.WriteLine("Simulated resolved nodes: " + string.Join(", ", resolvedNodes.Select(n => n.ToString()).ToArray()));
				bool result = IsSatisfiableDeep(depth);
				//Debug.WriteLine((result ? "SUCCESSFUL" : "FAILED") + " simulation " + testNode.ToString());
				return result;
			} finally {
				endSimulation(indeterminateNodes);
			}
		}


		static void beginSimulation(IEnumerable<INode> nodes) {
			foreach (var n in nodes)
				n.PushSimulation();
		}

		static void endSimulation(IEnumerable<INode> nodes) {
			foreach (var n in nodes)
				n.PopSimulation();
		}

		#region IConstraint Members

		/// <summary>
		/// Gets a list of unique nodes that are involved in the contained constraints.
		/// </summary>
		public IEnumerable<INode> Nodes {
			get {
				var uniqueNodes = new List<INode>();
				foreach (var node in from c in constraints
									 from n in c.Nodes
									 select n) {
					if (!uniqueNodes.Contains(node))
						uniqueNodes.Add(node);
				}
				return uniqueNodes;
			}
		}

		public bool IsResolved {
			get { return constraints.All(c => c.IsResolved); }
		}

		public bool CanResolve {
			get { return constraints.All(c => c.CanResolve); }
		}

		public bool Resolve() {
			// Make sure that all constraints can be resolved now before starting.
			if (CanResolve) {
				Resolve();
				return true;
			} else {
				return false;
			}
		}

		public bool IsSatisfied {
			get { return constraints.All(c => c.IsSatisfied); }
		}

		public bool IsSatisfiable {
			get { return constraints.All(c => c.IsSatisfiable); }
		}

		public bool IsBroken {
			get { return constraints.Any(c => c.IsBroken); }
		}

		public bool IsBreakable {
			get { return constraints.Any(c => c.IsBreakable); }
		}

		public bool IsMinimized {
			get { return constraints.All(c => c.IsMinimized); }
		}

		public bool IsWorthwhile {
			get { return constraints.Any(c => c.IsWorthwhile); }
		}

		public bool IsWorthless {
			get { return constraints.All(c => c.IsWorthless); }
		}

		#endregion
	}
}
