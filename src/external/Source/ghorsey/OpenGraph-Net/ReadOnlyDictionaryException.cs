// <copyright file="ReadOnlyDictionaryException.cs" company="SHHH Innovations LLC">
// Copyright SHHH Innovations LLC
// </copyright>
namespace OpenGraph_Net
{
    using System;

    /// <summary>
    /// Read-only dictionary exception
    /// </summary>
    /// <seealso cref="System.NotSupportedException" />
    [Serializable]
    public class ReadOnlyDictionaryException : NotSupportedException
    {
    }
}
