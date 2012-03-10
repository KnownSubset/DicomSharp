#region Copyright

// 
// This library is based on Dicom# see http://sourceforge.net/projects/dicom-cs/
// Copyright (C) 2002 Fang Yang. All rights reserved.
// That library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2012 Nathan Dauber. All rights reserved.
// 
// This file is part of dicomSharp, see https://github.com/KnownSubset/DicomSharp
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.                                 
// 
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
// Nathan Dauber (nathan.dauber@gmail.com)
//

#endregion

using System;
using System.Net;
using System.Text;

namespace DicomSharp.Utility {
    public class UniqueIdGenerator {
        private static readonly string IpAddress;

        static UniqueIdGenerator() {
            try {
                IpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            }
            catch (Exception) {
                IpAddress = "127.0.0.1";
            }
        }


        /// <summary>
        /// Creates a new instance of UniqueIdGenerator
        /// </summary>
        private UniqueIdGenerator() {}

        public static UniqueIdGenerator Instance {
            get { return new UniqueIdGenerator(); }
        }

        public virtual string CreateUniqueId() {
            return CreateUniqueId(Implementation.ClassUID);
        }

        public virtual string CreateUniqueId(string root) {
            StringBuilder sb = new StringBuilder(64).Append(root).Append('.');
            sb.Append(IpAddress.Replace(".", ""));
            string str = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            sb.Append(str);
            return sb.ToString();
        }

        public static void Main() {
            string uid = Instance.CreateUniqueId();
            Console.Write(uid);
        }
    }
}