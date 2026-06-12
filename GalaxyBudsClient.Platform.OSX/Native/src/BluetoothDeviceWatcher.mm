//
// Created by Tim Schneeberger on 18.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
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

// addressString/nameOrAddress can be nil while the device record is still
// populating; a nil NSString yields a NULL UTF8String and strcpy(NULL) crashes.
static char *copyUTF8OrNull(NSString *str) {
    const char *utf8 = str.UTF8String;
    return utf8 ? strdup(utf8) : NULL;
}

- (void)onConnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device {
    if (_onConnected) {
        char *mac = copyUTF8OrNull(device.addressString);
        if (mac == NULL) {
            return;
        }
        char *name = copyUTF8OrNull(device.nameOrAddress);
        if (name == NULL) {
            name = strdup(mac);
        }
        _onConnected(mac, name);
    }
}

- (void)onDisconnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device {
    if (_onDisconnected) {
        char *mac = copyUTF8OrNull(device.addressString);
        if (mac == NULL) {
            return;
        }
        _onDisconnected(mac);
    }
}

@end
