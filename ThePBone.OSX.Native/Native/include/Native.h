//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#ifndef NATIVEINTEROP_NATIVE_H
#define NATIVEINTEROP_NATIVE_H

#import "BluetoothDeviceWatcher.h"
#import "Bluetooth.h"
#import "Logger.h"

@class Bluetooth;
@class BluetoothDeviceWatcher;

struct BluetoothImpl {
    Bluetooth *client;
    BluetoothDeviceWatcher *watcher;
};

extern "C" {
    /* Bluetooth manager */
    extern bool bt_alloc(BluetoothImpl** self);
    extern void bt_free(BluetoothImpl* self);

    extern BT_CONN_RESULT bt_connect(BluetoothImpl* self, const char* mac, const unsigned char* uuid);
    extern bool bt_disconnect(BluetoothImpl* self);
    extern BT_SEND_RESULT bt_send(BluetoothImpl* self, void* data, unsigned int length);
    extern BT_ENUM_RESULT bt_enumerate(BluetoothImpl* self, EnumerationResult* result);
    extern bool bt_is_connected(BluetoothImpl* self);

    extern void bt_set_on_channel_data(BluetoothImpl* self, Bt_OnChannelData cb);
    extern void bt_set_on_channel_closed(BluetoothImpl* self, Bt_OnChannelClosed cb);

    /* Bluetooth watcher */
    extern bool bt_register_disconnect_notification(BluetoothImpl *self, const char* mac);
    extern void bt_set_on_connected(BluetoothImpl* self, BtDev_OnConnected cb);
    extern void bt_set_on_disconnected(BluetoothImpl *self, BtDev_OnDisconnected cb);

    /* Bluetooth device */
    extern void btdev_free(Device* self);

    /* System dialogs */
    extern UI_BTSEL_RESULT ui_select_bt_device(const unsigned char** uuids, unsigned char uuid_count, Device* result);

    /* Logging */
    extern void logger_set_on_event(Logger_OnEvent cb);

    /* General memory management */
    extern void mem_free(void* ptr);
};

#endif //NATIVEINTEROP_NATIVE_H
