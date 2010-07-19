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


/* PandoraLib - NFileSystem
 * (c) Stefan Licht, 2009
 * 
 * This class groups all Notification for the Filesystem.
 * 
 * Lead programmer:
 *      Stefan Licht
 * 
 * */

using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using sones.Notifications;
using sones.Notifications.NotificationTypes;

namespace sones.GraphFS.Notification
{
    /// <summary>
    /// This class groups all Notification for the Filesystem.
    /// </summary>
    public class NException : ANotificationType
    {

        public NException()
            : base(IPAddress.Parse("224.10.10.13"), 5000)
        { }

        public override string Description
        {
            get { return "This class groups all Notification for any kind of exception."; }
        }


        public override INotificationArguments GetEmptyArgumentInstance()
        {
            throw new NotImplementedException();
        }
    }
}