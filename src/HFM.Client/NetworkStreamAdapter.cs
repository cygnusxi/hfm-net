﻿/*
 * HFM.NET - Network Stream Adapter Class
 * Copyright (C) 2009-2012 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting;

namespace HFM.Client
{
   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
   internal interface INetworkStream : IDisposable
   {
      #region Properties

      bool CanSeek { get; }

      bool CanTimeout { get; }

      bool CanWrite { get; }

      bool DataAvailable { get; }

      long Length { get; }

      long Position { get; set; }

      int ReadTimeout { get; set; }

      int WriteTimeout { get; set; }

      bool CanRead { get; }

      #endregion

      #region Methods

      IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state);

      int EndRead(IAsyncResult asyncResult);

      int Read(byte[] buffer, int offset, int size);

      int ReadByte();

      IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state);

      void EndWrite(IAsyncResult asyncResult);

      void Write(byte[] buffer, int offset, int size);

      void WriteByte(byte value);

      void Close();

      void Close(int timeout);

      ObjRef CreateObjRef(Type requestedType);

      void Flush();

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
      object GetLifetimeService();

      object InitializeLifetimeService();

      void SetLength(long value);

      long Seek(long offset, SeekOrigin origin);

      #endregion
   }

   [CoverageExclude]
   internal sealed class NetworkStreamAdapter : INetworkStream
   {
      private readonly NetworkStream _networkStream;

      public NetworkStreamAdapter(NetworkStream networkStream)
      {
         _networkStream = networkStream;
      }

      #region Properties

      public bool CanSeek
      {
         get { return _networkStream.CanSeek; }
      }

      public bool CanTimeout
      {
         get { return _networkStream.CanTimeout; }
      }

      public bool CanWrite
      {
         get { return _networkStream.CanWrite; }
      }

      public bool DataAvailable
      {
         get { return _networkStream.DataAvailable; }
      }

      public long Length
      {
         get { return _networkStream.Length; }
      }

      public long Position
      {
         get { return _networkStream.Position; }
         set { _networkStream.Position = value; }
      }

      public int ReadTimeout
      {
         get { return _networkStream.ReadTimeout; }
         set { _networkStream.ReadTimeout = value; }
      }

      public int WriteTimeout
      {
         get { return _networkStream.WriteTimeout; }
         set { _networkStream.WriteTimeout = value; }
      }

      public bool CanRead
      {
         get { return _networkStream.CanRead; }
      }

      #endregion

      #region Methods

      public IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
      {
         return _networkStream.BeginRead(buffer, offset, size, callback, state);
      }

      public int EndRead(IAsyncResult asyncResult)
      {
         return _networkStream.EndRead(asyncResult);
      }

      public int Read(byte[] buffer, int offset, int size)
      {
         return _networkStream.Read(buffer, offset, size);
      }

      public int ReadByte()
      {
         return _networkStream.ReadByte();
      }

      public IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
      {
         return _networkStream.BeginWrite(buffer, offset, size, callback, state);
      }

      public void EndWrite(IAsyncResult asyncResult)
      {
         _networkStream.EndWrite(asyncResult);
      }

      public void Write(byte[] buffer, int offset, int size)
      {
         _networkStream.Write(buffer, offset, size);
      }

      public void WriteByte(byte value)
      {
         _networkStream.WriteByte(value);
      }

      public void Close()
      {
         _networkStream.Close();
      }

      public void Close(int timeout)
      {
         _networkStream.Close(timeout);
      }

      public ObjRef CreateObjRef(Type requestedType)
      {
         return _networkStream.CreateObjRef(requestedType);
      }

      public void Flush()
      {
         _networkStream.Flush();
      }

      public object GetLifetimeService()
      {
         return _networkStream.GetLifetimeService();
      }

      public object InitializeLifetimeService()
      {
         return _networkStream.InitializeLifetimeService();
      }

      public void SetLength(long value)
      {
         _networkStream.SetLength(value);
      }

      public long Seek(long offset, SeekOrigin origin)
      {
         return _networkStream.Seek(offset, origin);
      }

      public void Dispose()
      {
         _networkStream.Dispose();
      }

      #endregion
   }
}