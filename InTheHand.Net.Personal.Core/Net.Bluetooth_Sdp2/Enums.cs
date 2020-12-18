namespace InTheHand.Net.Bluetooth
{

#if V1
    /*
#endif
#pragma warning disable 1591
#if V1
    */
#endif


    /// <summary>
    /// Represents the type of the element in the SDP record binary format, 
    /// and is stored as the higher 5 bits of the header byte.
    /// </summary>
    /// <remarks>
    /// There is an identifier for each major type: String vs UUID vs unsigned integer.
    /// There are various sizes of UUID and integer type for instance, the resultant
    /// types are listed in enum <see cref="T:InTheHand.Net.Bluetooth.ElementType"/>.
    /// </remarks>
    public enum ElementTypeDescriptor //: byte
    {
        Unknown = -1,
        //--
        Nil = 0,
        UnsignedInteger = 1,
        TwosComplementInteger = 2,
        Uuid = 3,
        TextString = 4,
        Boolean = 5,
        ElementSequence = 6, 
        ElementAlternative = 7,
        Url = 8
    }//enum


    /// <summary>
    /// Represents the size of the SDP element in the record binary format,
    /// and is stored as the lower 3 bits of the header byte.
    /// </summary>
    /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.SplitHeaderByte(System.Byte,InTheHand.Net.Bluetooth.ElementTypeDescriptor@,InTheHand.Net.Bluetooth.SizeIndex@)"/>
    /// <seealso cref="M:InTheHand.Net.Bluetooth.ServiceRecordParser.GetSizeIndex(System.Byte)"/>
    public enum SizeIndex //: byte
    {
        LengthOneByteOrNil = 0,
        LengthTwoBytes = 1,
        LengthFourBytes = 2,
        LengthEightBytes = 3,
        LengthSixteenBytes = 4,
        //
        AdditionalUInt8 = 5,
        AdditionalUInt16 = 6,
        AdditionalUInt32 = 7,
    }//enum


    /// <summary>
    /// Represents the types that an SDP element can hold.
    /// </summary>
    /// <remarks>
    /// <para>
    /// (Is a logical combination of the <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/>
    /// field which defines the major type and the size field in the binary format; and
    /// the size field being made up of the <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/>
    /// field and any additional length bytes.
    /// </para>
    /// <para>Note, the values here are not the numerical bitwise combination of the 
    /// <see cref="T:InTheHand.Net.Bluetooth.ElementTypeDescriptor"/> and 
    /// <see cref="T:InTheHand.Net.Bluetooth.SizeIndex"/> fields as they appear 
    /// in the encoded protocol.  It was simpler to assign arbitrary values here as 
    /// firstly we wanted zero to be the 'Unknown' value, which conflicts with Nil's
    /// bitwise value; but also because the TextString, sequence and Url types can 
    /// have various SizeIndex values and thus they wouldn&#x2019;t be easily 
    /// representable by one value here).
    /// </para>
    /// </remarks>
    public enum ElementType
    {
        Unknown = 0,

        //--
        // TypeDescriptor.Nil = 0,
        //--
        Nil = 20,

        //--
        // TypeDescriptor.UnsignedInteger = 1,
        //--
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        UInt128,

        //--
        // TypeDescriptor.TwosComplementInteger = 2,
        //--
        Int8 = 30,
        Int16,
        Int32,
        Int64,
        Int128,

        //--
        // TypeDescriptor.Uuid = 3,
        //--
        Uuid16 = 40,
        Uuid32,
        Uuid128,

        //--
        // TypeDescriptor.TextString = 4,
        //--
        TextString,

        //--
        // TypeDescriptor.Boolean = 5,
        //--
        Boolean,

        //--
        // TypeDescriptor.DataElementSequence = 6,
        //--
        ElementSequence,

        //--
        // TypeDescriptor.DataElementAlternative = 7,
        //--
        ElementAlternative,

        //--
        // TypeDescriptor.Url = 8
        //--
        Url,
    }//enum


}