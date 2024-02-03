//
// NIBridge.m
// NativeInterop
//
// Created by nift4 on 03.02.2024.
//

#import "NIBridge.h"
#import "Native.h"

@implementation NIBridge

+ (void)dispatchHotkeyEvent:(void*)str identifier:(NSNumber*)identifier {
    dispatchHotkey(((HotkeyMgrImpl*)str), identifier);
}

@end
