# Galaxy Buds4 Pro support + macOS stability & Developer Tools improvements

Branch: `buds4pro-ambient-volume`

## Summary

Adds first-class support for the **Galaxy Buds4 Pro** (ambient volume control and a
custom 9-band equalizer) and, along the way, fixes several macOS-specific stability
issues and reworks the Developer Tools UI. The equalizer page was redesigned around a
single dropdown with multiple app-side custom slots, and a long-standing settings-save
bug that prevented per-device settings from persisting was fixed.

All changes are additive or macOS-scoped; no existing device behavior is removed except
one feature flag that the Buds4 Pro does not actually support (see below).

---

## Changes

### 1. Buds4 Pro device support
- New `Buds4ProDeviceSpec` feature wiring for ambient volume and custom EQ.
- Ambient sound volume control (0–N levels) with a localization-aware strength converter
  (`AmbientStrengthConverter`, `AmbientCustomizePage`).
- Custom EQ encode/decode support: `SetCustomEqualizerEncoder`,
  `CustomEqualizerDataDecoder`, and a new `Features.CustomEqualizer` flag.
- Fixed a nil-string crash hit during Buds4 Pro pairing/identification.

### 2. Equalizer redesign + multi-slot custom EQ
- Replaced the separate **EQ on/off switch + preset slider + custom toggle** with a
  single dropdown: **Off · Bass boost · Soft · Dynamic · Clear · Treble boost ·
  Custom 1 · Custom 2 · Custom 3**.
- The 9 band sliders are disabled unless a **Custom** slot is selected.
- The firmware exposes only **one** custom EQ table, so the three custom slots are stored
  **app-side, per device**. Selecting a slot pushes its curve to the single firmware table;
  editing the sliders saves back to the active slot.
- **The app's saved slots are the source of truth** for the custom curve. On connect the
  active slot is loaded into the sliders and re-pushed to the firmware (the firmware drops
  its custom table on power-cycle). The Buds4 Pro custom-table read-back is unreliable
  (returns flat), so it is intentionally ignored to avoid zeroing the sliders.
- The dropdown two-way syncs with tray hotkeys and device state via a re-entrancy guard.
- "Custom" entries only appear on devices whose firmware reports custom-EQ support.

### 3. Settings persistence fix (root cause of per-device settings not saving)
- `Settings` only subscribed devices/hotkeys **added during a session** to its
  save-on-change trigger. Items **deserialized from disk at startup** were already in the
  collection and never got subscribed, so any change to a saved device's properties never
  triggered a save (global settings/touch actions were unaffected — wired separately).
- Fix: after load, subscribe every existing device and hotkey to the save trigger.
- This is what made custom EQ (and any future per-device setting) actually persist.

### 4. macOS native Bluetooth stability
- Fixed a send/disconnect race, a disconnect-notification leak, and serialized outgoing
  sends in the native macOS Bluetooth layer
  (`BluetoothService.cs`, `Native/src/Bluetooth.mm`, `Native/src/BluetoothDeviceWatcher.mm`).

### 5. macOS app behavior
- **Crash fix:** the macOS accessibility bridge could raise an unguarded
  `NullReferenceException` in FluentAvalonia's `ItemsRepeaterAutomationPeer.GetChildrenCore()`
  while walking the automation tree (e.g. during a relayout), which propagated out of the
  dispatcher loop and killed the app. A `Dispatcher.UnhandledException` handler now marks
  **only** automation-peer exceptions as handled (logged); all other exceptions still
  crash/report as before. (Upstream's latest has the same unguarded code, so this can't be
  fixed by a FluentAvalonia bump.)
- **Window on launch:** the macOS startup path detached the main window and started
  menu-bar-only. It now shows the main window on launch (unless `/StartMinimized`) and
  restores the dock icon, while keeping the menu-bar item.

### 6. Developer Tools
- Searchable, alphabetically-sorted message-ID picker (`AutoCompleteBox`) with an explicit
  open button.
- Message-ID suggestions dropdown pinned to a fixed width (was auto-growing to the longest
  ID), with a smaller list font and tighter rows.
- Smaller `DataGrid` fonts and row heights for the message/property logs; wider default
  window (940×680).
- Request/Response selector now defaults to **Request** instead of requiring a pick.
- Fixed the 4-pane layout breaking on splitter drag.

### 7. Removed
- `Features.DoubleTapVolume` ("double-tap earbud edge") removed from `Buds4ProDeviceSpec` —
  empirically confirmed unsupported on the Buds4 Pro. `RequiresFeatureBehavior` now hides
  the toggle on this device.

---

## Files changed

| Area | Files |
|---|---|
| Buds4 Pro spec / features | `Model/Specifications/Buds4ProDeviceSpec.cs`, `Model/Specifications/Features.cs` |
| Equalizer | `Interface/Pages/EqualizerPage.axaml`, `Interface/ViewModels/Pages/EqualizerPageViewModel.cs`, `Message/Encoder/SetCustomEqualizerEncoder.cs`, `Message/Decoder/CustomEqualizerDataDecoder.cs` |
| Ambient | `Interface/Pages/AmbientCustomizePage.axaml`, `Interface/Converters/AmbientStrengthConverter.cs` |
| Settings persistence | `Model/Config/Settings.cs`, `Model/Config/SettingsData.cs` |
| macOS app | `App.axaml.cs` |
| macOS native BT | `GalaxyBudsClient.Platform.OSX/BluetoothService.cs`, `.../Native/src/Bluetooth.mm`, `.../Native/src/BluetoothDeviceWatcher.mm` |
| Developer Tools | `Interface/Developer/DevTools.axaml`, `Interface/Developer/DevToolsView.axaml`, `Interface/Developer/DevToolsView.axaml.cs` |

---

## Testing

Built and run on macOS (Apple Silicon, `net10.0-macos`, `osx-arm64`, Release) against a
real Galaxy Buds4 Pro:
- Ambient volume control works.
- Custom EQ: setting a curve, switching between Custom 1/2/3, and the curve persisting
  across app quit/relaunch — verified on disk in `settings.json` and reloaded into the
  sliders on launch.
- App no longer crashes when interacting with the EQ page.
- Window opens on launch; menu-bar item retained.
- Developer Tools dropdown sizing/fonts and default selections confirmed.

## Known issues / follow-ups

- **"Sharpen call sound" reverts to Off.** The toggle *writes* correctly
  (`EXTRA_CLEAR_SOUND_CALL`), but the read-back (`ExtraClearCallSound`) is a tail field of
  the Buds4 Pro `EXTENDED_STATUS_UPDATED` frame whose offset appears misaligned, so every
  status update resets the toggle. Needs a captured id-65 frame to correct the offset
  without disturbing adjacent fields (e.g. `CallPathControl`). Deliberately **not**
  guessed at in this PR.
- Factory-preset curves can't be shown on the (greyed) sliders: presets are applied
  firmware-side by index and their per-band values are never transmitted. Only possible as
  hardcoded cosmetic approximations.

## Notes for reviewers

- The committed build artifact `bin_osxarm64/Galaxy Buds Manager-5.2.1.0.pkg` (~92 MB)
  should likely be removed from the branch / gitignored before merge — it's a generated
  installer, not source.
