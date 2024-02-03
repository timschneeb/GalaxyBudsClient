//
// Native.AppUtils.mm
// NativeInterop
//
// Created by nift4 on 02.02.2024.
//

#include "Native.h"
#include "LaunchAtLoginController.h"
#include <AppKit/AppKit.h>
#include "NativeInterop-Swift.h"

void setHideInDock(bool doHide) {
    [[NSApplication sharedApplication] setActivationPolicy:
     (doHide ? NSApplicationActivationPolicyAccessory : NSApplicationActivationPolicyRegular)];
}

void setAutoStartEnabled(bool autoStart) {
    [[LaunchAtLoginController alloc] init].launchAtLogin = autoStart;
}

bool isAutoStartEnabled() {
    return [[LaunchAtLoginController alloc] init].launchAtLogin;
}

void allocHotkeyMgr(HotkeyMgrImpl **self, HotkeyOnDispatch cb) {
    HotkeyMgrImpl* newObj = (HotkeyMgrImpl *)malloc(sizeof(struct HotkeyMgrImpl));
    if (newObj == nullptr) return;
    newObj->hotkeys = [NSMutableArray array];
    if (newObj->hotkeys == nullptr) {
        free(newObj);
        return;
    }
    newObj->onDispatchHotkey = cb;
    *self = newObj;
}

void deallocHotkeyMgr(HotkeyMgrImpl *self) {
    free(self);
}

void unregisterAllHotkeys(HotkeyMgrImpl *self) {
    [self->hotkeys removeAllObjects];
    [HotkeyManager unregisterAll];
}

bool registerHotKey(HotkeyMgrImpl *self, uint win32Keyflags, uint win32Modflags) {
    [self->hotkeys addObject:[NSNumber numberWithUnsignedLong:win32Keyflags + (win32Modflags << 8)]];
    return [HotkeyManager registerWithStr:self identifier:[self->hotkeys count] win32Keyflags:win32Keyflags win32Modflags:win32Modflags];
}

void dispatchHotkey(HotkeyMgrImpl *self, NSNumber* identifier) {
    self->onDispatchHotkey([identifier unsignedIntValue]);
}
