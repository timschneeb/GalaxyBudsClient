// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BluetoothFactorySection
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if WinXP
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{
    sealed class BluetoothFactorySection : ConfigurationSection
    {
        internal const string GlobalSectionGroupName = "InTheHand.Net.Personal/";
        internal const string SectionName = "BluetoothFactory";

        //--------
        static BluetoothFactorySection s_instance;

        internal static BluetoothFactorySection GetInstance()
        {
            if (s_instance == null) {
                object s0 = ConfigurationManager.GetSection(GlobalSectionGroupName + SectionName);
                s_instance = (BluetoothFactorySection)s0;
                //Console.WriteLine(s_instance);
                if (s_instance == null) {
                    s_instance = new BluetoothFactorySection();
                }
            }
            return s_instance;
        }

        private BluetoothFactorySection()
        {
        }

        //--------
        public override string ToString()
        {
            BluetoothStackConfigElement[] arr = new BluetoothStackConfigElement[StackList.Count];
            StackList.CopyTo(arr, 0);
            string slConcat = "{" + string.Join(", ", Array.ConvertAll<BluetoothStackConfigElement, string>(arr,
                delegate(BluetoothStackConfigElement inp) { return (inp == null) ? "(null)" : inp.Name; })) + "}";
            //
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "[BluetoothFactorySection: OneStackOnly: {0}, ReportAllErrors: {1}, StackList: {2}, FooBar1: {3}]",
                this.OneStackOnly, this.ReportAllErrors, slConcat, this.FooBar1);
        }

        //--------
        [ConfigurationProperty("reportAllErrors", DefaultValue = BluetoothFactoryConfig.Default_reportAllErrors)]
        [ApplicationScopedSettingAttribute]
        public bool ReportAllErrors
        {
            get { return (bool)this["reportAllErrors"]; }
        }

        [ConfigurationProperty("oneStackOnly", DefaultValue = BluetoothFactoryConfig.Default_oneStackOnly)]
        [ApplicationScopedSettingAttribute]
        public bool OneStackOnly
        {
            get { return (bool)this["oneStackOnly"]; }
        }

        [ConfigurationProperty("firstStack", DefaultValue = "defaultValue2")]
        [ApplicationScopedSetting]
        public string FooBar1
        {
            get { return (string)this["firstStack"]; }
        }

        [ConfigurationProperty("widcommICheckIgnorePlatform", DefaultValue = false)]
        [ApplicationScopedSettingAttribute]
        public bool WidcommICheckIgnorePlatform
        {
            get { return (bool)this["widcommICheckIgnorePlatform"]; }
        }

        [ConfigurationProperty("stackList")]
        [ApplicationScopedSetting]
        public BluetoothStackCollection StackList
        {
            get { return (BluetoothStackCollection)this["stackList"]; }
        }

        public string[] StackList2
        {
            get
            {
                List<string> ls = new List<string>();
                foreach (BluetoothStackConfigElement cur in StackList) {
                    ls.Add(cur.Name);
                }//for
                return ls.ToArray();
            }
        }
    }


    sealed class BluetoothStackConfigElement : ConfigurationElement
    {
        internal BluetoothStackConfigElement()
        {
        }

        internal BluetoothStackConfigElement(string name)
        {
            this["name"] = name;
        }

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        [ApplicationScopedSetting]
        public string Name
        {
            get
            {
                string name = (string)this["name"];
                if (name.ToUpperInvariant() == "WIDCOMM")
                    name = BluetoothFactoryConfig.WidcommFactoryType.FullName;
                else if (name.ToUpperInvariant() == "MSFT")
                    name = BluetoothFactoryConfig.MsftFactoryType.FullName;
                return name;
            }
        }
    }


    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Is used by config.")]
    [ConfigurationCollection(typeof(BluetoothStackConfigElement))]
    sealed class BluetoothStackCollection : ConfigurationElementCollection
    {
        private BluetoothStackCollection()
        {
            // Is the right place to do this??
            // Add the default set.
            foreach (string cur in BluetoothFactoryConfig.s_knownStacks) {
                BaseAdd(new BluetoothStackConfigElement(cur));
            }
        }

        //--
        protected override ConfigurationElement CreateNewElement()
        {
            return new BluetoothStackConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            BluetoothStackConfigElement e2 = (BluetoothStackConfigElement)element;
            return e2.Name;
        }
    }

}
#endif
