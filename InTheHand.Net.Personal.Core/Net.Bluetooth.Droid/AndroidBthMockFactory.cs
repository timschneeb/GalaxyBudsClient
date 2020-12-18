// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if FAKE_ANDROID_BTH_API && ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{

    class AndroidBthMockFactory : AndroidBthFactoryBase
    {
        public AndroidBthMockFactory()
            : this(MockAndroidBth1.OriginalValues)
        {
        }

        public AndroidBthMockFactory(AndroidMockValues values)
            : base(MockAndroidBth1.Create<BluetoothAdapter>(values))
        {
        }

        // too tricky for user to call
        private AndroidBthMockFactory(IDictionary<KeyValuePair<string, string>, object> values)
            : base(MockAndroidBth1.Create<BluetoothAdapter>(values))
        {
        }

    }
}
#endif