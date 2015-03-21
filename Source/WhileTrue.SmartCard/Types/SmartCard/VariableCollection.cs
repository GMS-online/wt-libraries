using System.Collections;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Types.SmartCard
{
    /// <supportingClass/>
    /// <summary>
    /// Read-only collection of <see cref="Variable"/>s
    /// </summary>
    public class VariableCollection : ReadOnlyCollectionBase
    {
        internal VariableCollection()
        {
        }

        /// <summary>
        /// Gets the variable by its index
        /// </summary>
        public Variable this[int index]
        {
            get { return (Variable) this.InnerList[index]; }
        }

        internal void Add(Variable variable)
        {
            this.InnerList.Add(variable);
        }

        internal void Clear()
        {
            this.InnerList.Clear();
        }
    }
}