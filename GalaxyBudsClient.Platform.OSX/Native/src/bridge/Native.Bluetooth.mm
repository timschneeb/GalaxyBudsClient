//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import "Native.h"

bool bt_alloc(BluetoothImpl **self) {
    // calloc (not malloc): the struct's client/watcher are ARC __strong members, so the slots must
    // start nil — assigning into uninitialised memory would make ARC release a garbage pointer.
    *self = (BluetoothImpl *)calloc(1, sizeof(struct BluetoothImpl));
    if (*self == nullptr) {
        return false;
    }

    (*self)->client = [[Bluetooth alloc] init];
    (*self)->watcher = [[BluetoothDeviceWatcher alloc] init];

    return (*self)->client != nullptr &&
           (*self)->watcher != nullptr;
}

void bt_free(BluetoothImpl *self) {
    if (self == nullptr) {
        return;
    }

    // A raw free() does not run ARC's release of the struct's __strong members, and the watcher's
    // IOBluetooth notifications retain it. Tear those down and close the channel, then nil the
    // members so ARC releases the objects, before freeing the struct.
    [self->watcher teardown];
    [self->client disconnect];
    self->client = nil;
    self->watcher = nil;

    free(self);
}

BT_CONN_RESULT bt_connect(BluetoothImpl *self, const char *mac, const unsigned char *uuid) {
    NSString *nsMac = [NSString stringWithCString:mac encoding:[NSString defaultCStringEncoding]];
    BT_CONN_RESULT res = [self->client connect:nsMac uuid:uuid];
    return res;
}

bool bt_disconnect(BluetoothImpl *self) {
    return [self->client disconnect];
}

BT_SEND_RESULT bt_send(BluetoothImpl *self, void *data, unsigned int length) {
    return [self->client sendData:static_cast<char *>(data) length:length];
}

BT_ENUM_RESULT bt_enumerate(BluetoothImpl *self, EnumerationResult *result) {
    return [self->client enumerate:result];
}

bool bt_is_connected(BluetoothImpl *self) {
    return [self->client isConnected];
}

void bt_set_on_channel_data(BluetoothImpl *self, Bt_OnChannelData cb) {
    [self->client setOnChannelData:cb];
}

void bt_set_on_channel_closed(BluetoothImpl *self, Bt_OnChannelClosed cb) {
    [self->client setOnChannelClosed:cb];
}

/* Bluetooth watcher */

bool bt_register_disconnect_notification(BluetoothImpl *self, const char *mac) {
    NSString *nsMac = [NSString stringWithCString:mac encoding:[NSString defaultCStringEncoding]];
    BOOL result = [self->watcher registerForDisconnectNotification:nsMac];
    return result;
}

void bt_set_on_connected(BluetoothImpl *self, BtDev_OnConnected cb) {
    [self->watcher setOnConnected:cb];
}

void bt_set_on_disconnected(BluetoothImpl *self, BtDev_OnDisconnected cb) {
    [self->watcher setOnDisconnected:cb];
}
