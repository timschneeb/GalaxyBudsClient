//
// Created by Tim Schneeberger on 18.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import <Foundation/Foundation.h>

@class IOBluetoothDevice;
@class IOBluetoothUserNotification;

typedef void (*BtDev_OnConnected)(const char *mac, const char *name);
typedef void (*BtDev_OnDisconnected)(const char *mac);

@interface BluetoothDeviceWatcher : NSObject
- (id)init;
- (BOOL)registerForDisconnectNotification:(NSString *)mac;
- (void)setOnConnected:(BtDev_OnConnected)callback;
- (void)setOnDisconnected:(BtDev_OnDisconnected)callback;
- (void)onConnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device;
- (void)onDisconnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device;
@end
