//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import "Native.h"

void btdev_free(Device *self) {
    if (self->mac_address) {
        free(self->mac_address);
    }

    if (self->device_name) {
        free(self->device_name);
    }
}

void mem_free(void *ptr) {
    if (ptr) {
        free(ptr);
    }
}
