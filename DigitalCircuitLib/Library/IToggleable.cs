using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCircuit.Library
{
    /// <summary>
    /// Interface to make an item toggleable
    /// </summary>
    public interface IToggleable
    {
        /// <summary>
        /// Toggles the item
        /// </summary>
        void toggle();
    }
}
