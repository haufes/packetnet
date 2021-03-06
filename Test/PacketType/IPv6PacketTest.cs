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
 *  Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Net.NetworkInformation;
using NUnit.Framework;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using PacketDotNet.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Test.PacketType
{
    [TestFixture]
    public class IPv6PacketTest
    {
        // icmpv6
        public void VerifyPacket0(Packet p, LinkLayers linkLayer)
        {
            Assert.IsNotNull(p);
            Console.WriteLine(p.ToString());

            if (linkLayer == LinkLayers.Ethernet)
            {
                EthernetPacket e = (EthernetPacket)p;
                Assert.AreEqual(PhysicalAddress.Parse("00-A0-CC-D9-41-75"), e.SourceHwAddress);
                Assert.AreEqual(PhysicalAddress.Parse("33-33-00-00-00-02"), e.DestinationHwAddress);
            }

            var ip = (IPPacket)p.Extract (typeof(IPPacket));
            Console.WriteLine("ip {0}", ip.ToString());
            Assert.AreEqual(System.Net.IPAddress.Parse("fe80::2a0:ccff:fed9:4175"), ip.SourceAddress);
            Assert.AreEqual(System.Net.IPAddress.Parse("ff02::2"), ip.DestinationAddress);
            Assert.AreEqual(IPVersion.IPv6, ip.Version);
            Assert.AreEqual(IPProtocolType.ICMPV6, ip.Protocol);
            Assert.AreEqual(16,  ip.PayloadPacket.Bytes.Length, "ip.PayloadPacket.Bytes.Length mismatch");
            Assert.AreEqual(255, ip.HopLimit);
            Assert.AreEqual(255, ip.TimeToLive);
            Assert.AreEqual(0x3a, (Byte)ip.NextHeader);
            Console.WriteLine("Failed: ip.ComputeIPChecksum() not implemented.");
        }

        // Test that we can load and parse an IPv6 packet
        // for multiple LinkLayerType types
        [TestCase("../../CaptureFiles/ipv6_icmpv6_packet.pcap", LinkLayers.Ethernet)]
        [TestCase("../../CaptureFiles/ipv6_icmpv6_packet_raw_linklayer.pcap", LinkLayers.Raw)]
        public void IPv6PacketTestParsing(String pcapPath, LinkLayers linkLayer)
        {
            var dev = new CaptureFileReaderDevice(pcapPath);
            PcapPacket p;
            using (FileStream fsin = File.Open(pcapPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                PcapStream ps = new PcapStream();
                ps.Initialize(fsin);
                p = ps.GetPacket();


                Int32 packetIndex = 0;
                while (p != null)
                {
                    switch (packetIndex)
                    {
                        case 0:
                            VerifyPacket0(p.Packet, linkLayer);
                            break;
                        default:
                            Assert.Fail("didn't expect to get to packetIndex " + packetIndex);
                            break;
                    }

                    packetIndex++;
                    p = ps.GetPacket();

                }
            }
        }

        /// <summary>
        /// Test that we can load and parse an IPv6 TCP packet and that
        /// the computed tcp checksum matches the expected checksum
        /// </summary>
        [Test]
        public void TCPChecksumIPv6()
        {
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/ipv6_http.pcap");
            dev.Open();

            // checksums from wireshark of the capture file
            Int32[] expectedChecksum = {0x41a2,
                                      0x4201,
                                      0x5728,
                                      0xf448,
                                      0xee07,
                                      0x939c,
                                      0x63e4,
                                      0x4590,
                                      0x3725,
                                      0x3723};

            Int32 packetIndex = 0;
            RawCapture rawCapture;
            while ((rawCapture = dev.GetNextPacket()) != null)
            {
                var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var t = (TcpPacket)p.Extract(typeof(TcpPacket));
                Assert.IsNotNull(t, "Expected t to not be null");
                Assert.IsTrue(t.ValidChecksum, "t.ValidChecksum isn't true");

                // compare the computed checksum to the expected one
                Assert.AreEqual(expectedChecksum[packetIndex],
                                t.CalculateTCPChecksum(),
                                "Checksum mismatch");

                packetIndex++;
            }

            dev.Close();
        }

        // Test that we can correctly set the data section of a IPv6 packet
        [Test]
        public void TCPDataIPv6()
        {
            String s = "-++++=== HELLLLOOO ===++++-";
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(s);

            //create random packet
            TcpPacket p = TcpPacket.RandomPacket();

            //replace pkt's data with our string
            p.PayloadData = data;

            //sanity check
            Assert.AreEqual(s, System.Text.Encoding.Default.GetString(p.PayloadData));
        }

        [Test]
        public void ConstructFromValues()
        {
            var sourceAddress = RandomUtils.GetIPAddress(IPVersion.IPv6);
            var destinationAddress = RandomUtils.GetIPAddress(IPVersion.IPv6);
            var ipPacket = new IPv6Packet(sourceAddress, destinationAddress);

            Assert.AreEqual(sourceAddress, ipPacket.SourceAddress);
            Assert.AreEqual(destinationAddress, ipPacket.DestinationAddress);
        }

        [Test]
        public void PrintString()
        {
            Console.WriteLine("Loading the sample capture file");
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/ipv6_http.pcap");
            dev.Open();
            Console.WriteLine("Reading packet data");
            var rawCapture = dev.GetNextPacket();
            var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            Console.WriteLine("Parsing");
            var ip = (IPv6Packet)p.Extract(typeof(IPv6Packet));

            Console.WriteLine("Printing human readable string");
            Console.WriteLine(ip.ToString());
        }

        [Test]
        public void PrintVerboseString()
        {
            Console.WriteLine("Loading the sample capture file");
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/ipv6_http.pcap");
            dev.Open();
            Console.WriteLine("Reading packet data");
            var rawCapture = dev.GetNextPacket();
            var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            Console.WriteLine("Parsing");
            var ip = (IPv6Packet)p.Extract(typeof(IPv6Packet));

            Console.WriteLine("Printing human readable string");
            Console.WriteLine(ip.ToString(StringOutputType.Verbose));
        }

        [Test]
        public void RandomPacket()
        {
            IPv6Packet.RandomPacket();
        }

        [Test]
        public void BinarySerialization()
        {
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/ipv6_icmpv6_packet.pcap");
            dev.Open();

            RawCapture rawCapture;
            Boolean foundipv6 = false;
            while ((rawCapture = dev.GetNextPacket()) != null)
            {
                Packet p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var ipv6 = (IPv6Packet)p.Extract(typeof(IPv6Packet));
                if (ipv6 == null)
                {
                    continue;
                }
                foundipv6 = true;

                var memoryStream = new MemoryStream();
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(memoryStream, ipv6);

                memoryStream.Seek (0, SeekOrigin.Begin);
                BinaryFormatter deserializer = new BinaryFormatter();
                IPv6Packet fromFile = (IPv6Packet)deserializer.Deserialize(memoryStream);

                Assert.AreEqual(ipv6.Bytes, fromFile.Bytes);
                Assert.AreEqual(ipv6.BytesHighPerformance.Bytes, fromFile.BytesHighPerformance.Bytes);
                Assert.AreEqual(ipv6.BytesHighPerformance.BytesLength, fromFile.BytesHighPerformance.BytesLength);
                Assert.AreEqual(ipv6.BytesHighPerformance.Length, fromFile.BytesHighPerformance.Length);
                Assert.AreEqual(ipv6.BytesHighPerformance.NeedsCopyForActualBytes, fromFile.BytesHighPerformance.NeedsCopyForActualBytes);
                Assert.AreEqual(ipv6.BytesHighPerformance.Offset, fromFile.BytesHighPerformance.Offset);
                Assert.AreEqual(ipv6.Color, fromFile.Color);
                Assert.AreEqual(ipv6.HeaderData, fromFile.HeaderData);
                Assert.AreEqual(ipv6.PayloadData, fromFile.PayloadData);
                Assert.AreEqual(ipv6.DestinationAddress, fromFile.DestinationAddress);
                Assert.AreEqual(ipv6.HeaderLength, fromFile.HeaderLength);
                Assert.AreEqual(ipv6.HopLimit, fromFile.HopLimit);
                Assert.AreEqual(ipv6.NextHeader, fromFile.NextHeader);
                Assert.AreEqual(ipv6.PayloadLength, fromFile.PayloadLength);
                Assert.AreEqual(ipv6.Protocol, fromFile.Protocol);
                Assert.AreEqual(ipv6.SourceAddress, fromFile.SourceAddress);
                Assert.AreEqual(ipv6.TimeToLive, fromFile.TimeToLive);
                Assert.AreEqual(ipv6.TotalLength, fromFile.TotalLength);
                Assert.AreEqual(ipv6.Version, fromFile.Version);
                Assert.AreEqual(ipv6.FlowLabel, fromFile.FlowLabel);
                Assert.AreEqual(ipv6.TrafficClass, fromFile.TrafficClass);

                //Method Invocations to make sure that a deserialized packet does not cause 
                //additional errors.

                ipv6.PrintHex();
                ipv6.UpdateCalculatedValues();
            }

            dev.Close();
            Assert.IsTrue(foundipv6, "Capture file contained no ipv6 packets");
        }

    }
}
