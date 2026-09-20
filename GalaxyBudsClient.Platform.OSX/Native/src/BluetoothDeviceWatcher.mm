//
// Created by Tim Schneeberger on 18.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import <IOBluetooth/IOBluetooth.h>

#import "Bluetooth.h"
#import "BluetoothDeviceWatcher.h"
#import "Native.h"
#import "NativeStringUtils.h"

@implementation BluetoothDeviceWatcher {
    BtDev_OnConnected _onConnected;
    BtDev_OnDisconnected _onDisconnected;
    IOBluetoothUserNotification *mConnectNotification;
    IOBluetoothUserNotification *mDisconnectNotification;
}
- (id)init {
    if (self = [super init]) {
        mConnectNotification = [IOBluetoothDevice registerForConnectNotifications:self
                                                  selector:@selector(onConnected:fromDevice:)];
    }

    return self;
}

// IOBluetooth user-notifications retain their target (self), so they must be
// explicitly unregistered before the watcher can be released — relying on dealloc
// would deadlock, since the notification keeps self alive. Called from bt_free.
- (void)teardown {
    if (mConnectNotification != nil) {
        [mConnectNotification unregister];
        mConnectNotification = nil;
    }
    if (mDisconnectNotification != nil) {
        [mDisconnectNotification unregister];
        mDisconnectNotification = nil;
    }
}

- (BOOL)registerForDisconnectNotification:(NSString *)mac {
    IOBluetoothDevice *dev;
    BOOL found = [Bluetooth getDevice:mac result:&dev];

    if (!found) {
        return FALSE;
    }

    // Re-registering without unregistering stacks notifications: one physical
    // disconnect would then fire the managed callback once per stale handle.
    if (mDisconnectNotification != nil) {
        [mDisconnectNotification unregister];
    }
    mDisconnectNotification = [dev registerForDisconnectNotification:self
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
