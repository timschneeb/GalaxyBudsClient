//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. Licensed under GPLv3.
//

#import <IOBluetooth/IOBluetooth.h>
#import "Bluetooth.h"

@implementation Bluetooth {
    NSString *_macAddress;
    Bt_OnChannelData _onChannelData;
    Bt_OnChannelClosed _onChannelClosed;
}

- (id)init {
    if (self = [super init]) {
        _macAddress = NULL;
    }

    return self;
}

- (BT_CONN_RESULT)connect:(NSString *)mac uuid:(const UInt8 *)uuid {
    if (mRFCOMMChannel != nil) {
        NSLog(@"Error: Already connected. Disconnecting.. **This should never happen**\n");
        [self disconnect];
        return BT_CONN_EUNKNOWN;
    }
    IOReturn status;
    int i;
    IOBluetoothDevice *device = NULL;

    bool found = [Bluetooth getDevice:mac result:&device];

    if (!found) {
        return BT_CONN_ENOTPAIRED;
    }

    IOBluetoothSDPUUID *parsedUuid = [IOBluetoothSDPUUID uuidWithBytes:uuid length:16];

    // Before we can open the RFCOMM channel, we need to open a connection to the device.
    // The openRFCOMMChannel... API probably should do this for us, but for now we have to
    // do it manually.
    // (needed for sdp too: https://github.com/NSTerminal/terminal/blob/78316cee045c5156c12606e78fa58d1e01e7e0ef/swift/Sources/btutils.swift#L76)
    if (![device isConnected]) {
        status = [device openConnection];
        
        if (status == kIOReturnTimeout) {
            return BT_CONN_ENOTFOUND;
        }
        if (status != kIOReturnSuccess) {
            NSLog(@"Error: %s opening connection to device.\n", mach_error_string(status) );
            return BT_CONN_EBASECONN;
        }
    }

    sdpQueryDone = NO;
    // sdp query with uuids specified silently fails since Ventura, ref https://developer.apple.com/forums/thread/722228
    status = [device performSDPQuery:self];

    if (status != kIOReturnSuccess) {
        NSLog(@"Error: %s starting SDP query.\n", mach_error_string(status));
        // Do not fail hard, instead get a proper error message and let it be
    } else {
        // Poll until SDP query is done, to keep code and threading simple.
        i = 0;
        while (!sdpQueryDone && i++ < 15)
            //[[NSRunLoop currentRunLoop] runUntilDate:[NSDate dateWithTimeIntervalSinceNow:0.1]];
            [NSThread sleepForTimeInterval:0.1];
        if (!sdpQueryDone)
            NSLog(@"Warning: SDP query timed out.\n");
    }

    IOBluetoothSDPServiceRecord *serviceRecord = [device getServiceRecordForUUID:parsedUuid];

    if (serviceRecord == nil) {
        NSLog(@"Error - service in selected device. ***This should never happen.***\n", NULL);
        return BT_CONN_ESDP;
    }

    // To connect we need a device to connect and an RFCOMM channel ID to open on the device:
    UInt8 rfcommChannelID;
    status = [serviceRecord getRFCOMMChannelID:&rfcommChannelID];

    // Check to make sure the service record actually had an RFCOMM channel ID
    if (status != kIOReturnSuccess) {
        NSLog(@"Error: %s getting RFCOMM channel ID from service.\n", mach_error_string(status));
        return BT_CONN_ECID;
    }

    NSLog(@"Service selected '%@' - RFCOMM Channel ID = %u\n", [serviceRecord getServiceName], rfcommChannelID);

    // Open the RFCOMM channel on the new device connection
    IOBluetoothRFCOMMChannel *tempRFCOMMChannel = mRFCOMMChannel;
    status = [device openRFCOMMChannelSync:&tempRFCOMMChannel withChannelID:rfcommChannelID delegate:self];
    mRFCOMMChannel = tempRFCOMMChannel;
    
    if (mRFCOMMChannel == nil) {
        NSLog(@"Error: %s - unable to open RFCOMM channel.\n", mach_error_string(status) );
        [self disconnect];
        return BT_CONN_EOPEN;
    }

    // Poll until RFCOMM channel is open, to keep code and threading simple.
    i = 0;
    while (![mRFCOMMChannel isOpen] && i++ < 15)
        //[[NSRunLoop currentRunLoop] runUntilDate:[NSDate dateWithTimeIntervalSinceNow:0.1]];
        [NSThread sleepForTimeInterval:0.1];

    // Ignoring the returned error if RFCOMM channel is open
    // Documentation states that if status is not success, RFCOMM channel won't be set
    // For unknown reasons, status is always kIOReturnError even if connection was successful
    // As it appears to be a macOS bug, we work it around by using openRFCOMMChannelSync
    // then relying on RFCOMM channel to open after at most 1.5s (it does not open instantly even
    // though it's the Sync API)
    if (/*( status != kIOReturnSuccess ) || (*/ ![mRFCOMMChannel isOpen] /*)*/) {
        NSLog(@"Error: %s - unable to open RFCOMM channel.\n", mach_error_string(status) );
        [self disconnect];
        return BT_CONN_EOPEN;
    } else {
        if ( status != kIOReturnSuccess ) {
            NSLog(@"Warning: got %s while trying to open RFCOMM channel, but it's open anyway\n", mach_error_string(status) );
        }
        _macAddress = [[NSString alloc] initWithString:mac];
        return BT_CONN_SUCCESS;
    }
}

- (void)sdpQueryComplete:(IOBluetoothDevice *)device status:(IOReturn)status {
    if (status != kIOReturnSuccess) {
        NSLog(@"Error: %s performing SDP query.\n", mach_error_string(status));
    }

    sdpQueryDone = YES;
}

- (BOOL)disconnect
{
    if (mRFCOMMChannel != nil) {
        // This will close the RFCOMM channel and start an inactivity timer to close the baseband connection if no
        // other channels (L2CAP or RFCOMM) are open.
        [mRFCOMMChannel setDelegate:nil];
        [mRFCOMMChannel closeChannel];
        
        mRFCOMMChannel = nil;
    }
    
    _macAddress = NULL;

    return TRUE;
}

- (BOOL)isConnected {
    return mRFCOMMChannel != nil;
}

- (BT_ENUM_RESULT)enumerate:(EnumerationResult *)result {
    NSArray *inDevices = [IOBluetoothDevice pairedDevices];

    result->length = (int)(unsigned long)[inDevices count];
    result->devices = (Device*)malloc(sizeof(Device) * result->length);
    if (!result->devices) {
        NSLog(@"Error - failed to enumerate - out of memory!");
        return BT_ENUM_EUNKNOWN;
    }

    for (int i = 0; i < result->length; i++) {
        Device *resultDevice = &result->devices[i];
        IOBluetoothDevice *device = [inDevices objectAtIndex:i];
        resultDevice->device_name = (char *)malloc([device.name lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
        resultDevice->mac_address = (char *)malloc([device.addressString lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
        strcpy(resultDevice->device_name, device.name.UTF8String);
        strcpy(resultDevice->mac_address, device.addressString.UTF8String);
        resultDevice->is_connected = device.isConnected;
        resultDevice->is_paired = device.isPaired;
        resultDevice->cod = device.classOfDevice;
    }

    return BT_ENUM_SUCCESS;
}

- (BT_SEND_RESULT)sendData:(char *)buffer length:(UInt32)length
{
    if (mRFCOMMChannel != nil) {
        if (![mRFCOMMChannel isOpen]) {
            [self disconnect];
            _onChannelClosed();
            return BT_SEND_ENULL;
        }
        UInt32 numBytesRemaining;
        IOReturn result;
        BluetoothRFCOMMMTU rfcommChannelMTU;

        numBytesRemaining = length;
        result = kIOReturnSuccess;

        // Get the RFCOMM Channel's MTU.  Each write can only contain up to the MTU size
        // number of bytes.
        rfcommChannelMTU = [mRFCOMMChannel getMTU];

        // Loop through the data until we have no more to send.
        while ( (result == kIOReturnSuccess) && (numBytesRemaining > 0) ) {
            // finds how many bytes I can send:
            UInt32 numBytesToSend = ( (numBytesRemaining > rfcommChannelMTU) ? rfcommChannelMTU : numBytesRemaining);

            // This method won't return until the buffer has been passed to the Bluetooth hardware to be sent to the remote device.
            // Alternatively, the asynchronous version of this method could be used which would queue up the buffer and return immediately.
            result = [mRFCOMMChannel writeSync:buffer length:static_cast<UInt16>(numBytesToSend)];

            // Updates the position in the buffer:
            numBytesRemaining -= numBytesToSend;
            buffer += numBytesToSend;
        }

        // We are successful only if all the data was sent:
        if ( (numBytesRemaining == 0) && (result == kIOReturnSuccess) ) {
            return BT_SEND_SUCCESS;
        } else if (result == kIOReturnSuccess) {
            return BT_SEND_EPARTIAL;
        }
    }

    return BT_SEND_EUNKNOWN;
}

- (void)setOnChannelData:(Bt_OnChannelData)callback {
    _onChannelData = callback;
}

- (void)setOnChannelClosed:(Bt_OnChannelClosed)callback {
    _onChannelClosed = callback;
}

- (NSString *)currentMac {
    return _macAddress;
}

+ (BOOL)getDevice:(NSString *)nsId result:(IOBluetoothDevice **)device {
    *device = [IOBluetoothDevice deviceWithAddressString:nsId];

    if (!*device) {
        NSLog(@"Bluetooth::getDevice(): Device not found by address: %@\n", nsId);
        return FALSE;
    }

    return TRUE;
}

- (void)rfcommChannelData:(IOBluetoothRFCOMMChannel *)rfcommChannel data:(void *)dataPointer length:(size_t)dataLength {
    if (_onChannelData) {
        _onChannelData(dataPointer, dataLength);
    }
}

- (void)rfcommChannelClosed:(IOBluetoothRFCOMMChannel *)rfcommChannel {
    [self disconnect];
    if (_onChannelClosed) {
        _onChannelClosed();
    }
}

@end
