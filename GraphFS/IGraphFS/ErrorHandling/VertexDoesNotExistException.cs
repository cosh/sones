﻿using sones.ErrorHandling;
using System;

namespace sones.GraphFS.ErrorHandling
{
    /// <summary>
    /// A certain vertex does not exist
    /// </summary>
    public sealed class VertexDoesNotExistException : AGraphFSException
    {
        #region data

        /// <summary>
        /// The id of the type of the vertex
        /// </summary>
        public readonly UInt64 TypeID;

        /// <summary>
        /// The id of the desired vertex
        /// </summary>
        public readonly UInt64 VertexID;

        #endregion

        #region constructor

        /// <summary>
        /// Creates a new VertexDoesNotExist exception
        /// </summary>
        /// <param name="myTypeID">The vertex type id of the vertex</param>
        /// <param name="myVertexID">The id of the vertex</param>
        public VertexDoesNotExistException(UInt64 myTypeID, UInt64 myVertexID)
        {
            TypeID = myTypeID;
            VertexID = myVertexID;
        }

        #endregion

        public override ushort ErrorCode
        {
            get { return ErrorCodes.VertexDoesNotExist; }
        }
    }
}