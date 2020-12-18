// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Mime.MediaTypeNames
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Mime
{
	/// <summary>
	/// Specifies the media type information for an object.
	/// </summary>
	public static class MediaTypeNames
	{

		/// <summary>
		/// Specifies the type of image data in an object.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class Image
		{
			/// <summary>
			/// Specifies that the image data is in Graphics Interchange Format (GIF).
			/// </summary>
			public const string Gif = "image/gif";

			/// <summary>
			/// Specifies that the image data is in Joint Photographic Experts Group (JPEG) format.
			/// </summary>
			public const string Jpg = "image/jpg";
		}


		/// <summary>
		/// Specifies the type of text data in an object.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
		public static class Text
		{
			/// <summary>
			/// Specifies that the data is in HTML format.
			/// </summary>
			public const string Html = "text/html";

			/// <summary>
			/// Specifies that the data is in plain text format.
			/// </summary>
			public const string Plain = "text/plain";

			/// <summary>
			/// Specifies that the data is in vCalendar format.
            /// </summary>
#if CODE_ANALYSIS
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
#endif
            public const string vCalendar = "text/x-vcal";

			/// <summary>
			/// Specifies that the data is in vCard format.
            /// </summary>
#if CODE_ANALYSIS
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
#endif
            public const string vCard = "text/x-vcard";

			/// <summary>
			/// Specifies that the data is in vMsg format.
            /// </summary>
#if CODE_ANALYSIS
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
#endif
            public const string vMessage = "text/x-vMsg";

			/// <summary>
			/// Specifies that the data is in vNote format.
            /// </summary>
#if CODE_ANALYSIS
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
#endif
            public const string vNote = "text/x-vnote";

			/// <summary>
			/// Specifies that the data is in XML format.
			/// </summary>
			public const string Xml = "text/xml";
		}

		/// <summary>
		/// Specifies the type of Object Exchange specific data.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class ObjectExchange
		{
			/// <summary>
			/// Used to retrieve supported object types.
			/// </summary>
			public const string Capabilities = "x-obex/capability";

			/// <summary>
			/// Used to retrieve folder listing with OBEX FTP.
			/// </summary>
			public const string FolderListing = "x-obex/folder-listing";

			/// <summary>
			/// Used to retrieve an object profile.
			/// </summary>
			public const string ObjectProfile = "x-obex/object-profile";
		}
	}
}
