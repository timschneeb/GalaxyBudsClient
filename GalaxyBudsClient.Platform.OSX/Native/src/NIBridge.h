//
// NIBridge.h
// NativeInterop
//
// Created by nift4 on 03.02.2024.
//

#ifndef NIBridge_h
#define NIBridge_h

#import <Foundation/Foundation.h>

@interface NIBridge : NSObject

+ (void)dispatchHotkeyEvent:(void*)str identifier:(NSNumber*)identifier;

@end

#endif /* NIBridge_h */
