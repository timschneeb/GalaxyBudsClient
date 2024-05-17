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

void sendMagicMediaCmd(bool play) {
    return [HotkeyManager sendMagicMediaCmdWithPlay:play];
}

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

void allocHotkeySim(HotkeySimImpl **self) {
    HotkeySimImpl* newObj = (HotkeySimImpl *)malloc(sizeof(struct HotkeySimImpl));
    if (newObj == nullptr) return;
    newObj->src = CGEventSourceCreate(kCGEventSourceStateHIDSystemState);
    if (newObj->src == nullptr) {
        free(newObj);
        return;
    }
    *self = newObj;
}

void deallocHotkeySim(HotkeySimImpl *self) {
    CFRelease(self->src);
    free(self);
}

void simulateHotKey(HotkeySimImpl *self, uint keyCode, bool down, bool maskCmd, bool maskOpt, bool maskCtrl, bool maskShft) {
    if ([HotkeyManager submitMediaKeyIfPossibleWithKey:keyCode down:down]) return;
    NSInteger newKc = [HotkeyManager convertWin32KeysToCarbonWithKey:keyCode];
    if (newKc == 0) {
        NSLog(@"Invalid keyCode %u converted to 0", keyCode);
        return;
    }
    CGEventRef event = CGEventCreateKeyboardEvent(self->src, newKc, down);
    CGEventFlags flags = 0;
    if (maskCmd) flags |= kCGEventFlagMaskCommand;
    if (maskOpt) flags |= kCGEventFlagMaskAlternate;
    if (maskCtrl) flags |= kCGEventFlagMaskControl;
    if (maskShft) flags |= kCGEventFlagMaskShift;
    if (flags != 0) CGEventSetFlags(event, flags);
    CGEventTapLocation loc = kCGHIDEventTap; // kCGSessionEventTap also works
    CGEventPost(loc, event);
    CFRelease(event);
}
