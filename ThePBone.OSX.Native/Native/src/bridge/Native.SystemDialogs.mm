//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import "Native.h"
#import "SystemDialogs.h"

UI_BTSEL_RESULT ui_select_bt_device(const unsigned char** uuids, unsigned char uuid_count, Device* result) {
    return [SystemDialogs selectBluetoothDevice:uuids count:uuid_count result:result];
}
