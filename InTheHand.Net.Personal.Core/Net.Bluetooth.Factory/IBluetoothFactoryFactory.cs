// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Factory.IBluetoothFactoryFactory
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License


using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    /// <summary>
    /// Defines a class that provides Bluetooth Factory initialisation but returns
    /// multiple factories.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>In most cases configuration is provided so that
    /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothFactory"/> loads one or more
    /// classes each derived from <see cref="T:InTheHand.Net.Bluetooth.BluetoothFactory"/>.
    /// There the instance is the factory.  This interface allows a class to be 
    /// loaded by <see cref="T:InTheHand.Net.Bluetooth.BluetoothFactory"/> but 
    /// instead <strong>returns</strong> a list of factory instances.
    /// </para>
    /// </remarks>
    public interface IBluetoothFactoryFactory
    {
        /// <summary>
        /// Get the list of factories.
        /// </summary>
        /// <param name="errors">A list of exceptions, to which any errors in 
        /// attempting to create the factories are added.
        /// </param>
        /// <returns>A list of successfully created factories.
        /// </returns>
        IList<BluetoothFactory> GetFactories(IList<Exception> errors);
    }
}
