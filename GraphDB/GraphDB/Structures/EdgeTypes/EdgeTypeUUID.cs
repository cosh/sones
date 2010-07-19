﻿/*
* sones GraphDB - OpenSource Graph Database - http://www.sones.com
* Copyright (C) 2007-2010 sones GmbH
*
* This file is part of sones GraphDB OpenSource Edition.
*
* sones GraphDB OSE is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
*
* sones GraphDB OSE is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB OSE. If not, see <http://www.gnu.org/licenses/>.
*/

/* <id name="sones GraphDB – EdgeTypeUUID" />
 * <copyright file="EdgeTypeUUID.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>This class has been created in favour of getting compile errors when referencing a type.</summary>
 */

#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using sones.Lib.Serializer;
using sones.Lib.NewFastSerializer;
using sones.Lib.DataStructures.UUID;

#endregion

namespace sones.GraphDB.TypeManagement
{
    /// <summary>
    /// This class has been created in favour of getting compile errors when referencing an attribute.
    /// </summary>
    
    public class EdgeTypeUUID : UUID
    {
        #region TypeCode 
        public override UInt32 TypeCode { get { return 456; } }
        #endregion

        #region Constructors

        #region EdgeTypeUUID()

        public EdgeTypeUUID()
            : base()
        {
        }

        #endregion

        #region EdgeTypeUUID(myInt32)

        public EdgeTypeUUID(UInt64 myInt32)
            : base(myInt32.ToString())
        {
        }

        #endregion

        #region EdgeTypeUUID(myString)

        public EdgeTypeUUID(String myString)
            : base(myString)
        {
        }

        #endregion

        #region EdgeTypeUUID(mySerializedData)

        public EdgeTypeUUID(Byte[] mySerializedData)
            : base(mySerializedData)
        {
        }

        #endregion

        #region EdgeTypeUUID(ref mySerializationReader)

        public EdgeTypeUUID(ref SerializationReader mySerializationReader)
            : base(ref mySerializationReader)
        { }

        #endregion

        #endregion

    }
}