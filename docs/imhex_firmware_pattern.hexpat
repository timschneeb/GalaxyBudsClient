#pragma author timschneeb
#pragma description Samsung Galaxy Buds firmware archive

struct Segment {
    u32 unknownHeader1;
    u32 unknownHeader2;
    u32 unknownHeader3;
    u32 romOffsetMaybe;
};

struct SegmentInfo {
    u32 id;
    u32 crc32;
    Segment* position : u32;
    u32 size;
} [[single_color]];

struct Header {
    u32 magic;
    u32 totalSize;
    u32 segmentsCount;
    SegmentInfo segments[segmentsCount]; 
};

Header header @ 0x0;


u32 crc32 @ sizeof($) - 0x4;