using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	class ValueChangedEventArgs<T> : EventArgs {
		public ValueChangedEventArgs(T oldValue, T newValue) {
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}

		public readonly T OldValue;
		public readonly T NewValue;
	}
}
