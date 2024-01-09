//
// Created by Tim Schneeberger on 16.05.21.
// Copyright (c) 2021 Tim Schneeberger. All rights reserved.
//

#import <IOBluetooth/IOBluetooth.h>
#import "Bluetooth.h"
#import "Logger.h"

@implementation Bluetooth {
    NSString* _macAddress;
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
    IOBluetoothDevice* device = NULL;
    NSLDebug(@"connect: 0");

    bool found = [Bluetooth getDevice:mac result:&device];

    NSLDebug(@"connect: 1");

    if(!found)
    {
        return BT_CONN_ENOTFOUND;
    }

    IOBluetoothSDPServiceRecord* serviceRecord = [device getServiceRecordForUUID:[IOBluetoothSDPUUID uuidWithBytes:uuid length:16]];

    NSLDebug(@"connect: 2");

    if ( serviceRecord == nil )
    {
        NSLError(@"Error - service in selected device. ***This should never happen.***\n", NULL);
        return BT_CONN_ESDP;
    }

    // To connect we need a device to connect and an RFCOMM channel ID to open on the device:
    UInt8 rfcommChannelID;
    IOReturn status = [serviceRecord getRFCOMMChannelID:&rfcommChannelID];

    NSLDebug(@"connect: 3");


    // Check to make sure the service record actually had an RFCOMM channel ID
    if ( status != kIOReturnSuccess )
    {
        NSLError(@"Error: 0x%i getting RFCOMM channel ID from service.\n", status);
        return BT_CONN_ECID;
    }

    NSLDebug(@"Service selected '%s' - RFCOMM Channel ID = %u\n", [serviceRecord getServiceName], rfcommChannelID );

    // Before we can open the RFCOMM channel, we need to open a connection to the device.
    // The openRFCOMMChannel... API probably should do this for us, but for now we have to
    // do it manually.
    status = [device openConnection];
    if ( status != kIOReturnSuccess )
    {
        NSLError( @"Error: 0x%i opening connection to device.\n", status );
        return BT_CONN_EBASECONN;
    }

    NSLDebug(@"connect: 4");

    // Open the RFCOMM channel on the new device connection
    IOBluetoothRFCOMMChannel *tempRFCOMMChannel = mRFCOMMChannel;
    status = [device openRFCOMMChannelSync:&tempRFCOMMChannel withChannelID:rfcommChannelID delegate:self];
    mRFCOMMChannel = tempRFCOMMChannel;

    NSLDebug(@"connect: 5");


    if ( ( status == kIOReturnSuccess ) && ( mRFCOMMChannel != nil ) )
    {
        _macAddress = [[NSString alloc] initWithString:mac];
        return BT_CONN_SUCCESS;
    }
    else
    {
        NSLError( @"Error: 0x%i - unable to open RFCOMM channel.\n", status );
        return BT_CONN_EOPEN;
    }
}


- (BOOL)disconnect
{
    if ( mRFCOMMChannel != nil )
    {
        IOBluetoothDevice *device = [mRFCOMMChannel getDevice];

        // This will close the RFCOMM channel and start an inactivity timer to close the baseband connection if no
        // other channels (L2CAP or RFCOMM) are open.
        [mRFCOMMChannel closeChannel];

        mRFCOMMChannel = nil;

        // This signals to the system that we are done with the baseband connection to the device.  If no other
        // channels are open, it will immediately close the baseband connection.
        [device closeConnection];
    }

    return TRUE;
}

- (BOOL)isConnected {
    if(mRFCOMMChannel == nil)
    {
        return NO;
    }

    return [mRFCOMMChannel isOpen];
}

- (BT_ENUM_RESULT)enumerate:(EnumerationResult*)result {
    NSArray *inDevices = [IOBluetoothDevice pairedDevices];
    result->length = (unsigned long)[inDevices count];
    result->devices = new Device[result->length];
    
    for (int i = 0; i < result->length; i++) {
        Device* resultDevice = &result->devices[i];
        IOBluetoothDevice *device = [inDevices objectAtIndex:i];
        NSLog(@"found %@", [device name]);
        resultDevice->device_name = (char*)malloc([device.name lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
        resultDevice->mac_address = (char*)malloc([device.addressString lengthOfBytesUsingEncoding:NSUTF8StringEncoding]);
        strcpy(resultDevice->device_name, device.name.UTF8String);
        strcpy(resultDevice->mac_address, device.addressString.UTF8String);
        resultDevice->is_connected = device.isConnected;
        resultDevice->is_paired = device.isPaired;
        resultDevice->cod = device.classOfDevice;
    }
    
    return BT_ENUM_SUCCESS;
    //return BT_ENUM_EUNKNOWN;
}

- (BT_SEND_RESULT)sendData:(char*)buffer length:(UInt32)length
{
    if (mRFCOMMChannel != nil)
    {
        UInt32				numBytesRemaining;
        IOReturn			result;
        BluetoothRFCOMMMTU	rfcommChannelMTU;

        numBytesRemaining = length;
        result = kIOReturnSuccess;

        // Get the RFCOMM Channel's MTU.  Each write can only contain up to the MTU size
        // number of bytes.
        rfcommChannelMTU = [mRFCOMMChannel getMTU];

        // Loop through the data until we have no more to send.
        while ( ( result == kIOReturnSuccess ) && ( numBytesRemaining > 0 ) )
        {
            // finds how many bytes I can send:
            UInt32 numBytesToSend = ( ( numBytesRemaining > rfcommChannelMTU ) ? rfcommChannelMTU :  numBytesRemaining );

            // This method won't return until the buffer has been passed to the Bluetooth hardware to be sent to the remote device.
            // Alternatively, the asynchronous version of this method could be used which would queue up the buffer and return immediately.
            result = [mRFCOMMChannel writeSync:buffer length:static_cast<UInt16>(numBytesToSend)];

            // Updates the position in the buffer:
            numBytesRemaining -= numBytesToSend;
            buffer += numBytesToSend;
        }

        // We are successful only if all the data was sent:
        if ( ( numBytesRemaining == 0 ) && ( result == kIOReturnSuccess ) )
        {
            return BT_SEND_SUCCESS;
        }
        else if ( result == kIOReturnSuccess )
        {
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

+ (BOOL)getDevice:(NSString*)nsId result:(IOBluetoothDevice**)device {
    *device = [IOBluetoothDevice deviceWithAddressString:nsId];
    if (!*device) {
        NSLError(@"Bluetooth::getDevice(): Device not found by address: %@\n", nsId);
        return FALSE;
    }
    return TRUE;
}

- (void)rfcommChannelData:(IOBluetoothRFCOMMChannel *)rfcommChannel data:(void *)dataPointer length:(size_t)dataLength {
    NSLog(@"onChannelData");
    if(_onChannelData)
    {
        _onChannelData(dataPointer, dataLength);
    }
}

- (void)rfcommChannelClosed:(IOBluetoothRFCOMMChannel *)rfcommChannel {
    NSLog(@"onChannelClosed");
    if(_onChannelClosed)
    {
        _onChannelClosed();
    }
}
@end

