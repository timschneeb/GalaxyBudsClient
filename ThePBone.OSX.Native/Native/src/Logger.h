//
// Created by Tim Schneeberger on 22.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Enums.h"

#define NSLDebug(_format, ...) \
         [[Logger sharedInstance] log:LOG_DEBUG format:_format, ##__VA_ARGS__]
#define NSLWarn(_format, ...) \
         [[Logger sharedInstance] log:LOG_WARN format:_format, ##__VA_ARGS__]
#define NSLInfo(_format, ...) \
         [[Logger sharedInstance] log:LOG_INFO format:_format, ##__VA_ARGS__]
#define NSLError(_format, ...) \
         [[Logger sharedInstance] log:LOG_ERROR format:_format, ##__VA_ARGS__]

typedef void (*Logger_OnEvent)(LOG_LEVELS level, const char* message);

@interface Logger : NSObject
+ (Logger*) sharedInstance;

- (void) setOnEvent:(Logger_OnEvent)callback;
- (void) log:(enum LOG_LEVELS)level format:(NSString *)format, ...;

@end