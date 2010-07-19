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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.GraphDB.Errors
{
    public class Error_UnknownDBError : GraphDBError
    {

        public Exception ThrownException { get; private set; }

        public Error_UnknownDBError(Exception myException)
        {
            ThrownException = myException;
        }

        public Error_UnknownDBError(String myErrorMessage)
        {
            ThrownException = new Exception(myErrorMessage);
        }

        public override string ToString()
        {
            if (ThrownException.InnerException != null)
                return String.Format("An unknown GraphDB error occured: {0} Exception: {1} Stacktrace: {2}", Environment.NewLine, ThrownException.InnerException.Message + Environment.NewLine, ThrownException.InnerException.StackTrace);
            else
                return String.Format("An unknown GraphDB error occured: {0} Exception: {1} Stacktrace: {2}", Environment.NewLine, ThrownException.Message + Environment.NewLine, ThrownException.StackTrace);
        }

    }
}