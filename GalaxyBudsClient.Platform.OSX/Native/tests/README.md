# macOS hotkey regression tests

Run on macOS with Xcode installed, from the repository root:

```sh
bash GalaxyBudsClient.Platform.OSX/Native/tests/run.sh
```

The runner builds the native library and tests the production keyboard-event bridge.
It intercepts event posting and permission checks: no keys are sent to other applications
and no Accessibility permissions are requested. The real Swift key conversion is used,
including the current keyboard layout.

The checks cover supported and unsupported key codes, Command+A and F5 press/release
events, and existing, denied, and newly granted event-posting permission.
A physical earbud long-press test is still needed to validate the complete app flow.
