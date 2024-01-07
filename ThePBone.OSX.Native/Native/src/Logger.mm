//
// Created by Tim Schneeberger on 22.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import "Logger.h"

@implementation Logger {
    Logger_OnEvent _onEvent;
}
static Logger* _sharedInstance;

+ (Logger *)sharedInstance {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
       _sharedInstance = [[self alloc] init];
    });
    return _sharedInstance;
}

- (void)setOnEvent:(Logger_OnEvent)callback {
    _onEvent = callback;
}
- (void)log:(LOG_LEVELS)level format:(NSString *)format, ... {
    va_list args;
    va_start(args, format);
    ssize_t bufsz = snprintf(NULL, 0, [format cStringUsingEncoding:NSASCIIStringEncoding], args);
    char* buf = (char*)malloc(bufsz + 1);
    snprintf(buf, bufsz + 1, [format cStringUsingEncoding:NSASCIIStringEncoding], args);
    va_end (args);

    if(_onEvent) {
        _onEvent(level, buf);
    }
    else {
        NSLog(@"%s", buf);
    }

    free(buf);
}

@end