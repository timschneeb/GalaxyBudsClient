#import <AppKit/AppKit.h>
#import <Carbon/Carbon.h>
#import "Native.h"
#import "NativeInterop-Swift.h"
#include <cstdio>

static bool hasAccess = true;
static bool grantAccess = false;
static int accessRequests = 0;
static int postedEvents = 0;
static int failures = 0;
static CGEventType postedType;
static int64_t postedKey;
static CGEventFlags postedFlags;

static bool TestPreflightAccess() { return hasAccess; }
static bool TestRequestAccess() { ++accessRequests; return grantAccess; }
static void TestPostEvent(CGEventTapLocation, CGEventRef event) {
    ++postedEvents;
    postedType = CGEventGetType(event);
    postedKey = CGEventGetIntegerValueField(event, kCGKeyboardEventKeycode);
    postedFlags = CGEventGetFlags(event);
}

// Exercise the production bridge without posting keys or requesting OS permissions.
#define CGEventPost TestPostEvent
#define CGPreflightPostEventAccess TestPreflightAccess
#define CGRequestPostEventAccess TestRequestAccess
#include "../src/bridge/Native.AppUtils.mm"
#undef CGEventPost
#undef CGPreflightPostEventAccess
#undef CGRequestPostEventAccess

static void Check(bool condition, const char *name) {
    std::printf("%s %s\n", condition ? "PASS" : "FAIL", name);
    if (!condition) ++failures;
}
static void Reset(bool allowed = true, bool granted = false) {
    hasAccess = allowed;
    grantAccess = granted;
    accessRequests = postedEvents = 0;
}
int main() {
    @autoreleasepool {
        HotkeySimImpl *sim = nullptr;
        allocHotkeySim(&sim);
        if (!sim) { std::puts("Cannot create event source"); return 1; }
        auto a = [HotkeyManager convertWin32KeysToCarbonWithKey:0x41];
        Check(a >= 0, "A converts to a valid key code");
        Check([HotkeyManager convertWin32KeysToCarbonWithKey:0xFFFF] < 0,
              "Unsupported keys have a distinct invalid code");
        Check([HotkeyManager convertWin32KeysToCarbonWithKey:0] < 0,
              "Unmapped enum values have a distinct invalid code");
        Check([HotkeyManager convertWin32KeysToCarbonWithKey:0x74] == kVK_F5,
              "F5 converts to kVK_F5");
        Reset();
        simulateHotKey(sim, 0x41, true, true, false, false, false);
        Check(postedEvents == 1 && postedKey == a && postedType == kCGEventKeyDown &&
              (postedFlags & kCGEventFlagMaskCommand), "Command+A key down is posted");
        simulateHotKey(sim, 0x41, false, true, false, false, false);
        Check(postedEvents == 2 && postedKey == a && postedType == kCGEventKeyUp,
              "Command+A key up is posted");
        Reset();
        simulateHotKey(sim, 0x74, true, false, false, false, false);
        Check(postedEvents == 1 && postedKey == kVK_F5 && postedType == kCGEventKeyDown,
              "F5 key down is posted");
        simulateHotKey(sim, 0x74, false, false, false, false, false);
        Check(postedEvents == 2 && postedType == kCGEventKeyUp, "F5 key up is posted");
        Check(accessRequests == 0, "Existing permission does not prompt again");
        Reset();
        simulateHotKey(sim, 0xFFFF, true, false, false, false, false);
        Check(postedEvents == 0 && accessRequests == 0, "Unsupported keys are ignored");
        Reset(false, false);
        simulateHotKey(sim, 0x74, true, false, false, false, false);
        Check(accessRequests == 1 && postedEvents == 0,
              "Denied permission requests access without posting F5");
        Reset(false, true);
        simulateHotKey(sim, 0x74, true, false, false, false, false);
        Check(accessRequests == 1 && postedEvents == 1 && postedKey == kVK_F5,
              "Granted permission allows F5");
        deallocHotkeySim(sim);
        std::printf("%d failure(s)\n", failures);
        return failures ? 1 : 0;
    }
}
