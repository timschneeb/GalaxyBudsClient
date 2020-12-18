// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaFakeFactory
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaFakeFactory : BluetopiaFactory
    {
        public BluetopiaFakeFactory()
            : base(new BluetopiaFakeApi())
        {
        }
    }
}
