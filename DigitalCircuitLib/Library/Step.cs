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
using System.Windows.Forms;

namespace DigitalCircuit.Library
{
    /// <summary>
    /// Pairs a list of actions and controls the execution, undoing and redoing of these actions.
    /// </summary>
    public class Step
    {
        /// <summary>
        ///  A list of actions.
        /// </summary>
        private List<Action> actions;

        public Step()
        {
            this.actions = new List<Action>();
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <returns></returns>
        public bool execute()
        {
            foreach (Action action in actions)
            {
                if (!action.execute())
                {
                    return false;
                }
            }

            return true;
            
        }

        /// <summary>
        /// Adds a new action to the list.
        /// </summary>
        /// <param name="action"></param>
        public void addAction(Action action)
        {
            actions.Add(action);
        }

        /// <summary>
        /// Undoes an action.
        /// </summary>
        /// <returns></returns>
        public void undo()
        {
            foreach (Action action in actions)
            {
                action.undo();
            }
        }

    }
}