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

namespace DicomSharp.Net {
    /// <summary> 
    /// </summary>
    public interface IDicomService {
        void CStore(ActiveAssociation assoc, IDimse rq);
        void CGet(ActiveAssociation assoc, IDimse rq);
        void CFind(ActiveAssociation assoc, IDimse rq);
        void CMove(ActiveAssociation assoc, IDimse rq);
        void CEcho(ActiveAssociation assoc, IDimse rq);
        void NCreate(ActiveAssociation assoc, IDimse rq);
        void NSet(ActiveAssociation assoc, IDimse rq);
        void NGet(ActiveAssociation assoc, IDimse rq);
        void NDelete(ActiveAssociation assoc, IDimse rq);
        void NAction(ActiveAssociation assoc, IDimse rq);
        void NEventReport(ActiveAssociation assoc, IDimse rq);
    }
}