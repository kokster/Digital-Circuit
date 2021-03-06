﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalCircuit.Library
{
    /// <summary>
    /// Is either an input or an output and is added to an item. 
    /// A port can be used by a connection and will be powered accordingly to this connection.
    /// </summary>
    public class Port
    {
        /// <summary>
        /// True if port is powered, false if not.
        /// </summary>
        public bool value;
        public bool Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// True if port is used, false if not.
        /// </summary>
        public virtual bool isUsed
        {
            get;
            set;
        }

    }
}