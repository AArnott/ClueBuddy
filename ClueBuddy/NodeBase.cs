using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	abstract class NodeBase : INode {
		public NodeBase() {
			isSelected.Push(null); // push the actual selection status onto the stack.
		}

		Stack<bool?> isSelected = new Stack<bool?>();
		/// <summary>
		/// Whether <see cref="Player"/> actually holds the <see cref="Card"/>.
		/// </summary>
		public bool? IsSelected {
			get { return isSelected.Peek(); }
			set {
				// Careful to use the IsSelected property getter rather than the field,
				// so that simulation redirection can occur properly.
				if (value == IsSelected) return; // nothing to change
				if (IsSelected.HasValue) // if changing a known state (not so known, is it?!)
					throw new InvalidOperationException(string.Format(
							  Strings.PropertyChangeFromToError, IsSelected.Value,
							  value.HasValue ? value.Value.ToString() : "<NULL>"));
				bool? oldValue = IsSelected;
				isSelected.Pop(); // dispose of previous value
				isSelected.Push(value); // push in new value.
				OnIsSelectedChanged(oldValue);
			}
		}

		/// <summary>
		/// Fires when the <see cref="IsSelected"/> property changes.
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<bool?>> IsSelectedChanged;
		protected virtual void OnIsSelectedChanged(bool? oldValue) {
			EventHandler<ValueChangedEventArgs<bool?>> handlers = IsSelectedChanged;
			if (handlers != null)
				handlers(this, new ValueChangedEventArgs<bool?>(oldValue, IsSelected));
		}

		public bool IsSimulating {
			get { return isSelected.Count > 1; }
		}
		public void PushSimulation() {
			isSelected.Push(IsSelected);
		}
		public bool PopSimulation() {
			if (!IsSimulating)
				throw new InvalidOperationException(Strings.NotSimulating);
			isSelected.Pop();
			return isSelected.Count == 1;
		}
	}
}
