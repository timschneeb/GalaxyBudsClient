#if OBSOLETED && V1
using System;
using System.Diagnostics;

namespace System.Diagnostics.CodeAnalysis
{
    // Summary:
    //     Suppresses reporting of a specific static analysis tool rule violation, allowing
    //     multiple suppressions on a single code artifact.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    //[Conditional("CODE_ANALYSIS")]
    public sealed class SuppressMessageAttribute : Attribute
    {
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.CodeAnalysis.SuppressMessageAttribute
        //     class, specifying the category of the static analysis tool and the identifier
        //     for an analysis rule.
        //
        // Parameters:
        //   category:
        //     The category for the attribute.
        //
        //   checkId:
        //     The identifier of the analysis tool rule the attribute applies to.
        public SuppressMessageAttribute(string category, string checkId) { }

        // Summary:
        //     Gets the category identifying the classification of the attribute.
        //
        // Returns:
        //     The category identifying the attribute.
        public string Category { get { return null; } }
        //
        // Summary:
        //     Gets the identifier of the static analysis tool rule to be suppressed.
        //
        // Returns:
        //     The identifier of the static analysis tool rule to be suppressed.
        public string CheckId { get { return null; } }
        //
        // Summary:
        //     Gets or sets the justification for suppressing the code analysis message.
        //
        // Returns:
        //     The justification for suppressing the message.
        public string Justification { get { return null; } set { ;} }
        //
        // Summary:
        //     Gets or sets an optional argument expanding on exclusion criteria.
        //
        // Returns:
        //     A string containing the expanded exclusion criteria.
        public string MessageId { get { return null; } set { ;} }
        //
        // Summary:
        //     Gets or sets the scope of the code that is relevant for the attribute.
        //
        // Returns:
        //     The scope of the code that is relevant for the attribute.
        public string Scope { get { return null; } set { ;} }
        //
        // Summary:
        //     Gets or sets a fully qualified path that represents the target of the attribute.
        //
        // Returns:
        //     A fully qualified path that represents the target of the attribute.
        public string Target { get { return null; } set { ;} }
    }
}
#endif
