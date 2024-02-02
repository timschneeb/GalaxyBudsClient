//
// Native.AppUtils.mm
// NativeInterop
//
// Created by nift4 on 02.02.2024.
//

#include "Native.h"
#include <AppKit/AppKit.h>

void setHideInDock(bool doHide) {
    [[NSApplication sharedApplication] setActivationPolicy:
     (doHide ? NSApplicationActivationPolicyAccessory : NSApplicationActivationPolicyRegular)];
}
