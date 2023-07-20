//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Native.h"

#import <IOBluetoothUI/IOBluetoothUI.h>
#import <IOBluetooth/IOBluetooth.h>

@interface SystemDialogs : NSObject
+ (UI_BTSEL_RESULT)selectBluetoothDevice:(const UInt8**)uuids count:(UInt8)uuidCount result:(Device*)result;
@end