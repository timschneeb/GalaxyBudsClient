using System;
namespace InTheHand.Net.Bluetooth
{

    /// <summary>
    /// A Service Attribute Id identifies each attribute within an SDP service record.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>The content of the record for a particular service class is defined in the
    /// profile&#x2019;s specification along with the IDs it uses. The IDs for the 
    /// common standard services have beed defined here, as e.g. 
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ObexAttributeId"/>,
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BasicPrintingProfileAttributeId"/>,
    /// etc, see namespace <see cref="N:InTheHand.Net.Bluetooth.AttributeIds"/>.
    /// The Service Discovery profile itself defines IDs, some that can be used 
    /// in any record <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId"/>, 
    /// and others
    /// <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId"/>,
    /// and <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId"/>.
    /// </para>
    /// <para>Note that except for the attributes in the &#x201C;Universal&#x201D; category 
    /// the IDs are <i>not</i> unique, for instance the ID is 0x0200 for both 
    /// <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId.VersionNumberList"/>
    /// and <see cref="F:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId.GroupId"/>
    /// from <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.ServiceDiscoveryServerAttributeId"/>
    /// and <see cref="T:InTheHand.Net.Bluetooth.AttributeIds.BrowseGroupDescriptorAttributeId"/>
    /// respectively.
    /// </para>
    /// </remarks>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
#endif
    public enum ServiceAttributeId : short
    {
        // Playing
        // warning: "ServiceAttributeId': base type 'ushort' is not CLS-compliant"
    }


    //--------------------------------------------------------------------------


    /// <summary>
    /// Retrieves the name of the SDP Attribute ID with the given value in the
    /// specified Attribute ID class sets.  Implementing <see cref="T:System.Enum"/>-like
    /// behaviour.
    /// </summary>
    public 
#if ! V1
         static 
#endif
            class AttributeIdLookup
    {
#if V1
        private AttributeIdLookup() { }
#endif

        /// <summary>
        /// Retrieves the name of the SDP Attribute ID with the given value in the
        /// specified Attribute ID class sets.
        /// </summary>
        /// -
        /// <remarks>
        /// Each particular service (ObexPushProfile, SerialPortProfile) etc defines
        /// its own SDP record content and the Attribute IDs are defined locally in
        /// each, and thus with values overlapping with other service specifications.
        /// Therefore for each profile we must define the set of Attribute IDs used, this
        /// is done by creating a class for each with the IDs defined as <c>const</c>
        /// member fields.
        /// </remarks>
        /// -
        /// <param name="id">
        /// The Attribute Id as an <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>
        /// </param>
        /// <param name="attributeIdDefiningClasses">
        /// The set of classes defining Attribute IDs for the service classed contained
        /// in the record containing this attribute id.
        /// </param>
        /// -
        /// <returns>
        /// A string containing the name of the Attribute ID whose numerical value is <paramref name="id"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        public static string GetName(ServiceAttributeId id, Type[] attributeIdDefiningClasses)
        {
            LanguageBaseItem applicableLangBase;
            string name = GetName(id, attributeIdDefiningClasses,
                new LanguageBaseItem[0], //HACK new LanguageBaseItem[0] -- instead of -- null
                out applicableLangBase);
            return name;
        }

        /// <summary>
        /// Retrieves the name of the SDP Attribute ID with the given value 
        /// and using one of the languages from the supplied LanguageBaseItem 
        /// in the specified AttributeID class sets.
        /// </summary>
        /// -
        /// <remarks>
        /// Each particular service (ObexPushProfile, SerialPortProfile) etc defines
        /// its own SDP record content and the Attribute IDs are defined locally in
        /// each, and thus with values overlapping with other service specifications.
        /// Therefore for each profile we must define the set of Attribute IDs used, this
        /// is done by creating a class for each with the IDs defined as <c>const</c>
        /// member fields.
        /// </remarks>
        /// -
        /// <param name="id">
        /// The Attribute Id as an <see cref="T:InTheHand.Net.Bluetooth.ServiceAttributeId"/>
        /// </param>
        /// <param name="attributeIdDefiningClasses">
        /// The set of classes defining Attribute IDs for the service classed contained
        /// in the record containing this attribute id.
        /// </param>
        /// <param name="langBaseList">
        /// The list of <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> applying 
        /// to the current record.  They are used when an attribute is marked as a
        /// multi-language one and thus need the base offset removed from the specified
        /// numerical value.
        /// </param>
        /// <param name="applicableLangBase">
        /// The applicable <see cref="T:InTheHand.Net.Bluetooth.LanguageBaseItem"/> if the 
        /// matched attribute is a multi-language one.  <see langword="null"/> 
        /// (<see langword="Nothing"/> in Visual Basic), if no attribute was matched
        /// or it was not a multi-language one.
        /// </param>
        /// -
        /// <returns>
        /// A string containing the name of the Attribute ID whose numerical value is <paramref name="id"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
#endif
        public static string GetName(ServiceAttributeId id, Type[] attributeIdDefiningClasses,
            LanguageBaseItem[] langBaseList, out LanguageBaseItem applicableLangBase)
        {
            if (attributeIdDefiningClasses == null) {
                throw new ArgumentNullException("attributeIdDefiningClasses");
            }
            //HACK if (langBaseList == null) {
            if (langBaseList == null) {
                throw new ArgumentNullException("langBaseList");
            }
            // Foreach: class that defines AttributeId enum.
            //    Foreach: AttributeId enum field in that class.
            //       Check whether its value matches the one being searched for, and return if so.
            //
            foreach (Type curDefiningType in attributeIdDefiningClasses) {
                //if (!(curDefiningType.IsSealed && curDefiningType.IsAbstract)) { }
                //----
                System.Reflection.FieldInfo[] fieldArr = curDefiningType.GetFields(
                    // With Public, no permissions required, apparently.
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (System.Reflection.FieldInfo curField in fieldArr) {
                    if (curField.FieldType == typeof(ServiceAttributeId)) {
                        // A multi-language attribute or a just a normal one?
                        Object[] dotnetAtttrs = curField.GetCustomAttributes(typeof(StringWithLanguageBaseAttribute), false);
                        if (dotnetAtttrs.Length != 0) {
                            System.Diagnostics.Debug.Assert(dotnetAtttrs.Length == 1,
                                "Not that it's a problem for us at all, but that Attribute should only be applied once.");
                            string name = _GetNameIfMatchesMultiLang(id, curField, langBaseList, out applicableLangBase);
                            if (name != null) {
                                return name;
                            }
                        } else {
                            // No just a normal Attribute, not language base offsetting.
                            string name = _GetNameIfMatches(id, curField);
                            if (name != null) {
                                applicableLangBase = null;
                                return name;
                            }
                        }//else
                    }
                }//foreach
            }//foreach
            // Not found.
            applicableLangBase = null;
            return null;
        }

        /// <summary>
        /// Retrieves the name of the SDP Attribute ID with the given value 
        /// and using one of the languages from the supplied LanguageBaseItem 
        /// in the specified AttributeID class sets
        /// </summary>
        private static string 
            _GetNameIfMatchesMultiLang(ServiceAttributeId id, System.Reflection.FieldInfo curField,
            LanguageBaseItem[] langBaseList, out LanguageBaseItem applicableLangBase)
        {
            foreach (LanguageBaseItem curBaseItem in langBaseList) {
                ServiceAttributeId baseOffset = curBaseItem.AttributeIdBase;
                ServiceAttributeId realId = id;
                unchecked { realId -= baseOffset; }
                // (Theorically 'unchecked' above could allow wrong results but
                // only 0, 1, and 2, have "[StringWithLanguageBaseAttribute]",
                // and it would be an odd record that could produce those
                // integers for wrong reasons).
                string fieldName = _GetNameIfMatches(realId, curField);
                if (fieldName != null) {
                    applicableLangBase = curBaseItem;
                    return fieldName;
                }
            }//foreach
            applicableLangBase = null;
            return null;
        }

        // Check if the current field has the supplied value.
        private static string _GetNameIfMatches(ServiceAttributeId id, System.Reflection.FieldInfo curField)
        {
            object rawValue;
#if NETCF
            // This does the job too.  As a static field the parameter is ignored.
            rawValue = curField.GetValue(null);
#else
            // Does this require less permissions than the GetValue version.
            rawValue = curField.GetRawConstantValue();
#endif
            ServiceAttributeId fieldValue = (ServiceAttributeId)rawValue;
            if (fieldValue == id) {
                string fieldName = curField.Name;
                return fieldName;
            }
            return null;
        }

    }//class


    ///// <summary>
    ///// Mark a class containing Attribute ID definitions as applying to particular Service Classes.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    //sealed class AttributeIdsOfServiceClassAttribute : System.Attribute
    //{
    //    // See the attribute guidelines at 
    //    //  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconusingattributeclasses.asp
    //    readonly Int16 m_SvcClassId;

    //    // alanjmcf: Note we can't use Guid variable as: "An attribute argument must
    //    //   be a constant expression, typeof expression or array creation expression"
    //    public AttributeIdsOfServiceClassAttribute(Int16 serviceClassId)
    //    {
    //        m_SvcClassId = serviceClassId;
    //    }

    //    public Int16 ServiceClassId { get { return m_SvcClassId; } }
    //}//class


    /// <summary>
    /// Indicates that the field to which it is applied represents an SDP Attribute 
    /// that can exist in multiple language instances and thus has a language base 
    /// offset applied to its numerical ID when added to a record.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class StringWithLanguageBaseAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.StringWithLanguageBaseAttribute"/>
        /// class. 
        /// </summary>
        public StringWithLanguageBaseAttribute()
        { }

    }//class


}//namespace
