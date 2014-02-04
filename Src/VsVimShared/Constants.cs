﻿using System;

namespace VsVim
{
    internal static class Constants
    {
        /// <summary>
        /// Content Type name and display name for C++ projects
        /// </summary>
        internal const string CPlusPlusContentType = "C/C++";

        /// <summary>
        /// Content Type name and display name for C# projects
        /// </summary>
        internal const string CSharpContentType = "CSharp";

        /// <summary>
        /// Name of the main Key Processor
        /// </summary>
        internal const string VsKeyProcessorName = "VsVim";

        /// <summary>
        /// Name of the fallback Key Processor
        /// </summary>
        internal const string FallbackKeyProcessorName = "VsVimFallback";

        /// <summary>
        /// Name of the standard ICommandTarget implementation
        /// </summary>
        internal const string StandardCommandTargetName = "Standard Command Target";

        /// <summary>
        /// Name of the main Visual Studio KeyProcessor implementation
        /// </summary>
        internal const string VisualStudioKeyProcessorName = "VisualStudioKeyProcessor";

        /// <summary>
        /// This text view role was added in VS 2013.  Adding the constant here so we can refer to 
        /// it within our code as we compile against the VS 2010 binaries
        /// </summary>
        internal const string TextViewRoleEmbeddedPeekTextView = "EMBEDDED_PEEK_TEXT_VIEW";

        internal static Guid VsUserDataFileNameMoniker = new Guid(0x978a8e17, 0x4df8, 0x432a, 150, 0x23, 0xd5, 0x30, 0xa2, 100, 0x52, 0xbc);
    }
}
