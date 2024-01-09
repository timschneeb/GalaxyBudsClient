//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import "SystemDialogs.h"

@implementation SystemDialogs
+ (UI_BTSEL_RESULT)selectBluetoothDevice:(const UInt8**)uuids count:(UInt8)uuidCount result:(Device*)result {

    IOBluetoothDeviceSelectorController	*deviceSelector;
    NSArray								*deviceArray;
    IOBluetoothDevice					*selectedDevice;

    // The device selector will provide UI to the end user to find a remote device
    deviceSelector = [IOBluetoothDeviceSelectorController deviceSelector];

    if ( deviceSelector == nil )
    {
        NSLError( @"Error - unable to allocate IOBluetoothDeviceSelectorController.\n" );
        return UI_BTSEL_EALLOC;
    }

    // Create an IOBluetoothSDPUUID object for the chat service UUID
    for(int i = 0; i < uuidCount; i++) {
        [deviceSelector addAllowedUUID:[IOBluetoothSDPUUID uuidWithBytes:(const void*)uuids[i] length:16]];
    }

    // Run the device selector modal.  This won't return until the user has selected a device and the device has
    // been validated to contain the specified service or the user has hit the cancel button.
    if ( [deviceSelector runModal] != kIOBluetoothUISuccess )
    {
        NSLInfo(@"User has cancelled the device selection.\n" );
        return UI_BTSEL_CANCELLED;
    }

    // Get the list of devices the user has selected.
    // By default, only one device is allowed to be selected.
    deviceArray = [deviceSelector getResults];

    if ( ( deviceArray == nil ) || ( [deviceArray count] == 0 ) )
    {
        NSLError( @"Error - no selected device.  ***This should never happen.***\n" );
        return UI_BTSEL_CANCELLED;
    }

    selectedDevice = deviceArray[0];
    result->device_name = (char*)malloc([selectedDevice.name lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
    result->mac_address = (char*)malloc([selectedDevice.addressString lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
    strcpy(result->device_name, selectedDevice.name.UTF8String);
    strcpy(result->mac_address, selectedDevice.addressString.UTF8String);
    result->is_connected = selectedDevice.isConnected;
    result->is_paired = selectedDevice.isPaired;
    result->cod = selectedDevice.classOfDevice;
    return UI_BTSEL_SUCCESS;
}
@end