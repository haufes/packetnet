/*
This file is part of PacketDotNet

PacketDotNet is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PacketDotNet is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PacketDotNet.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 *  Copyright 2009 Chris Morgan <chmorgan@gmail.com>
 */
namespace PacketDotNet
{
    /// <summary> GRE protocol field encoding information. </summary>
    public class GREFields
    {
        /// <summary> Length of the Flags in bytes.</summary>
        public readonly static int FlagsLength = 2;
        /// <summary> Length of the Protocol in bytes.</summary>
        public readonly static int ProtocolLength = 2;
        /// <summary> Length of the Checksum in bytes (Optional).</summary>      
        public readonly static int ChecksumLength = 2;
        /// <summary> Length of the Reserved in bytes (Optional).</summary>
        public readonly static int ReservedLength = 2;
        /// <summary> Length of the Key in bytes (Optional).</summary>
        public readonly static int KeyLength = 4;
        /// <summary> Length of the Sequence Number in bytes (Optional).</summary>
        public readonly static int SequenceLength = 4;
        /// <summary> Position of the Protocol.</summary>
        public readonly static int ProtocolPosition = 2;
        /// <summary> Position of the Checksum in bytes (Optional).</summary>      
        public readonly static int ChecksumPosition = 2;


    }
}
