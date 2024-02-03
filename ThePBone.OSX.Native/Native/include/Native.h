//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#pragma once

#import "Bluetooth.h"
#import "BluetoothDeviceWatcher.h"

@class Bluetooth;
@class BluetoothDeviceWatcher;

typedef void (*HotkeyOnDispatch)(const uint identifier);

struct BluetoothImpl {
    Bluetooth *client;
    BluetoothDeviceWatcher *watcher;
};

struct HotkeyMgrImpl {
    NSMutableArray *hotkeys;
    HotkeyOnDispatch onDispatchHotkey;
};

void dispatchHotkey(HotkeyMgrImpl *self, NSNumber* identifier);

extern "C" {
/* Bluetooth manager */
extern bool bt_alloc(BluetoothImpl **self);
extern void bt_free(BluetoothImpl *self);

extern BT_CONN_RESULT bt_connect(BluetoothImpl *self, const char *mac, const unsigned char *uuid);
extern bool bt_disconnect(BluetoothImpl *self);
extern BT_SEND_RESULT bt_send(BluetoothImpl *self, void *data, unsigned int length);
extern BT_ENUM_RESULT bt_enumerate(BluetoothImpl *self, EnumerationResult *result);
extern bool bt_is_connected(BluetoothImpl *self);

extern void bt_set_on_channel_data(BluetoothImpl *self, Bt_OnChannelData cb);
extern void bt_set_on_channel_closed(BluetoothImpl *self, Bt_OnChannelClosed cb);

/* Bluetooth watcher */
extern bool bt_register_disconnect_notification(BluetoothImpl *self, const char *mac);
extern void bt_set_on_connected(BluetoothImpl *self, BtDev_OnConnected cb);
extern void bt_set_on_disconnected(BluetoothImpl *self, BtDev_OnDisconnected cb);

/* Bluetooth device */
extern void btdev_free(Device *self);

/* General memory management */
extern void mem_free(void *ptr);

extern void setHideInDock(bool doHide);
extern void setAutoStartEnabled(bool autoStart);
extern bool isAutoStartEnabled();

extern void allocHotkeyMgr(HotkeyMgrImpl **self, HotkeyOnDispatch cb);
extern bool registerHotKey(HotkeyMgrImpl *self, uint win32Keyflags, uint win32Modflags);
extern void deallocHotkeyMgr(HotkeyMgrImpl *self);
extern void unregisterAllHotkeys(HotkeyMgrImpl *self);
};
