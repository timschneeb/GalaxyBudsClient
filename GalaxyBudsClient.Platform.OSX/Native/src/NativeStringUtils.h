//
// Created by Tim Schneeberger. Licensed under GPLv3.
//

#pragma once

#include <stdlib.h>
#include <string.h>

#import <Foundation/Foundation.h>

static inline BOOL IsNullOrEmpty(NSString *value) {
    return value == nil || [value length] == 0;
}

static inline NSString *FirstNonEmptyString(NSString *first, NSString *second, NSString *third) {
    if (!IsNullOrEmpty(first)) {
        return first;
    }

    if (!IsNullOrEmpty(second)) {
        return second;
    }

    if (!IsNullOrEmpty(third)) {
        return third;
    }

    return @"";
}

static inline char *CopyNSStringToUtf8CString(NSString *value) {
    if (value == nil) {
        return NULL;
    }

    const char *utf8String = [value UTF8String];
    if (utf8String == NULL) {
        return NULL;
    }

    size_t length = strlen(utf8String);
    char *copy = (char *)malloc(length + 1);
    if (copy == NULL) {
        return NULL;
    }

    memcpy(copy, utf8String, length + 1);
    return copy;
}
