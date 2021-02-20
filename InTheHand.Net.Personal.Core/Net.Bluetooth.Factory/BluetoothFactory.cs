// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Factory.BluetoothFactory
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Text;
using System.Diagnostics;
#if !V1
using List_BluetoothFactory = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.BluetoothFactory>;
using IList_BluetoothFactory = System.Collections.Generic.IList<InTheHand.Net.Bluetooth.Factory.BluetoothFactory>;
using List_Exception = System.Collections.Generic.List<System.Exception>;
using System.Diagnostics.CodeAnalysis;
#else
using List_BluetoothFactory = System.Collections.ArrayList;
using IList_BluetoothFactory = System.Collections.IList;
using List_Exception = System.Collections.ArrayList;
#endif

namespace InTheHand.Net.Bluetooth.Factory
{
#pragma warning disable 1591
    /// <exclude/>
    [CLSCompliant(false)] // Due to CommonRfcommStream
    public abstract class BluetoothFactory : IDisposable
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual IL2CapClient GetL2CapClient() { throw new NotImplementedException(); }
        protected abstract IBluetoothClient GetBluetoothClient();
        protected abstract IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket);
        protected abstract IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStrm);
        protected abstract IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP);
        protected abstract IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address);
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract IBluetoothListener GetBluetoothListener();
        //
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract IBluetoothRadio GetPrimaryRadio();
        protected abstract IBluetoothRadio[] GetAllRadios();
        //
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract IBluetoothSecurity GetBluetoothSecurity();
        //
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(bool disposing);

        //--------------------------------------------------------------
        #region Internal accessors to abstract methods
        public IL2CapClient DoGetL2CapClient()
        { return GetL2CapClient(); }
        public IBluetoothClient DoGetBluetoothClient()
        { return GetBluetoothClient(); }
        public IBluetoothClient DoGetBluetoothClientForListener(CommonRfcommStream acceptedStream)
        { return GetBluetoothClientForListener(acceptedStream); }
        public IBluetoothClient DoGetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        { return GetBluetoothClient(acceptedSocket); }
        public IBluetoothClient DoGetBluetoothClient(BluetoothEndPoint localEP)
        { return GetBluetoothClient(localEP); }
        public IBluetoothDeviceInfo DoGetBluetoothDeviceInfo(BluetoothAddress address)
        { return GetBluetoothDeviceInfo(address); }
        public IBluetoothListener DoGetBluetoothListener()
        { return GetBluetoothListener(); }
        //
        public IBluetoothRadio DoGetPrimaryRadio()
        { return GetPrimaryRadio(); }
        public IBluetoothRadio[] DoGetAllRadios()
        { return GetAllRadios(); }
        //
        public IBluetoothSecurity DoGetBluetoothSecurity()
        { return GetBluetoothSecurity(); }
        #endregion

        //--------------------------------------------------------------
        static IList_BluetoothFactory s_factories;
        static object lockKey = new object();

#if !V1
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
#endif
        private static void GetStacks_inLock()
        {
            List_BluetoothFactory list = new List_BluetoothFactory();
            List_Exception errors = new List_Exception();
            var stacks = new System.Collections.Generic.List<string>(
                BluetoothFactoryConfig.KnownStacks);
            TraceWriteLibraryVersion();
            foreach (string factoryName in stacks) {
                try {
                    Type t = Type.GetType(factoryName, true);
                    Debug.Assert(t != null, string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "Expected GetType to throw when type not found: '{0}'", factoryName));
                    object tmp = Activator.CreateInstance(t);
                    Debug.Assert(tmp != null, "Expect all failures to throw rather than return null.");
                    IBluetoothFactoryFactory ff = tmp as IBluetoothFactoryFactory;
                    if (ff == null) {
                        list.Add((BluetoothFactory)tmp);
                    } else { // BluetoothFactoryFactory!
                        IList_BluetoothFactory multiple = ff.GetFactories(errors);
                        if (multiple != null) {
                            Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "BluetoothFactoryFactory '{0}' supplied {1} items.",
                                ff.GetType().AssemblyQualifiedName, multiple.Count));
                            list.AddRange(multiple);
                        } else {
                            Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "BluetoothFactoryFactory '{0}' returned null.", ff.GetType().AssemblyQualifiedName));
                        }
                    }
                    if (BluetoothFactoryConfig.OneStackOnly) {
                        break;
                    }
                } catch (Exception ex) {
                    if (ex is System.Reflection.TargetInvocationException) {
                        Debug.Assert(ex.InnerException != null, "We know from the old }catch(TIEX){throw ex.InnerEx;} that this is non-null");
                        ex = ex.InnerException;
                    }
                    errors.Add(ex);
                    string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "Exception creating factory '{0}, ex: {1}", factoryName, ex);
                    if (BluetoothFactoryConfig.ReportAllErrors) {
                        Utils.MiscUtils.Trace_Fail(msg);  // (No Trace.Fail on NETCF).
                    }
                    Debug.WriteLine(msg);
                }
            }//for
            if (list.Count == 0) {
                if (!BluetoothFactoryConfig.ReportAllErrors) { // Have been reported above.
                    foreach (Exception ex in errors) {
                        string info = ExceptionExtension.ToStringNoStackTrace(ex);
                        Utils.MiscUtils.Trace_WriteLine(info);
                    }
                }
#if !NO_WIDCOMM
                // Special case Widcomm -- report if stacks seems there, but not our DLL.
                Exception wcNoInterfaceDll = null;
                try {
                    wcNoInterfaceDll = Widcomm.WidcommBtIf.IsWidcommStackPresentButNotInterfaceDll();
                } catch (Exception ex) {
                    // Maybe a P/Invoke exception calling GetModuleHandle
                    if (Environment.OSVersion.Platform.ToString().StartsWith("Win", StringComparison.Ordinal)) {
                        // Don't complain in Linux etc.
                        Utils.MiscUtils.Trace_WriteLine("Exception in IsWidcommStackPresentButNotInterfaceDll: " + ex);
                    }
                }
                if (wcNoInterfaceDll != null)
                    throw wcNoInterfaceDll;
#endif
                throw new PlatformNotSupportedException("No supported Bluetooth protocol stack found.");
            } else {
                SetFactories_inLock(list);
            }
            // result
#if !ANDROID
            /*Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Num factories: {1}, Primary Factory: {0}",
                (s_factories == null ? "(null)" : s_factories[0].GetType().Name),
                (s_factories == null ? "(null)" : s_factories.Count.ToString())));*/
#endif
        }

        internal static IList_BluetoothFactory Factories
        {
            get
            {
                lock (lockKey) {
                    if (s_factories == null) {
                        GetStacks_inLock();
                    }//if
                    Debug.Assert(s_factories.Count > 0, "Empty s_factories!");
#if !V1 // Have to suffer mutableness in NETCFv1. :-(
                    Debug.Assert(((System.Collections.IList)s_factories).IsReadOnly, "!IsReadOnly");
                    Debug.Assert(((System.Collections.IList)s_factories).IsFixedSize, "!IsFixedSize");
#endif
                    return s_factories;
                }
            }
        }

        internal static BluetoothFactory Factory
        {
            get { return (BluetoothFactory)Factories[0]; /* cast for NETCFv1 */ }
        }

        internal static void SetFactory(BluetoothFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            lock (lockKey) {
                Debug.WriteLine("SetFactory: " + factory == null ? "(null)" : factory.GetType().Name);
                if (!TestUtilities.IsUnderTestHarness()) {
                    Debug.Assert(s_factories == null, "Shouldn't change the factory.");
                    //    throw new InvalidOperationException("Can't change the factory.");
                }
                SetFactories_inLock(new List_BluetoothFactory(new BluetoothFactory[] { factory }));
            }
        }

        private static void SetFactories_inLock(List_BluetoothFactory list)
        {
            Debug.Assert(list.Count > 0, "Empty s_factories!");
#if !V1
            s_factories = list.AsReadOnly();
#else
            s_factories = list; // warning not ReadOnly
#endif
        }

        //--------------------------------------------------------------
        public static void HackShutdownAll()
        {
            lock (lockKey) {
                if (s_factories != null)
                    foreach (var cur in s_factories) {
                        ((IDisposable)cur).Dispose();
                    }
            }//lock
            s_factories = null;
        }


        /// <summary>
        /// PRE-RELEASE
        /// Get the instance of the given factory type -- if it exists.
        /// </summary>
        /// -
        /// <typeparam name="TFactory">The factory type e.g.
        /// <see cref="T:InTheHand.Net.Bluetooth.SocketsBluetoothFactory"/>
        /// or <see cref="T:InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase"/>
        /// etc.
        /// </typeparam>
        /// -
        /// <returns>The instance of the given type or <c>null</c>.
        /// </returns>
        public static TFactory GetTheFactoryOfTypeOrDefault<TFactory>()
            where TFactory : BluetoothFactory
        {
            foreach (BluetoothFactory curF in BluetoothFactory.Factories) {
                var asF = curF as TFactory;
                if (asF != null) {
                    return (TFactory)curF;
                }
            }//for
            return null;
        }

        /// <summary>
        /// PRE-RELEASE
        /// Get the instance of the given factory type -- if it exists.
        /// </summary>
        /// -
        /// <param name="factoryType">The factory type e.g.
        /// <see cref="T:InTheHand.Net.Bluetooth.SocketsBluetoothFactory"/>
        /// or <see cref="T:InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase"/>
        /// etc.
        /// </param>
        /// -
        /// <returns>The instance of the given type or <c>null</c>.
        /// </returns>
        public static BluetoothFactory GetTheFactoryOfTypeOrDefault(Type factoryType)
        {
            foreach (BluetoothFactory curF in BluetoothFactory.Factories) {
                if (factoryType.IsInstanceOfType(curF)) {
                    return curF;
                }
            }//for
            return null;
        }

        //----
        private static void TraceWriteLibraryVersion()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            var fn = a.FullName;
            //var an = new System.Reflection.AssemblyName(fn);
            var an = a.GetName();
            var v = an.Version; // Is AssemblyVersionAttribute.
            //
            var aiva = GetCustomAttributes<System.Reflection.AssemblyInformationalVersionAttribute>(a);
            string vi = (aiva != null) ? aiva.InformationalVersion : null;
            //
            Utils.MiscUtils.Trace_WriteLine("32feet.NET: '{0}'\r\n   versions: '{1}' and '{2}'.",
                an, v, vi);
        }

        static TAttr GetCustomAttributes<TAttr>(System.Reflection.Assembly a)
            where TAttr : Attribute
        {
            var arr = a.GetCustomAttributes(typeof(TAttr), true);
#if false // _Not_ supported on NETCF
            var newArr = Array.ConvertAll(arr, x => (TAttr)x);
#endif
            if (arr.Length == 0) return null;
            if (arr.Length > 1) {
                throw new InvalidOperationException("Don't support multiple attribute instances.");
            }
            var attr = (TAttr)arr[0];
            return attr;
        }

    }
}
