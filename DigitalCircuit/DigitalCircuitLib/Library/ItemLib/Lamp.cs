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
    /// Child of the item class that can be added to the circuit.
    /// </summary>
    public class Lamp : Item
    {
        public Lamp()
            : base(1, 0)
        {
        }

        public override bool getOutput()
        {
            return Inputs[0].value;
        }
    }
}