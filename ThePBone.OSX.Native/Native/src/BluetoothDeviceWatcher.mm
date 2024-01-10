//
// Created by Tim Schneeberger on 18.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import <IOBluetooth/IOBluetooth.h>
#import "Bluetooth.h"
#import "BluetoothDeviceWatcher.h"
#import "Native.h"


@implementation BluetoothDeviceWatcher {
    BtDev_OnConnected _onConnected;
    BtDev_OnDisconnected _onDisconnected;
}
- (id)init {
    if (self = [super init]) {
        [IOBluetoothDevice registerForConnectNotifications:self
                                                  selector:@selector(onConnected:fromDevice:)];
    }

    return self;
}

- (BOOL)registerForDisconnectNotification:(NSString *)mac {
    IOBluetoothDevice *dev;
    BOOL found = [Bluetooth getDevice:mac result:&dev];

    if (!found) {
        return FALSE;
    }

    [dev registerForDisconnectNotification:self
                                  selector:@selector(onDisconnected:fromDevice:)];
    return TRUE;
}

- (void)setOnConnected:(BtDev_OnConnected)callback {
    _onConnected = callback;
}

- (void)setOnDisconnected:(BtDev_OnDisconnected)callback {
    _onDisconnected = callback;
}

- (void)onConnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device {
    if (_onConnected) {
        char *mac = (char *)malloc(sizeof(char) * [[device addressString] length]);
        char *name = (char *)malloc(sizeof(char) * [[device nameOrAddress] length]);
        strcpy(mac, device.addressString.UTF8String);
        strcpy(name, device.nameOrAddress.UTF8String);
        _onConnected(mac, name);
    }
}

- (void)onDisconnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device {
    if (_onDisconnected) {
        char *mac = (char *)malloc(sizeof(char) * [[device addressString] length]);
        strcpy(mac, device.addressString.UTF8String);
        _onDisconnected(mac);
    }
}

@end
