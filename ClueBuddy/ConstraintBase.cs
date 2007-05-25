using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	abstract class ConstraintBase : IConstraint {
		protected ConstraintBase(IEnumerable<INode> nodes) {
			if (nodes == null) throw new ArgumentNullException("nodes");
			if (nodes.Count() == 0)
				throw new ArgumentException(Strings.ListCannotBeEmpty, "nodes");
			this.nodes = nodes;
		}

		private IEnumerable<INode> nodes;
		/// <summary>
		/// The nodes involved in the constraint.
		/// </summary>
		public IEnumerable<INode> Nodes {
			get { return nodes; }
		}

		/// <summary>
		/// Calculates how many possible ending combinations of the selection
		/// of nodes that yet exist, given the already known nodes.
		/// </summary>
		public abstract int PossibleSolutions { get; }

		#region IConstraint Members
		public bool IsResolved {
			get { return Nodes.All(n => n.IsSelected.HasValue); }
		}
		public abstract bool CanResolve { get; }
		public abstract bool Resolve();
		public abstract bool IsSatisfied { get; }
		public abstract bool IsSatisfiable { get; }
		public virtual bool IsBroken {
			get { return !IsSatisfiable; }
		}
		public abstract bool IsBreakable { get; }
		public abstract bool IsMinimized { get; }
		public abstract bool IsWorthwhile { get; }
		public abstract bool IsWorthless { get; }
		#endregion

		protected void ThrowIfBroken() {
			if (IsBroken) throw new BrokenConstraintException();
		}
	}
}
