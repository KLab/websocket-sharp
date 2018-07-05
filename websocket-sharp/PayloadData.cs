#region License
/*
 * PayloadData.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2013 sta.blockhead
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebSocketSharp
{
  internal class PayloadData
  {
    #region Public Const Fields

    public const ulong MaxLength = long.MaxValue;

    #endregion

    #region Public Constructors

    public PayloadData ()
       : this(new ArraySegment<byte>(new byte[]{ }), new ArraySegment<byte>(new byte[] { }), false)
    {
    }

    public PayloadData (byte[] appData)
       : this(new ArraySegment<byte>(new byte[]{ }), new ArraySegment<byte>(appData), false)
    {
    }

    public PayloadData (ArraySegment<byte> appData)
       : this(new ArraySegment<byte>(new byte[]{ }), appData, false)
    {
    }

     public PayloadData (ArraySegment<byte> extData, ArraySegment<byte> appData, bool masked)
    {
      if ((ulong) extData.Count + (ulong) appData.Count > MaxLength)
        throw new ArgumentOutOfRangeException (
          "The length of 'extData' plus 'appData' must be less than MaxLength.");

      ApplicationData = appData;
      ExtensionData = extData;
      IsMasked = masked;
    }

    #endregion

    #region Internal Properties

    internal ArraySegment<byte> ApplicationData;
    internal ArraySegment<byte> ExtensionData;

    internal bool ContainsReservedCloseStatusCode {
      get {
        return ApplicationData.Count > 1
               ? ApplicationData.Array.SubArray (ApplicationData.Offset, 2).ToUInt16 (ByteOrder.BIG).IsReserved ()
               : false;
      }
    }

    internal bool IsMasked {
      get; private set;
    }

    #endregion

    #region Public Properties

    public ulong Length {
      get {
        return (ulong) (ExtensionData.Count + ApplicationData.Count);
      }
    }

    #endregion

    #region Private Methods

    private static void mask (ArraySegment<byte> buf, byte [] key)
    {
      for (int i = 0; i < buf.Count; i++)
        buf.Array[buf.Offset + i] ^= key [i % 4];
    }

    #endregion

    #region Public Methods

    public void Mask (byte [] maskingKey)
    {
      if (ExtensionData.Count > 0)
        mask (ExtensionData, maskingKey);

      if (ApplicationData.Count > 0)
        mask (ApplicationData, maskingKey);

      IsMasked = !IsMasked;
    }

    public override string ToString ()
    {
      return BitConverter.ToString(ExtensionData.Array, ExtensionData.Offset, ExtensionData.Count) +
          BitConverter.ToString(ApplicationData.Array, ApplicationData.Offset, ApplicationData.Count);
    }

    #endregion
  }
}
