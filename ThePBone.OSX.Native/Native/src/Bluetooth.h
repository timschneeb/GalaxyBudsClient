//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Enums.h"

@class IOBluetoothDevice;
@class IOBluetoothRFCOMMChannel;

typedef void (*Bt_OnChannelData)(void* data, unsigned long size);
typedef void (*Bt_OnChannelClosed)();

@interface Bluetooth<IOBluetoothRFCOMMChannelDelegate> : NSObject
{
    __strong IOBluetoothRFCOMMChannel	*mRFCOMMChannel;
}

- (id)              init;
- (BT_CONN_RESULT)  connect:(NSString*)mac uuid:(const UInt8*)uuid;
- (BOOL)            disconnect;
- (BOOL)            isConnected;
- (BT_SEND_RESULT)  sendData:(char *)buffer length:(UInt32)length;

- (void)            setOnChannelData:(Bt_OnChannelData)callback;
- (void)            setOnChannelClosed:(Bt_OnChannelClosed)callback;

- (NSString*)       currentMac;

+ (BOOL)            getDevice:(NSString*)nsId result:(IOBluetoothDevice**)device;

// Implementation of delegate calls (see IOBluetoothRFCOMMChannel.h)
- (void)            rfcommChannelData:(IOBluetoothRFCOMMChannel *)rfcommChannel data:(void *)dataPointer length:(size_t)dataLength;
- (void)            rfcommChannelClosed:(IOBluetoothRFCOMMChannel *)rfcommChannel;

@end
