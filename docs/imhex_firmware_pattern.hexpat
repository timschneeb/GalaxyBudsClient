#pragma author timschneeb
#pragma description Samsung Galaxy Buds firmware archive
#pragma pattern_limit 20000000

struct Segment {
    u32 unknownHeader1;
    u32 unknownHeader2;
    u32 unknownHeader3;
    u32 romOffsetMaybe;
    u8 data[parent.size-16];
};

struct SegmentInfo {
    u32 id;
    u32 crc32;
    u32 position;
    u32 size;
    Segment data @ position;
} [[single_color]];

struct FirmwareFile {
    u32 magic;
    u32 totalSize;
    u32 segmentsCount;
    SegmentInfo segments[segmentsCount];
    u32 crc32 @ sizeof($) - 0x4;;
};

FirmwareFile file @ 0x0;
