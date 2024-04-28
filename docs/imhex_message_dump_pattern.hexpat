import std.mem;

bitfield Header {
    size : 10;
    unused : 2;
    isRequest : 1;
    isFragment : 1;
    unused2 : 2;
};

struct SppMessage {
    u8 som;
    Header header;
    u8 msgId;
    u8 string[header.size - 3];
    u16 crc16;
    u8 eom;
};

SppMessage msg[while(!std::mem::eof())] @ 0x00;