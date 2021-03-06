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
    /// A connection between an output port and an input port. 
    /// If the output port is powered the connection will pass the signal to the input port.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Checks if the connection is powered.
        /// </summary>
        /// <return>True if powered, false if not. </return>
        public virtual bool isPowered
        {
            get;
            private set;
        }

        /// <summary>
        /// Input port of the connecton.
        /// </summary>
        private Port inputPort
        {
            get;
            set;
        }
        /// <summary>
        /// Output port of the connection.
        /// </summary>
        private Port outputPort
        {
            get;
            set;
        }
    }
}