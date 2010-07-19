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


/*
 * AFSStructure
 * Achim Friedland, 2008 - 2009
 * 
 * The abstract class for all PandoraFS file system structures
 * 
 *  * Layout of the APandoraStructure Header:
 * -------------------------------------------------
 * IFastSerialize Header            - 4 bytes
 * 
 * Header Version                   - 1 byte 
 * Length of IntegrityCheckValue    - 1 byte  >> 3
 * Length of EncryptionParameters   - 1 byte  >> 3
 * Length of Data Padding           - 1 bytes
 * Length of Additional Padding     - 1 bytes >> 3
 * Reserved                         - 2 bytes
 * IntegrityCheckValue              - n bytes
 * EncryptionParameters             - m bytes
 * 
 * StructureVersion                 - 2 bytes
 * ObjectUUID                       - o bytes
 * SerializedObject                 - p bytes
 * 
 * Data Padding                     - q bytes
 * Additional Padding               - r bytes
 * 
 * Lead programmer:
 *      Achim Friedland
 * 
 */

#region Usings

using System;
using System.Text;
using System.Runtime.Serialization;

using sones.Lib;
using sones.Lib.Serializer;
using sones.Lib.Cryptography.IntegrityCheck;
using sones.Lib.Cryptography.SymmetricEncryption;
using sones.Lib.NewFastSerializer;
using sones.GraphFS.Objects;
using sones.GraphFS.Exceptions;

#endregion

namespace sones.GraphFS.DataStructures
{

    /// <summary>
    /// The abstract class for all file system structures
    /// </summary>

    public abstract class AFSStructure : AObjectHeader, IFastSerialize
    {

        #region Constructors

        #region AFSStructure()

        /// <summary>
        /// This will set all important variables within this AFSStructure.
        /// This will especially create a new ObjectUUID and mark the
        /// APandoraStructure as "new" and "dirty".
        /// </summary>
        public AFSStructure()
        {

            // Members of APandoraStructure
            _isNew                   = true;
            _StructureVersion        = 1;
            _IntegrityCheckValue     = null;
            _EncryptionParameters    = null;
            ObjectUUID               = new ObjectUUID(false);
            _INodeReference          = null;
            _ObjectLocatorReference  = null;

            // Members of IFastSerialize
            isDirty                  = true;

        }

        #endregion

        #region AFSStructure(myObjectUUID)

        /// <summary>
        /// This will set all important variables within this AFSStructure.
        /// Additionally it sets the ObjectUUID to the given value and marks
        /// the APandoraStructure as "new" and "dirty".
        /// </summary>
        public AFSStructure(ObjectUUID myObjectUUID)
            : this()
        {
            ObjectUUID              = myObjectUUID;
        }

        #endregion

        #endregion


        #region New (De-)Serialization and Cloning

        #region Serialize(myIntegrityCheckAlgorithm, myEncryptionAlgorithm, myCacheSerializeData)

        /// <summary>
        /// This will serialize the whole PandoraObject including the common header of an
        /// APandoraObject and the actual PandoraObject
        /// </summary>
        /// <param name="myIntegrityCheckAlgorithm"></param>
        /// <param name="myEncryptionAlgorithm"></param>
        /// <param name="myCacheSerializeData"></param>
        /// <returns></returns>
        public Byte[] Serialize(IIntegrityCheck myIntegrityCheckAlgorithm, ISymmetricEncryption myEncryptionAlgorithm, Boolean myCacheSerializeData)
        {

            #region Data

            Int32   IntegrityCheckValue_Length      = 0;
            Byte[]  IntegrityCheckValue             = null;
            Int64   IntegrityCheckValue_Position    = 0;

            Int32   EncryptionParameters_Length     = 0;
            Byte    DataPadding_Length              = 0;
            Int32   AdditionalPadding_Length        = 0;
            Byte[]  _TmpSerializedAPandoraStructure = null;

            #endregion

            try
            {

                #region Init SerializationWriter

                var _SerializationWriter = new SerializationWriter();

                #endregion

                #region Pad the length of the EncryptionParameters

                EncryptionParameters         = Encoding.Default.GetBytes("-HIGHSECUREDATA-");
                EncryptionParameters_Length  = sones.Lib.BufferHelper.AlignBufferLength(EncryptionParameters.Length, 8);

                #endregion

                #region Serialize APandoraObjectHeader

                var APandoraObjectHeader = new Byte[HeaderLength];

                APandoraObjectHeader[0] = HeaderVersion;

                if (myIntegrityCheckAlgorithm != null)
                    IntegrityCheckValue_Length = myIntegrityCheckAlgorithm.HashSize;

                if (IntegrityCheckValue_Length  % 8 == 0)
                         APandoraObjectHeader[1] = (Byte)  (IntegrityCheckValue_Length  / 8);
                    else APandoraObjectHeader[1] = (Byte) ((IntegrityCheckValue_Length  / 8) + 1);

                if (EncryptionParameters_Length % 8 == 0)
                         APandoraObjectHeader[2] = (Byte)  (EncryptionParameters_Length / 8);
                    else APandoraObjectHeader[2] = (Byte) ((EncryptionParameters_Length / 8) + 1);

                APandoraObjectHeader[3] = (Byte)(DataPadding_Length);
                APandoraObjectHeader[4] = (Byte)(AdditionalPadding_Length / 256);
                APandoraObjectHeader[5] = (Byte)(AdditionalPadding_Length % 256);
                APandoraObjectHeader[6] = 0x00;
                APandoraObjectHeader[7] = 0x00;

                _SerializationWriter.WriteBytesDirect(APandoraObjectHeader);                      // 8 Bytes
                IntegrityCheckValue_Position = _SerializationWriter.BaseStream.Position;
                _SerializationWriter.WriteBytesDirect(new Byte[IntegrityCheckValue_Length]);      // n or at least 16 Bytes
                _SerializationWriter.WriteBytesDirect(EncryptionParameters);                      // m Bytes

                #endregion

                #region Serialize StructureVersion, ObjectUUID and the Inner Object

                _SerializationWriter.WriteBytesDirect(BitConverter.GetBytes(_StructureVersion));
                ObjectUUID.Serialize(ref _SerializationWriter);        // n or at least 16 Bytes

                Serialize(ref _SerializationWriter);

                #endregion

                _TmpSerializedAPandoraStructure = _SerializationWriter.ToArray();

                #region Encrypt
                #endregion

                #region Add IntegrityCheck

                if (myIntegrityCheckAlgorithm != null && IntegrityCheckValue_Length > 0)
                {

                    IntegrityCheckValue = myIntegrityCheckAlgorithm.GetHashValueAsByteArray(_TmpSerializedAPandoraStructure);
  
                    // If the returned array is shorter than expected => pad with 0x00
                    // And if it is longer just copy the number of expected bytes
                    Array.Copy(IntegrityCheckValue, 0, _TmpSerializedAPandoraStructure, IntegrityCheckValue_Position, IntegrityCheckValue.Length);

                }

                #endregion

                isDirty = false;

                if (myCacheSerializeData)
                    _SerializedAPandoraStructure = _TmpSerializedAPandoraStructure;


                _EstimatedSize = (UInt64) _TmpSerializedAPandoraStructure.LongLength;

                return _TmpSerializedAPandoraStructure;

            }

            catch (SerializationException e)
            {
                throw new SerializationException(e.Message, e);
            }

        }

        #endregion

        #region Deserialize(mySerializedData, myIntegrityCheckAlgorithm, myEncryptionAlgorithm)

        public void Deserialize(Byte[] mySerializedData, IIntegrityCheck myIntegrityCheckAlgorithm, ISymmetricEncryption myEncryptionAlgorithm)
        {

            #region Data

            Byte[]  IntegrityCheckValue;
            int     IntegrityCheckValue_Length;
            Int64   IntegrityCheckValue_Position;
            Byte[]  actualIntegrityCheckValue;

            Byte[]  EncryptionParameters;
            int     EncryptionParameters_Length;

            int     DataPadding_Length;
            int     AdditionalPadding_Length = 0;

            #endregion

            #region Check if data is larger than the minimum allowed size

            if (mySerializedData == null)
                throw new PandoraFSException_InvalidInformationHeader("The information header is invalid!");

            if (mySerializedData.Length < 8)
                throw new PandoraFSException_InvalidInformationHeader("The information header is invalid!");

            #endregion

            try
            {

                #region Init reader

                var reader = new SerializationReader(mySerializedData);

                #endregion
                
                #region Read HeaderVersion

                var _HeaderVersion = reader.ReadByte();

                #endregion

                #region Read Length of the Integrity Check Value

                // Multiply the value of the first byte with 8
                IntegrityCheckValue_Length = reader.ReadByte() << 3;

                if (IntegrityCheckValue_Length > mySerializedData.Length - HeaderLength)
                    throw new PandoraFSException_InvalidIntegrityCheckLengthField("The length of the integrity check value is invalid!");

                // HACK: Remeber that a IntegrityCheckValue of 0 will circumvent the whole integrity checking!
                if (myIntegrityCheckAlgorithm != null)
                    if ((IntegrityCheckValue_Length > 0) && (IntegrityCheckValue_Length != myIntegrityCheckAlgorithm.HashSize))
                        throw new PandoraFSException_InvalidIntegrityCheckLengthField("The length of the integrity check value is " + IntegrityCheckValue_Length + ", but " + myIntegrityCheckAlgorithm.HashSize + " was expected!");

                #endregion

                #region Read Length of the Encryption Parameters

                // Multiply the value of the second byte with 8
                EncryptionParameters_Length = reader.ReadByte() << 3;

                if (EncryptionParameters_Length > mySerializedData.Length - HeaderLength - IntegrityCheckValue_Length)
                    throw new PandoraFSException_InvalidEncryptionParametersLengthField("The length of the encryption parameters is invalid!");

                #endregion

                #region Read Padding lengths

                DataPadding_Length        = (Int32) reader.ReadByte();
                AdditionalPadding_Length  = (Int32) (256 * reader.ReadByte() + reader.ReadByte()) << 3;

                if ((HeaderLength + IntegrityCheckValue_Length + EncryptionParameters_Length + AdditionalPadding_Length) >= mySerializedData.Length)
                    throw new PandoraFSException_InvalidAdditionalPaddingLengthField("The length of the additional padding is invalid!");

                reader.ReadBytesDirect(2);  // Read reserved bytes

                #endregion
               
                #region Read Integrity Check Value and Encryption Parameters

                IntegrityCheckValue_Position = reader.BaseStream.Position;

                if (IntegrityCheckValue_Length > 0)
                    IntegrityCheckValue = reader.ReadBytesDirect(IntegrityCheckValue_Length);

                if (EncryptionParameters_Length > 0)
                    EncryptionParameters = reader.ReadBytesDirect(EncryptionParameters_Length);

                #endregion


                #region Verify the integrity of the data

                if (myIntegrityCheckAlgorithm != null && IntegrityCheckValue_Length > 0)
                {

                    // Save the read IntegrityCheckValue
                    IntegrityCheckValue = new Byte[IntegrityCheckValue_Length];
                    Array.Copy(mySerializedData, IntegrityCheckValue_Position, IntegrityCheckValue, 0, IntegrityCheckValue_Length);

                    // Zero the IntegrityCheckValue within the serialized data
                    var AllZeros = new Byte[IntegrityCheckValue_Length];
                    Array.Copy(AllZeros, 0, mySerializedData, IntegrityCheckValue_Position, IntegrityCheckValue_Length);

                    // Calculate the actual IntegrityCheckValue
                    actualIntegrityCheckValue = myIntegrityCheckAlgorithm.GetHashValueAsByteArray(mySerializedData);

                    // Compare read and actual IntegrityCheckValue
                    if (IntegrityCheckValue.CompareByteArray(actualIntegrityCheckValue) != 0)
                        throw new PandoraFSException_IntegrityCheckFailed(String.Concat("The IntegrityCheck failed as ", actualIntegrityCheckValue.ToHexString(), " is not equal to the expected ", IntegrityCheckValue.ToHexString()));

                }

                #endregion

                #region Decrypt the remaining data

                //EncryptedData_Length      = (UInt64) (mySerializedData.Length - (HeaderLength + IntegrityCheckValue_Length + EncryptionParameters_Length + AdditionalPadding_Length));
                //EncryptedData             = new Byte[EncryptedData_Length];
                //Array.Copy(mySerializedData, HeaderLength + IntegrityCheckValue_Length + EncryptionParameters_Length, EncryptedData, 0, (Int64) EncryptedData_Length);

                //#endregion

                //#region Decrypt Data

                // Decrypt Data, sooon...!

                //if ( (UInt64) DataPadding_Length >= EncryptedData_Length)
                //    throw new PandoraFSException_InvalidDataPaddingLengthField("The length of the data padding is invalid!");

                //DecryptedData = new Byte[EncryptedData_Length - (UInt64) DataPadding_Length];
                //Array.Copy(EncryptedData, 0, DecryptedData, 0, (Int64) (EncryptedData_Length - (UInt64) DataPadding_Length));

                #endregion


                #region Deserialize Inner Object

                _StructureVersion       = BitConverter.ToUInt16(reader.ReadBytesDirect(2), 0);
                ObjectUUID              = new ObjectUUID((Byte[]) reader.ReadObject());   // n or at least 16 Bytes

                Deserialize(ref reader);

                #endregion


            }

            catch (PandoraFSException_IntegrityCheckFailed e)
            {
                throw new PandoraFSException_IntegrityCheckFailed("The APandoraStructure could not be deserialized as its integrity is corrupted!\n\n" + e);
            }

            catch (Exception e)
            {
                throw new PandoraFSException_APandoraStructureCouldNotBeDeserialized("The APandoraStructure could not be deserialized!\n\n" + e);
            }

            _SerializedAPandoraStructure = mySerializedData;
            _EstimatedSize               = (UInt64) _SerializedAPandoraStructure.LongLength;

        }

        #endregion

        #region (abstract) SerializeInnerObject(ref mySerializationWriter)

        /// <summary>
        /// This method will serialize this APandoraStructure
        /// </summary>
        /// <param name="mySerializationWriter">An SerializationWriter to write the serialized bytes to</param>
        public abstract void Serialize(ref SerializationWriter mySerializationWriter);

        #endregion

        #region (abstract) DeserializeInnerObject(ref mySerializationReader)

        /// <summary>
        /// This method will deserialize the content of the given array of bytes into this APandoraStructure
        /// </summary>
        /// <param name="mySerializationReader">An SerializationReader to read the serialized bytes from</param>
        public abstract void Deserialize(ref SerializationReader mySerializationReader);

        #endregion
     
        #region (abstract) Clone()

        /// <summary>
        /// This will create an exact deep-copy of this APandoraStructure
        /// </summary>
        /// <returns>An exact deep-copy of this APandoraStructure</returns>
        public abstract AFSStructure Clone();

        #endregion

        #endregion


        #region Members of IFastSerialize

        #region Serialize()

        /// <summary>
        /// An abstract method to serialize this file system structure or object.
        /// </summary>
        /// <returns></returns>
        public Byte[] Serialize()
        {
            return Serialize(null, null, true);
        }

        #endregion

        #region Deserialize(mySerializedData)

        /// <summary>
        /// An abstract method to deserialize this file system structure or object.
        /// </summary>
        /// <returns></returns>
        public void Deserialize(Byte[] mySerializedData)
        {
            Deserialize(mySerializedData, null, null);
        }

        #endregion

        #endregion       

    }

}