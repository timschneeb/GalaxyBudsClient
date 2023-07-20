//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import "Native.h"

void logger_set_on_event(Logger_OnEvent cb) {
    [[Logger sharedInstance] setOnEvent:cb];
}
