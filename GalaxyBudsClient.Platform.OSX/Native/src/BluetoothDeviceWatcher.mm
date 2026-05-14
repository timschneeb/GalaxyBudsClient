//
// Created by Tim Schneeberger on 18.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#include <stdlib.h>
#include <string.h>

#import <IOBluetooth/IOBluetooth.h>

#import "Bluetooth.h"
#import "BluetoothDeviceWatcher.h"
#import "Native.h"

static BOOL IsNullOrEmpty(NSString *value) {
    return value == nil || [value length] == 0;
}

static NSString *FirstNonEmptyString(NSString *first, NSString *second, NSString *third) {
    if (!IsNullOrEmpty(first)) {
        return first;
    }

    if (!IsNullOrEmpty(second)) {
        return second;
    }

    if (!IsNullOrEmpty(third)) {
        return third;
    }

    return @"";
}

static char *CopyNSStringToUtf8CString(NSString *value) {
    if (value == nil) {
        return NULL;
    }

    const char *utf8String = [value UTF8String];
    if (utf8String == NULL) {
        return NULL;
    }

    size_t length = strlen(utf8String);
    char *copy = (char *)malloc(length + 1);
    if (copy == NULL) {
        return NULL;
    }

    memcpy(copy, utf8String, length + 1);
    return copy;
}

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
        NSString *addressString = [device addressString];
        if (IsNullOrEmpty(addressString)) {
            NSLog(@"BluetoothDeviceWatcher: Ignoring connect notification without device address: %@\n", device);
            return;
        }

        char *mac = CopyNSStringToUtf8CString(addressString);
        if (mac == NULL) {
            NSLog(@"BluetoothDeviceWatcher: Failed to copy connect notification device address: %@\n", addressString);
            return;
        }

        NSString *nameString = FirstNonEmptyString([device name], [device nameOrAddress], addressString);
        char *name = CopyNSStringToUtf8CString(nameString);
        if (name == NULL) {
            NSLog(@"BluetoothDeviceWatcher: Failed to copy connect notification device name for %@\n", addressString);
            free(mac);
            return;
        }

        _onConnected(mac, name);
    }
}

- (void)onDisconnected:(IOBluetoothUserNotification *)notification fromDevice:(IOBluetoothDevice *)device {
    if (_onDisconnected) {
        NSString *addressString = [device addressString];
        if (IsNullOrEmpty(addressString)) {
            NSLog(@"BluetoothDeviceWatcher: Ignoring disconnect notification without device address: %@\n", device);
            return;
        }

        char *mac = CopyNSStringToUtf8CString(addressString);
        if (mac == NULL) {
            NSLog(@"BluetoothDeviceWatcher: Failed to copy disconnect notification device address: %@\n", addressString);
            return;
        }

        _onDisconnected(mac);
    }
}

@end
