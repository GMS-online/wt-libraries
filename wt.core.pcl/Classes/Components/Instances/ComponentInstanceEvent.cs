using System;
using JetBrains.Annotations;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components
{
    /// <summary/>
   [PublicAPI,ExcludeFromCodeCoverage]
    public class ComponentInstanceEventArgs : EventArgs
    {
        /// <summary>
        /// Creates the event args.
        /// </summary>
        /// <param name="componentInstance">componentDescriptor that is accessible via the 
        /// <see cref="ComponentInstance"/> property</param>
        public ComponentInstanceEventArgs(ComponentInstance componentInstance)
        {
            this.ComponentInstance = componentInstance;
        }

        /// <summary>
        /// Gets the componentInstance of the event
        /// </summary>
        public ComponentInstance ComponentInstance { get; }
    }
}