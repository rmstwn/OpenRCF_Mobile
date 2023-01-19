using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using System.Management;

namespace OpenRCF
{
    public static class SerialDevice
    {
        public static void ConsoleWriteDeviceNameList()
        {
            DeviceManagement.ConsoleWriteDeviceNameList();
        }

        private static class DeviceManagement
        {
            private static ArrayList DeviceNameList = new ArrayList();
            private static ManagementClass Mgmt = new ManagementClass("Win32_PnPEntity");
            private static ManagementObjectCollection MgmtObjCol;

            private static List<string> ConnectedPortList = new List<string>();

            public static string SearchPortName(string deviceName)
            {
                DeviceNameList.Clear();
                MgmtObjCol = Mgmt.GetInstances();
                string fullDeviceName;

                foreach (ManagementObject mgmtObj in MgmtObjCol)
                {
                    object nameObject = mgmtObj.GetPropertyValue("Name");

                    if (nameObject != null)
                    {
                        fullDeviceName = nameObject.ToString();

                        if (fullDeviceName.Contains("COM"))
                        {
                            string portName = ExtractPortName(fullDeviceName);

                            if (fullDeviceName.Contains(deviceName) && !ConnectedPortList.Contains(portName))
                            {
                                Console.WriteLine(fullDeviceName);
                                return portName;
                            }
                            else
                            {
                                DeviceNameList.Add(fullDeviceName);
                            }
                        }
                    }
                }

                return null;
            }

            private static string ExtractPortName(string text)
            {
                for (int i = 0; i < text.Length - 3; i++)
                {
                    if (text[i] == 'C' && text[i + 1] == 'O' && text[i + 2] == 'M')
                    {
                        if (i + 4 < text.Length && char.IsNumber(text[i + 4]))
                        {
                            return text.Substring(i, 5);
                        }
                        else if (char.IsNumber(text[i + 3]))
                        {
                            return text.Substring(i, 4);
                        }
                    }
                }

                return "COM";
            }

            public static void ConsoleWriteDeviceNameList()
            {
                MgmtObjCol = Mgmt.GetInstances();
                string fullDeviceName;

                Console.WriteLine("Following serial devices have been detected.");

                foreach (ManagementObject mgmtObj in MgmtObjCol)
                {
                    object nameObject = mgmtObj.GetPropertyValue("Name");

                    if (nameObject != null)
                    {
                        fullDeviceName = nameObject.ToString();

                        if (fullDeviceName.Contains("COM"))
                        {
                            Console.Write("  ");
                            Console.WriteLine(fullDeviceName.ToString());
                        }
                    }
                }

                Console.WriteLine();
            }

        }

        private class Header
        {
            private byte[] bytes = new byte[0];

            public Header(params byte[] header)
            {
                bytes = new byte[header.Length];

                for (int i = 0; i < header.Length; i++)
                {
                    bytes[i] = header[i];
                }
            }

            public byte[] Bytes { get { return bytes; } }

            public byte this[int i] { get { return bytes[i]; } }

            public int Length { get { return bytes.Length; } }

            public bool Contains(byte[] readBytes)
            {
                if (bytes.Length <= readBytes.Length)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] != readBytes[i]) return false;
                    }
                }

                return true;
            }
        }

        private class Packet
        {
            private byte[] bytes = new byte[1024];
            private int length = 0;

            public int Length { get { return length; } }

            public byte this[int i] { get { return bytes[i]; } }

            public short ToInt16(int startIndex)
            {
                return BitConverter.ToInt16(bytes, startIndex);
            }

            public ushort ToUInt16(int startIndex)
            {
                return BitConverter.ToUInt16(bytes, startIndex);
            }

            public int ToInt32(int startIndex)
            {
                return BitConverter.ToInt32(bytes, startIndex);
            }

            public uint ToUInt32(int startIndex)
            {
                return BitConverter.ToUInt32(bytes, startIndex);
            }

            public float ToSingle(int startIndex)
            {
                return BitConverter.ToSingle(bytes, startIndex);
            }

            public void Reset() { length = 0; }

            public byte[] Get
            {
                get
                {
                    byte[] result = new byte[length];

                    for (int i = 0; i < length; i++)
                    {
                        result[i] = bytes[i];
                    }

                    return result;
                }
            }

            public byte[] Stack
            {
                set
                {
                    if (length + value.Length < bytes.Length)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            bytes[length + i] = value[i];
                        }

                        length = length + value.Length;
                    }
                    else
                    {
                        Console.WriteLine("Error : Packet capacity is full.");
                    }
                }
            }

            public void CutOut(int endIndex)
            {
                if (endIndex == length)
                {
                    length = 0;
                }
                else if (endIndex < length)
                {
                    for (int i = 0; i < length - endIndex; i++)
                    {
                        bytes[i] = bytes[endIndex + i];
                    }

                    length = length - endIndex;
                }
                else
                {
                    Console.WriteLine("Error : Index is out of packet.");
                }
            }
        }

        public class Arduino
        {
            private SerialPort SerialPort = new SerialPort();
            private RecieveDataHandler RecieveData = new RecieveDataHandler();
            private static Header Header = new Header(0xFF, 0x7F);

            public Arduino(int baudRate = 9600)
            {
                SetBaudRate(baudRate);
                SerialPort.Handshake = Handshake.None;
                SerialPort.DtrEnable = false;
                SerialPort.RtsEnable = true;
                SerialPort.ReadTimeout = 500;
                SerialPort.WriteTimeout = 500;
                SerialPort.DataReceived += DataReceiveEvent;
            }

            private void DataReceiveEvent(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] readBytes = new byte[SerialPort.BytesToRead];
                SerialPort.Read(readBytes, 0, readBytes.Length);
                RecieveData.Read(readBytes);
                // Console.WriteLine(String.Join(" ", readBytes));
            }

            public short[] RecieveShortArray { get { return RecieveData.ShortArray; } }

            public void SetBaudRate9600() { SerialPort.BaudRate = 9600; }

            public void SetBaudRate19200() { SerialPort.BaudRate = 19200; }

            public void SetBaudRate38400() { SerialPort.BaudRate = 38400; }

            public void SetBaudRate115200() { SerialPort.BaudRate = 115200; }

            public void SetBaudRate(int baudRate)
            {
                if (0 < baudRate) SerialPort.BaudRate = baudRate;
                else Console.WriteLine("Error : Baud rate is negative.");
            }

            public void PortOpen(string deviceName)
            {
                if (SerialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is already open.");
                }
                else
                {
                    string portName = DeviceManagement.SearchPortName(deviceName);

                    if (portName != null)
                    {
                        SerialPort.PortName = portName;

                        try
                        {
                            Console.WriteLine("Connecting...");
                            SerialPort.Open();
                            if (SerialPort.DtrEnable) Thread.Sleep(1000);
                            SerialPort.DiscardInBuffer();
                            SerialPort.DiscardOutBuffer();
                            Console.WriteLine("Serial port is open.");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to open serial port.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} could not be found.", deviceName);
                        ConsoleWriteDeviceNameList();
                    }
                }
            }

            public void PortClose()
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                    Console.WriteLine("Serial port is closed.");
                }
                else
                {
                    Console.WriteLine("Serial port is already closed.");
                }
            }

            public void ConsoleWriteReceiveData()
            {
                Console.WriteLine(String.Join(" ", RecieveShortArray));
            }

            private Packet SendPacket = new Packet();

            public void Send(params short[] sendData)
            {
                if (SerialPort.IsOpen)
                {
                    if (sendData.Length < 125)
                    {
                        SendPacket.Reset();
                        SendPacket.Stack = Header.Bytes;
                        SendPacket.Stack = new byte[1] { (byte)(Header.Length + 2 * sendData.Length + 2) };

                        for (int i = 0; i < sendData.Length; i++)
                        {
                            SendPacket.Stack = BitConverter.GetBytes(sendData[i]);
                        }

                        byte bcc = 0;

                        for (int i = Header.Length; i < SendPacket.Length; i++)
                        {
                            bcc ^= SendPacket[i];
                        }

                        bcc = (byte)(bcc & 0xFF);

                        SendPacket.Stack = new byte[1] { bcc };
                        SerialPort.Write(SendPacket.Get, 0, SendPacket.Length);
                    }
                    else
                    {
                        Console.WriteLine("Error : Length of send data is too large.");
                    }
                }
                else
                {
                    Console.WriteLine("Serial port is not open.");
                }
            }

            private class RecieveDataHandler
            {
                private short[] shortArray = new short[128];
                private int arrayLength;
                private Packet ReadPacket = new Packet();

                public short[] ShortArray
                {
                    get
                    {
                        short[] result = new short[arrayLength];

                        for (int i = 0; i < result.Length; i++)
                        {
                            result[i] = shortArray[i];
                        }

                        return result;
                    }
                }

                public void Read(byte[] readBytes)
                {
                    ReadPacket.Stack = readBytes;

                    while (Header.Length < ReadPacket.Length)
                    {
                        if (Header.Contains(ReadPacket.Get))
                        {
                            byte length = ReadPacket[Header.Length];

                            if (ReadPacket.Length < length) return;

                            byte bcc = 0;

                            for (int i = Header.Length; i < length - 1; i++)
                            {
                                bcc ^= ReadPacket[i];
                            }

                            bcc = (byte)(bcc & 0xFF);

                            if (bcc == ReadPacket[length - 1])
                            {
                                int i = 0;

                                for (int j = Header.Length + 1; j < length - 1; j += 2)
                                {
                                    shortArray[i] = ReadPacket.ToInt16(j);
                                    i++;
                                }

                                arrayLength = i;
                                ReadPacket.CutOut(length);
                            }
                            else
                            {
                                Console.WriteLine("Error : Checksum (LRC) does not match.");
                                ReadPacket.Reset();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error : Header does not match.");
                            ReadPacket.Reset();
                        }
                    }
                }

            }

        }

        public class Dynamixel
        {
            private SerialPort SerialPort = new SerialPort();
            private RecieveDataHandler RecieveData = new RecieveDataHandler();
            private Buffer[] SendBuffer = new Buffer[10];
            private static Header Header = new Header(0xFF, 0xFF, 0xFD, 0x00);

            public Dynamixel(int baudRate = 57600)
            {
                SetBaudRate(baudRate);
                SerialPort.Handshake = Handshake.None;
                SerialPort.DtrEnable = false;
                SerialPort.RtsEnable = true;
                SerialPort.ReadTimeout = 500;
                SerialPort.WriteTimeout = 500;
                SerialPort.DataReceived += DataReceiveEvent;

                for (int i = 0; i < SendBuffer.Length; i++)
                {
                    SendBuffer[i] = new Buffer();
                }
            }

            private void DataReceiveEvent(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] readBytes = new byte[SerialPort.BytesToRead];
                SerialPort.Read(readBytes, 0, readBytes.Length);
                RecieveData.Read(readBytes);
            }

            public int Position(byte id) { return RecieveData.Servo[id].Position; }
            public int Velocity(byte id) { return RecieveData.Servo[id].Velocity; }
            public int Current(byte id) { return RecieveData.Servo[id].Current; }
            public int Temperature(byte id) { return RecieveData.Servo[id].Temperature; }

            public int[] Position(params byte[] id)
            {
                int[] result = new int[id.Length];
                for (int i = 0; i < id.Length; i++) result[i] = RecieveData.Servo[id[i]].Position;
                return result;
            }

            public int[] Velocity(params byte[] id)
            {
                int[] result = new int[id.Length];
                for (int i = 0; i < id.Length; i++) result[i] = RecieveData.Servo[id[i]].Velocity;
                return result;
            }

            public int[] Current(params byte[] id)
            {
                int[] result = new int[id.Length];
                for (int i = 0; i < id.Length; i++) result[i] = RecieveData.Servo[id[i]].Current;
                return result;
            }

            public int[] Temperature(params byte[] id)
            {
                int[] result = new int[id.Length];
                for (int i = 0; i < id.Length; i++) result[i] = RecieveData.Servo[id[i]].Temperature;
                return result;
            }

            public void SetBaudRate57600() { SerialPort.BaudRate = 57600; }

            public void SetBaudRate115200() { SerialPort.BaudRate = 115200; }

            public void SetBaudRate1000000() { SerialPort.BaudRate = 1000000; }

            public void SetBaudRate2000000() { SerialPort.BaudRate = 2000000; }

            public void SetBaudRate3000000() { SerialPort.BaudRate = 3000000; }

            public void SetBaudRate(int baudRate)
            {
                if (0 < baudRate) SerialPort.BaudRate = baudRate;
                else Console.WriteLine("Error : Baud rate is negative.");
            }

            public void PortOpen(string deviceName)
            {
                if (SerialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is already open.");
                }
                else
                {
                    string portName = DeviceManagement.SearchPortName(deviceName);

                    if (portName != null)
                    {
                        SerialPort.PortName = portName;

                        try
                        {
                            Console.WriteLine("Connecting...");
                            SerialPort.Open();
                            if (SerialPort.DtrEnable) Thread.Sleep(1000);
                            SerialPort.DiscardInBuffer();
                            SerialPort.DiscardOutBuffer();
                            Task.Run(SerialWriteLoop);
                            Console.WriteLine("Serial port is open.");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to open serial port.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} could not be found.", deviceName);
                        DeviceManagement.ConsoleWriteDeviceNameList();
                    }
                }
            }

            public void PortClose()
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                    Console.WriteLine("Serial port is closed.");
                }
                else
                {
                    Console.WriteLine("Serial port is already closed.");
                }
            }

            public void ConsoleWriteReceiveData(params byte[] id)
            {
                for (int i = 0; i < id.Length; i++)
                {
                    Console.WriteLine("ID:{0}, Position:{1}, Velocity:{2}, Current:{3}", id[i], Position(id[i]), Velocity(id[i]), Current(id[i]));
                }
            }

            private void SendData(byte id, byte[] sendData, byte instruction)
            {
                if (SerialPort.IsOpen)
                {
                    byte[] sendBytes = new byte[sendData.Length + 10];

                    sendBytes[0] = Header[0];
                    sendBytes[1] = Header[1];
                    sendBytes[2] = Header[2];
                    sendBytes[3] = Header[3];
                    sendBytes[4] = id;
                    short length = (short)(sendData.Length + 3);
                    sendBytes[5] = BitConverter.GetBytes(length)[0];
                    sendBytes[6] = BitConverter.GetBytes(length)[1];
                    sendBytes[7] = instruction;

                    for (int i = 0; i < sendData.Length; i++)
                    {
                        sendBytes[8 + i] = sendData[i];
                    }

                    byte[] checksum = BitConverter.GetBytes(CRC16.Checksum(sendBytes, length + 5));
                    sendBytes[length + 5] = checksum[0];
                    sendBytes[length + 6] = checksum[1];

                    AddSerialWriteBuffer(sendBytes, length + 7);
                }
                else
                {
                    Console.WriteLine("Serial port is not open.");
                }
            }

            private static class CRC16
            {
                public static ushort InitialValue = 0;

                // polynomial : 0x8005
                private static ushort[] table = new ushort[256] {
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011,
                    0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022,
                    0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072,
                    0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041,
                    0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2,
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1,
                    0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1,
                    0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082,
                    0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192,
                    0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1,
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1,
                    0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2,
                    0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151,
                    0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162,
                    0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101,
                    0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312,
                    0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321,
                    0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371,
                    0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342,
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1,
                    0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2,
                    0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2,
                    0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381,
                    0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291,
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2,
                    0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2,
                    0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1,
                    0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252,
                    0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261,
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231,
                    0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202
                };

                public static ushort Checksum(byte[] bytes, int length)
                {
                    ushort result = InitialValue;
                    int i, j;

                    for (j = 0; j < length; j++)
                    {
                        i = ((result >> 8) ^ bytes[j]) & 0xFF;
                        result = (ushort)((result << 8) ^ table[i]);
                    }

                    return result;
                }
            }

            private static class Instruction
            {
                public readonly static byte Ping = 0x01;
                public readonly static byte Read = 0x02;
                public readonly static byte Write = 0x03;
                public readonly static byte RegWrite = 0x04;
                public readonly static byte Action = 0x05;
                public readonly static byte FactoryReset = 0x06;
                public readonly static byte Reboot = 0x08;
                public readonly static byte SyncRead = 0x82;
                public readonly static byte SyncWrite = 0x83;
                public readonly static byte BulkRead = 0x92;
                public readonly static byte BulkWrite = 0x93;
                public readonly static byte Status = 0x55;
            }

            private static class Address
            {
                public readonly static byte[] ID = new byte[2] { 7, 0 };
                public readonly static byte[] BaudRate = new byte[2] { 8, 0 };
                public readonly static byte[] ControlMode = new byte[2] { 11, 0 };
                public readonly static byte[] TorqueOnOff = new byte[2] { 64, 0 };
                public readonly static byte[] WriteCurrent = new byte[2] { 102, 0 };
                public readonly static byte[] WriteVelocity = new byte[2] { 104, 0 };
                public readonly static byte[] WritePosition = new byte[2] { 116, 0 };
                public readonly static byte[] ReadCurrent = new byte[2] { 126, 0 };
                public readonly static byte[] ReadVelocity = new byte[2] { 128, 0 };
                public readonly static byte[] ReadPosition = new byte[2] { 132, 0 };
                public readonly static byte[] ReadTemperature = new byte[2] { 146, 0 };
            }

            public void TorqueEnable(params byte[] id)
            {
                byte[] param = new byte[4 + 2 * id.Length];

                param[0] = Address.TorqueOnOff[0];
                param[1] = Address.TorqueOnOff[1];
                param[2] = 0x01;
                param[3] = 0x00;

                for (int i = 0; i < id.Length; i++)
                {
                    param[4 + 2 * i] = id[i];
                    param[5 + 2 * i] = 0x01;
                }

                SendData(254, param, Instruction.SyncWrite);
            }

            public void TorqueDisable(params byte[] id)
            {
                byte[] param = new byte[4 + 2 * id.Length];

                param[0] = Address.TorqueOnOff[0];
                param[1] = Address.TorqueOnOff[1];
                param[2] = 0x01;
                param[3] = 0x00;

                for (int i = 0; i < id.Length; i++)
                {
                    param[4 + 2 * i] = id[i];
                    param[5 + 2 * i] = 0x00;
                }

                SendData(254, param, Instruction.SyncWrite);
            }

            public void ChangeID(byte id, byte newID)
            {
                byte[] param = new byte[3];
                param[0] = Address.ID[0];
                param[1] = Address.ID[1];
                param[2] = newID;
                SendData(id, param, Instruction.Write);
            }

            public void ChangeBaudRate57600(params byte[] id)
            {
                ChangeBaudRate(id, 0x01);
                SetBaudRate57600();
            }

            public void ChangeBaudRate115200(params byte[] id)
            {
                ChangeBaudRate(id, 0x02);
                SetBaudRate115200();
            }

            public void ChangeBaudRate1000000(params byte[] id)
            {
                ChangeBaudRate(id, 0x03);
                SetBaudRate1000000();
            }

            public void ChangeBaudRate2000000(params byte[] id)
            {
                ChangeBaudRate(id, 0x04);
                SetBaudRate2000000();
            }

            public void ChangeBaudRate3000000(params byte[] id)
            {
                ChangeBaudRate(id, 0x05);
                SetBaudRate3000000();
            }

            private void ChangeBaudRate(byte[] id, byte value)
            {
                byte[] param = new byte[4 + 2 * id.Length];

                param[0] = Address.BaudRate[0];
                param[1] = Address.BaudRate[1];
                param[2] = 0x01;
                param[3] = 0x00;

                for (int i = 0; i < id.Length; i++)
                {
                    param[4 + 2 * i] = id[i];
                    param[5 + 2 * i] = value;
                }

                SendData(254, param, Instruction.SyncWrite);
            }

            public void Reboot(byte id)
            {
                SendData(id, new byte[0], Instruction.Reboot);
            }

            public void FactoryReset(byte id)
            {
                SendData(id, new byte[1] { 0xFF }, Instruction.FactoryReset);
            }

            public void FactoryResetExceptID(byte id)
            {
                SendData(id, new byte[1] { 0x01 }, Instruction.FactoryReset);
            }

            public void FactoryResetExceptBaudrateAndID(byte id)
            {
                SendData(id, new byte[1] { 0x02 }, Instruction.FactoryReset);
            }

            public void CurrentControlMode(params byte[] id)
            {
                ChangeControlMode(id, 0x00);
            }

            public void VelocityControlMode(params byte[] id)
            {
                ChangeControlMode(id, 0x01);
            }

            public void PositionControlMode(params byte[] id)
            {
                ChangeControlMode(id, 0x03);
            }

            public void ExtendedPositionControlMode(params byte[] id)
            {
                ChangeControlMode(id, 0x04);
            }

            public void CurrentBasedPositionControlMode(params byte[] id)
            {
                ChangeControlMode(id, 0x05);
            }

            private void ChangeControlMode(byte[] id, byte modeNo)
            {
                TorqueDisable(id);

                byte[] param = new byte[4 + 2 * id.Length];

                param[0] = Address.ControlMode[0];
                param[1] = Address.ControlMode[1];
                param[2] = 0x01;
                param[3] = 0x00;

                for (int i = 0; i < id.Length; i++)
                {
                    param[4 + 2 * i] = id[i];
                    param[5 + 2 * i] = modeNo;
                }

                SendData(254, param, Instruction.SyncWrite);
            }

            public void WritePosition(byte[] id, int[] angle)
            {
                if (id.Length == angle.Length)
                {
                    byte[] param = new byte[4 + 5 * id.Length];

                    param[0] = Address.WritePosition[0];
                    param[1] = Address.WritePosition[1];
                    param[2] = 0x04;
                    param[3] = 0x00;

                    byte[] angleBytes;

                    for (int i = 0; i < id.Length; i++)
                    {
                        angleBytes = BitConverter.GetBytes(angle[i]);
                        param[4 + 5 * i] = id[i];
                        param[5 + 5 * i] = angleBytes[0];
                        param[6 + 5 * i] = angleBytes[1];
                        param[7 + 5 * i] = angleBytes[2];
                        param[8 + 5 * i] = angleBytes[3];
                    }

                    SendData(254, param, Instruction.SyncWrite);
                }
                else
                {
                    Console.WriteLine("Error : Lengths of id[] and angle[] are not equal.");
                }
            }

            public void WritePosition(byte id, int angle)
            {
                WritePosition(new byte[1] { id }, new int[1] { angle });
            }

            public void WriteVelocity(byte[] id, int[] velocity)
            {
                if (id.Length == velocity.Length)
                {
                    byte[] param = new byte[4 + 5 * id.Length];

                    param[0] = Address.WriteVelocity[0];
                    param[1] = Address.WriteVelocity[1];
                    param[2] = 0x04;
                    param[3] = 0x00;

                    byte[] velocityBytes;

                    for (int i = 0; i < id.Length; i++)
                    {
                        velocityBytes = BitConverter.GetBytes(velocity[i]);
                        param[4 + 5 * i] = id[i];
                        param[5 + 5 * i] = velocityBytes[0];
                        param[6 + 5 * i] = velocityBytes[1];
                        param[7 + 5 * i] = velocityBytes[2];
                        param[8 + 5 * i] = velocityBytes[3];
                    }

                    SendData(254, param, Instruction.SyncWrite);
                }
                else
                {
                    Console.WriteLine("Error : Lengths of id[] and velocity[] are not equal.");
                }
            }

            public void WriteVelocity(byte id, int velocity)
            {
                WriteVelocity(new byte[1] { id }, new int[1] { velocity });
            }

            public void WriteCurrent(byte[] id, int[] current)
            {
                if (id.Length == current.Length)
                {
                    byte[] param = new byte[4 + 3 * id.Length];

                    param[0] = Address.WriteCurrent[0];
                    param[1] = Address.WriteCurrent[1];
                    param[2] = 0x02;
                    param[3] = 0x00;

                    for (int i = 0; i < id.Length; i++)
                    {
                        param[4 + 3 * i] = id[i];
                        param[5 + 3 * i] = BitConverter.GetBytes(current[i])[0];
                        param[6 + 3 * i] = BitConverter.GetBytes(current[i])[1];
                    }

                    SendData(254, param, Instruction.SyncWrite);
                }
                else
                {
                    Console.WriteLine("Error : Lengths of id[] and current[] are not equal.");
                }
            }

            public void WriteCurrent(byte id, int current)
            {
                WriteCurrent(new byte[1] { id }, new int[1] { current });
            }

            public void RequestPositionReply(params byte[] id)
            {
                RequestReply(id, Address.ReadPosition, 0x04);
            }

            public void RequestVelocityReply(params byte[] id)
            {
                RequestReply(id, Address.ReadVelocity, 0x03);
            }

            public void RequestCurrentReply(params byte[] id)
            {
                RequestReply(id, Address.ReadCurrent, 0x02);
            }

            public void RequestTemperatureReply(params byte[] id)
            {
                RequestReply(id, Address.ReadTemperature, 0x01);
            }

            private void RequestReply(byte[] id, byte[] address, byte size)
            {
                byte[] param = new byte[4 + id.Length];

                param[0] = address[0];
                param[1] = address[1];
                param[2] = size;
                param[3] = 0x00;

                for (int i = 0; i < id.Length; i++)
                {
                    param[4 + i] = id[i];
                }

                SendData(254, param, Instruction.SyncRead);
            }

            private class Buffer
            {
                public bool isStacked = false;
                private byte[] buffer;

                public bool IsStacked { get { return isStacked; } }

                public void Set(byte[] sendByte, int length)
                {
                    buffer = new byte[length];

                    for (int i = 0; i < length; i++)
                    {
                        buffer[i] = sendByte[i];
                    }

                    isStacked = true;
                }

                public byte[] Get
                {
                    get
                    {
                        isStacked = false;
                        return buffer;
                    }
                }

                public int Length { get { return buffer.Length; } }
            }

            private void AddSerialWriteBuffer(byte[] sendByte, int length)
            {
                for (int i = 0; i < SendBuffer.Length - 1; i++)
                {
                    if (!SendBuffer[i].IsStacked && !SendBuffer[i + 1].IsStacked)
                    {
                        SendBuffer[i].Set(sendByte, length);
                        return;
                    }
                }

                Console.WriteLine("Error : SerialWriteBuffer is full.");
            }

            private void SerialWriteLoop()
            {
                while (SerialPort.IsOpen)
                {
                    if (SendBuffer[0].IsStacked)
                    {
                        SerialPort.Write(SendBuffer[0].Get, 0, SendBuffer[0].Length);
                    }
              
                    for (int i = 1; i < SendBuffer.Length; i++)
                    {
                        if (!SendBuffer[i - 1].IsStacked && SendBuffer[i].IsStacked)
                        {
                            SendBuffer[i - 1].Set(SendBuffer[i].Get, SendBuffer[i].Length);
                        }
                    }

                    Thread.Sleep(2);
                }
            }

            private class RecieveDataHandler
            {
                public ServoInfo[] Servo = new ServoInfo[255];
                private Packet ReadPacket = new Packet();

                public RecieveDataHandler()
                {
                    for (int i = 0; i < Servo.Length; i++)
                    {
                        Servo[i] = new ServoInfo();
                    }
                }

                public class ServoInfo
                {
                    public int Position;
                    public int Velocity;
                    public int Current;
                    public int Temperature;
                }

                private static string ErrorMessage(int errorCode)
                {
                    string msg;

                    if (errorCode == 1) msg = "Packet Faile";
                    else if (errorCode == 2) msg = "Instruction Error";
                    else if (errorCode == 3) msg = "CRC Error";
                    else if (errorCode == 4) msg = "Data Range Error";
                    else if (errorCode == 5) msg = "Data Length Error";
                    else if (errorCode == 6) msg = "Data Limit Error";
                    else if (errorCode == 7) msg = "Access Error";
                    else msg = "Undefined Error Code";

                    return msg;
                }

                public void Read(byte[] readBytes)
                {
                    ReadPacket.Stack = readBytes;

                    while (11 <= ReadPacket.Length)
                    {
                        if (Header.Contains(ReadPacket.Get))
                        {
                            byte id = ReadPacket[Header.Length];
                            ushort length = ReadPacket.ToUInt16(Header.Length + 1);
                            byte errorCode = ReadPacket[Header.Length + 4];

                            if (252 < id || 252 < length)
                            {
                                Console.WriteLine("Error : Received id or length is too large.");
                                ReadPacket.Reset();
                                return;
                            }
                            else if (ReadPacket.Length < length + 7)
                            {
                                return;
                            }

                            ushort checksum = CRC16.Checksum(ReadPacket.Get, length + 5);

                            if (checksum == ReadPacket.ToUInt16(length + 5))
                            {
                                if (errorCode == 0)
                                {
                                    int size = length - 4;
                                    if (size == 4) Servo[id].Position = ReadPacket.ToInt16(Header.Length + 5);
                                    else if (size == 3) Servo[id].Velocity = ReadPacket.ToInt16(Header.Length + 5);
                                    else if (size == 2) Servo[id].Current = ReadPacket.ToInt16(Header.Length + 5);
                                    else if (size == 1) Servo[id].Temperature = ReadPacket[Header.Length + 5];
                                }
                                else
                                {
                                    Console.WriteLine("Error : " + ErrorMessage(errorCode));
                                }

                                ReadPacket.CutOut(length + 7);
                            }
                            else
                            {
                                Console.WriteLine("Error : Checksum (CRC) does not match.");
                                ReadPacket.Reset();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error : Header does not match.");
                            ReadPacket.Reset();
                        }
                    }
                }
            }

        }

        public class Leptrino
        {
            private SerialPort SerialPort = new SerialPort();
            private RecieveDataHandler RecieveData = new RecieveDataHandler();

            public Leptrino(int baudRate = 460800)
            {
                SetBaudRate(baudRate);
                SerialPort.Handshake = Handshake.None;
                SerialPort.DtrEnable = false;
                SerialPort.RtsEnable = true;
                SerialPort.ReadTimeout = 500;
                SerialPort.WriteTimeout = 500;
                SerialPort.DataReceived += DataReceiveEvent;
            }

            private void DataReceiveEvent(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] readBytes = new byte[SerialPort.BytesToRead];
                SerialPort.Read(readBytes, 0, readBytes.Length);
                RecieveData.Read(readBytes);
                // Console.WriteLine(String.Join(" ", readBytes));
            }

            public short[] Force { get { return RecieveData.Force; } }
            public short Fx { get { return RecieveData.Force[0]; } }
            public short Fy { get { return RecieveData.Force[1]; } }
            public short Fz { get { return RecieveData.Force[2]; } }
            public short Mx { get { return RecieveData.Force[3]; } }
            public short My { get { return RecieveData.Force[4]; } }
            public short Mz { get { return RecieveData.Force[5]; } }

            public float[] ForceLimit { get { return RecieveData.ForceLimit; } }
            public float FxLimit { get { return RecieveData.ForceLimit[0]; } }
            public float FyLimit { get { return RecieveData.ForceLimit[1]; } }
            public float FzLimit { get { return RecieveData.ForceLimit[2]; } }
            public float MxLimit { get { return RecieveData.ForceLimit[3]; } }
            public float MyLimit { get { return RecieveData.ForceLimit[4]; } }
            public float MzLimit { get { return RecieveData.ForceLimit[5]; } }

            public string ModelName { get { return Encoding.ASCII.GetString(RecieveData.ModelName); } }
            public string SerialNo { get { return Encoding.ASCII.GetString(RecieveData.SerialNo); } }
            public string FirmVersion { get { return Encoding.ASCII.GetString(RecieveData.FirmVersion); } }
            public string OutputRate { get { return Encoding.ASCII.GetString(RecieveData.OutputRate); } }

            public void SetBaudRate38400() { SerialPort.BaudRate = 38400; }

            public void SetBaudRate115200() { SerialPort.BaudRate = 115200; }

            public void SetBaudRate230400() { SerialPort.BaudRate = 230400; }

            public void SetBaudRate460800() { SerialPort.BaudRate = 460800; }

            public void SetBaudRate921600() { SerialPort.BaudRate = 921600; }

            public void SetBaudRate(int baudRate)
            {
                if (0 < baudRate) SerialPort.BaudRate = baudRate;
                else Console.WriteLine("Error : Baud rate is negative.");
            }

            public void PortOpen(string deviceName)
            {
                if (SerialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is already open.");
                }
                else
                {
                    string portName = DeviceManagement.SearchPortName(deviceName);

                    if (portName != null)
                    {
                        SerialPort.PortName = portName;

                        try
                        {
                            Console.WriteLine("Connecting...");
                            SerialPort.Open();
                            if (SerialPort.DtrEnable) Thread.Sleep(1000);
                            SerialPort.DiscardInBuffer();
                            SerialPort.DiscardOutBuffer();
                            Console.WriteLine("Serial port is open.");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to open serial port.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} could not be found.", deviceName);
                        DeviceManagement.ConsoleWriteDeviceNameList();
                    }
                }
            }

            public void PortClose()
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                    Console.WriteLine("Serial port is closed.");
                }
                else
                {
                    Console.WriteLine("Serial port is already closed.");
                }
            }

            public void ConsoleWriteForce()
            {
                Console.WriteLine(String.Join(" ", Force));
            }

            public void ConsoleWriteForceLimit()
            {
                Console.WriteLine(String.Join(" ", ForceLimit));
            }

            public void ConsoleWriteProductInfo()
            {
                Console.WriteLine("ModelName : {0}", ModelName);
                Console.WriteLine("SerialNo : {0}", SerialNo);
                Console.WriteLine("FirmVersion : {0}", FirmVersion);
                Console.WriteLine("OutputRate : {0}", OutputRate);
            }

            private static Header Header = new Header(0x10, 0x02);
            private static readonly byte[] Footer = new byte[2] { 0x10, 0x03 };

            private static class Kind
            {
                public readonly static byte ProductInfo = 0x2A;
                public readonly static byte ForceLimit = 0x2B;
                public readonly static byte Force = 0x30;
                public readonly static byte ContinuousMode = 0x32;
                public readonly static byte StopContinuousMode = 0x33;
            }

            private void SendData(byte[] sendData)
            {
                if (SerialPort.IsOpen)
                {
                    byte[] sendBytes = new byte[sendData.Length + 6];
                    sendBytes[0] = Header[0];
                    sendBytes[1] = Header[1];
                    byte length = (byte)(sendData.Length + 1);
                    sendBytes[2] = length;

                    for (int i = 0; i < sendData.Length; i++)
                    {
                        sendBytes[3 + i] = sendData[i];
                    }

                    byte bcc = 0;

                    for (int i = Header.Length; i < Header.Length + length; i++)
                    {
                        bcc ^= sendBytes[i];
                    }

                    sendBytes[Header.Length + length] = Footer[0];
                    sendBytes[Header.Length + length + 1] = Footer[1];
                    bcc ^= Footer[1];
                    sendBytes[Header.Length + length + 2] = (byte)(bcc & 0xFF);

                    SerialPort.Write(sendBytes, 0, sendBytes.Length);
                }
                else
                {
                    Console.WriteLine("Serial port is not open.");
                }
            }

            public void RequestProductInfoReply()
            {
                SendData(new byte[3] { 0xFF, Kind.ProductInfo, 0x00 });
            }

            public void RequestForceLimitReply()
            {
                SendData(new byte[3] { 0xFF, Kind.ForceLimit, 0x00 });
            }

            public void RequestForceReply()
            {
                SendData(new byte[3] { 0xFF, Kind.Force, 0x00 });
            }

            public void ContinuousMode()
            {
                SendData(new byte[3] { 0xFF, Kind.ContinuousMode, 0x00 });
            }

            public void StopContinuousMode()
            {
                SendData(new byte[3] { 0xFF, Kind.StopContinuousMode, 0x00 });
            }

            private static string ErrorMessage(byte errorCode)
            {
                string msg;

                if (errorCode == 0x01) msg = "Data Length does not match.";
                else if (errorCode == 0x02) msg = "Undefined command has been received.";
                else if (errorCode == 0x04) msg = "Abnormal state.";
                else msg = "Undefined Error Code";

                return msg;
            }

            private class RecieveDataHandler
            {
                public short[] Force = new short[6];
                public float[] ForceLimit = new float[6];
                public byte[] ModelName = new byte[16];
                public byte[] SerialNo = new byte[8];
                public byte[] FirmVersion = new byte[4];
                public byte[] OutputRate = new byte[6];

                private Packet ReadPacket = new Packet();

                public void Read(byte[] readBytes)
                {
                    ReadPacket.Stack = readBytes;

                    while (Header.Length < ReadPacket.Length)
                    {
                        if (Header.Contains(ReadPacket.Get))
                        {
                            byte length = ReadPacket[Header.Length];

                            if (ReadPacket.Length < length + 5) return;

                            byte reserve = ReadPacket[Header.Length + 1];
                            byte kind = ReadPacket[Header.Length + 2];
                            byte errorCode = ReadPacket[Header.Length + 3];
                            int index = Header.Length + 4;

                            if (reserve == 0xFF && errorCode == 0)
                            {
                                if (kind == Kind.Force)
                                {
                                    for (int i = 0; i < 6; i++, index += 2)
                                    {
                                        if (ReadPacket[index] == 0x10) index++;
                                        Force[i] = ReadPacket.ToInt16(index);
                                    }
                                }
                                else if (kind == Kind.ForceLimit)
                                {
                                    for (int i = 0; i < 6; i++, index += 4)
                                    {
                                        if (ReadPacket[index] == 0x10) index++;
                                        ForceLimit[i] = ReadPacket.ToSingle(index);
                                    }
                                }
                                else if (kind == Kind.ContinuousMode)
                                {
                                    if (25 <= length + 5)
                                    {
                                        for (int i = 0; i < 6; i++, index += 2)
                                        {
                                            if (ReadPacket[index] == 0x10) index++;
                                            Force[i] = ReadPacket.ToInt16(index);
                                        }
                                    }
                                }
                                else if (kind == Kind.ProductInfo)
                                {
                                    byte[] tmp = ModelName;

                                    for (int s = 0; s < 4; s++)
                                    {
                                        if (s == 0) tmp = ModelName;
                                        else if (s == 1) tmp = SerialNo;
                                        else if (s == 2) tmp = FirmVersion;
                                        else if (s == 3) tmp = OutputRate;

                                        for (int i = 0; i < tmp.Length; i++, index++)
                                        {
                                            tmp[i] = ReadPacket[index];
                                        }
                                    }
                                }

                                ReadPacket.CutOut(length + 5);
                            }
                            else if (0 < errorCode)
                            {
                                Console.WriteLine("Error : " + ErrorMessage(errorCode));
                                ReadPacket.Reset();
                            }
                            else
                            {
                                Console.WriteLine("Error : Reserve byte is not 0xFF.");
                                ReadPacket.Reset();
                            }
                        }
                        else
                        {
                            ReadPacket.CutOut(1);

                            while (true)
                            {
                                if (Header.Length < ReadPacket.Length)
                                {
                                    if (Header.Contains(ReadPacket.Get)) break;
                                    else ReadPacket.CutOut(1);
                                }
                                else
                                {
                                    Console.WriteLine("Error : Header does not match.");
                                    ReadPacket.Reset();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

        }
   
    }

}
