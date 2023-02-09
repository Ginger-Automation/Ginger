#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GingerCore.Drivers.CommunicationProtocol
{
    // Prep for better and faster communication Protocol between platforms: C#, JS, Java   
    // We will have the same pack/unpack - C#, JS, Java
    // Payload is the data we want to pass between 2 end points
    // Can be from Ginger to GTB
    // Can be from GTB to HTML
    // first impl for Ginger and JavaDriver
    // Payload Will be used for send commands and response result
    // This is the C# Pack/Parser
    // For JS there will be one too
    // For Java there is one in the JavaDriver Payload.java.
    // all need to be in sync to work and pass data
    // See Unit test for samples

    // Add item is for adding data, can be any type
    // field must be packed and unpacked in the same order
    // This class must be super fast as will be used a lot!! - so not generating objects or doing XMLs processing

    // Payload Template structure
    // Sample - | are not part of the packet
    // Packet contains 1 int value=5 and String="ABC"
    // |  0123      4 5678 9 10 - 16|17   
    // |  0013d    |2|0005|1|0003ABC|&

    // Payload - first 4 bytes is len, then data, then last byte=255 for integrity checks
    // We do value type so we can do dump of package to see what exist

    public class PayLoad
    {
        //UTF8 for regular string which are not created by the user and not language or special chars needed
        public static System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

        //UTF16 for String which are created by the user and might have language or special chars
        public static System.Text.UnicodeEncoding UTF16 = new System.Text.UnicodeEncoding();
        
        //Const - Data Type - one byte before each type in package
        const byte StringType = 1;    // string
        const byte IntType = 2;       // int
        const byte EnumValueType = 3;   // Any Enum value  - will write the ToString()
        const byte StringUTF16Type = 4;  // string - which contains unicode chars - UTF16
        const byte ListStringType = 5;   // List<string>?
        const byte ListPayLoadType = 6;    // List<PayLoad>
        const byte BytesType = 7;    // Byte[]
        const byte KeyValuePair = 8;
        
        // Last char is 255 - looks like space but is not and marking end of packet
        const byte LastByteMarker = 255;

        const int cNULLStringLen = -1;    // if the string we write is null we write len = -1 - save space and parsing time

        byte[] mBuffer = new byte[1024];  // strat with initial buffer of 1024, will grow if needed
        int mBufferIndex = 4; // We strat to write data at position 4, the first 4 bytes will be the data length

        public string Name {get; set;}

        public static string PAYLOAD_PARSING_ERROR = "List PayLoad Parsing Error/wrong value type";

        /// Create new empty Payload with name 
        public PayLoad(string Name)
        {
            this.Name = Name;
            WriteString(Name);
        }
        
        /// Create Payload from Bytes
        public PayLoad(byte[] bytes)
        {
            //TODO: check if we need to do memcopy, to dup and not use the original, since we do not change packets it should be OK.
            mBuffer = bytes;
            Name = ReadString();

            //Verify integrity
            byte b = bytes[bytes.Length - 1];
            if ( b!= LastByteMarker)
            {
                throw new Exception("PayLoad Integrity Error - last byte != 255");
            }
        }

        public string GetHexString()
        {
            //mBuffer to hex string write code here
            StringBuilder hex = new StringBuilder(mBuffer.Length * 2);

            foreach (byte b in mBuffer)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public byte[] GetPackage()
        {
            return mBuffer;
        }

        public void ClosePackage()
        {
            // Each packaet start with 4 bytes length
            // then each type: 1 = String, 2 = Int, 3=Enum
            // data                        
            CheckBuffer(1);
            mBuffer[mBufferIndex] = LastByteMarker;
            mBufferIndex++;

            SetDataLen(mBufferIndex-4);   //-4 since len is not included
                                          // TODO: find a way instead of copy to return subset of buffer          

            Array.Resize(ref mBuffer, mBufferIndex); // Cut the extra unused buffer        

            //We set the buffer index to start of the data. 
            //Because if doing PL2 = PL1 then buffer index is already at end
            ResetBufferIndex();
        }

        private void ResetBufferIndex()
        {
            mBufferIndex = 4;
            int len = ReadInt();
            mBufferIndex += len;
        }

        private void SetDataLen(int Len)
        {
            mBuffer[0] = (byte)(Len >> 24);
            mBuffer[1] = (byte)(Len >> 16);
            mBuffer[2] = (byte)(Len >> 8);
            mBuffer[3] = (byte)Len;            
        }

        private int GetDataLen()
        {
            int Len = ((mBuffer[0]) << 24) + (mBuffer[1] << 16) + (mBuffer[2] << 8) + mBuffer[3];
            return Len;
        }

        /// <summary>
        /// Use Write String for regular UTF8 String
        /// For Special Chars and String use WriteUnicodeString
        /// </summary>
        /// <param name="val"></param>
        private void WriteString(string val)
        {
            // String is combined of length = dynamic, then the value
            if (val != null)
            {
                WriteInt(val.Length);
                UTF8.GetBytes(val, 0, val.Length, mBuffer, mBufferIndex);
                mBufferIndex += val.Length;
            }
            else
            {
                WriteInt(cNULLStringLen);                               
            }
        }
        private void WriteUnicodeString(string val)
        {
            if (val != null)
            {
                int len = UTF16.GetBytes(val, 0, val.Length, mBuffer, mBufferIndex + 4);
                WriteInt(len);
                mBufferIndex += len;
            }
            else
            {
                WriteInt(cNULLStringLen);
            }
        }

        public bool isNonAsciiString(String s)
        {
            bool flag = false;
            //TODO: find the most efficient way to do this.
            if (s != null)
            {
                if (Encoding.UTF8.GetByteCount(s) != s.Length)
                {
                    flag = true;
                }
            }
            return flag;
        }
        private string ReadString()
        {
            int len = ReadInt();
            if (len != cNULLStringLen)
            {
                String s = UTF8.GetString(mBuffer, mBufferIndex, len);
                mBufferIndex += len;
                return s;
            }
            else
            {
                return null;
            }     
        }

        private KeyValuePair<string, string> ReadKeyValuePair()
        {
            byte byteValue = ReadValueType();
            string key = ReadString();

            byteValue = ReadValueType();
            string value = ReadString();

            return new KeyValuePair<string, string>(key, value); 
        }
        private String ReadUnicodeString()
        {
            int len = ReadInt();
            String s = UTF16.GetString(mBuffer,mBufferIndex,len);
            mBufferIndex += len;
            return s;            
        }

        private void WriteInt(int val)
        {
            mBuffer[mBufferIndex] = (byte)(val >> 24);
            mBufferIndex++;
            mBuffer[mBufferIndex] = (byte)(val >> 16);
            mBufferIndex++;
            mBuffer[mBufferIndex] = (byte)(val >> 8);
            mBufferIndex++;
            mBuffer[mBufferIndex] = (byte)val;
            mBufferIndex++;
        }

        private int ReadInt()
        {
            int val = ((mBuffer[mBufferIndex]) << 24) + (mBuffer[mBufferIndex + 1] << 16) + (mBuffer[mBufferIndex + 2] << 8) + mBuffer[mBufferIndex +3];            
            mBufferIndex += 4;
            return val;          
        }

        /// <summary>
        /// Check that we have enough space to add Len bytes of data, if not space will be added, so it make sure data will be added with no err
        /// </summary>
        /// <param name="Len">Length of data to be added</param>
        private void CheckBuffer(int Len)
        {
            if (mBufferIndex + Len > mBuffer.Length)
            {
                int SpaceToAdd = 1024;
                if (Len > SpaceToAdd) 
                {
                    SpaceToAdd = Len + 1024;  // Make sure that we add enough space to hold the new data
                }
                
                Array.Resize(ref mBuffer, mBuffer.Length + SpaceToAdd); // Add more space in chuncks of 1024
            }
        }

        private void WriteValueType(byte b)
        {
            mBuffer[mBufferIndex] = b;
            mBufferIndex++;
        }

        private byte ReadValueType()
        {
            byte b = mBuffer[mBufferIndex];
            mBufferIndex++;
            return b;
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        //   Public Add to package functions
        // ----------------------------------------------------------------------------------------------------------------------------------------------------

        //1 - Add String Simple UTF8
        public void AddValue(string s)
        {
            if (s!=null)
            {               
                CheckBuffer(s.Length + 5); // String is 1(type) + 4(len) + data           
            }
            else
            {
                CheckBuffer(6); // String null is 1(type) + 4(len) + (-1)=null
            }
            if (isNonAsciiString(s))
            {
                WriteValueType(StringUTF16Type);
                WriteUnicodeString(s);
            }
            else
            {
                WriteValueType(StringType);
                WriteString(s);
            }

        }

        //2 - Add Int
        public void AddValue(int val)
        {
            CheckBuffer(5); // type(2) + int size 4
            WriteValueType(IntType);
            WriteInt(val);
        }

        //3 Add - Enum
        public void AddEnumValue(object val)
        {
            CheckBuffer(val.ToString().Length + 5);
            WriteValueType(EnumValueType);
            WriteString(val.ToString());
        }

        //4 Add - StringUTF16
        public void AddStringUTF16(string val)
        {
            int len = UTF16.GetByteCount(val);
            CheckBuffer(len + 5);
            WriteValueType(StringUTF16Type);
            WriteUnicodeString(val);
        }
        
        //5 Add - List of Strings
        public void AddValue(List<String> list) 
	    {
		    // List is #5
		    // First we write the size of the List and then String one after another 
            if (list != null)
            {
                int len = 0;
                foreach (string s in list)
                {
                    if (s != null)
                        len += s.Length;
                }
                CheckBuffer(len + 5); 
            }
		    WriteValueType(ListStringType);
		    WriteInt(list.Count);
		    foreach(string s in list)
		    {
                if (isNonAsciiString(s))
                {
                    WriteUnicodeString(s);
                }
                else
                {
                    WriteString(s);
                }
		    }
        }

        // 6 List of PayLoad
        public void AddListPayLoad(List<PayLoad> elements)
        {
            CheckBuffer(5);
            WriteValueType(ListPayLoadType);
            //TODO: replace to WriteLen
            WriteInt(elements.Count);

            foreach (PayLoad PL in elements)
            {
                WriteBytes(PL.GetPackage());
            }
        }

        // 7 Add Bytes
        public void AddBytes(Byte[] bytes)
        {
            CheckBuffer(1 + bytes.Length);
            WriteValueType(BytesType);
            WriteInt(bytes.Length);
            WriteBytes(bytes);
        }

        // 8 Key Value Pair
        public void AddKeyValuePair(string key, string value)
        {
            CheckBuffer(2);
            WriteValueType(KeyValuePair);
            AddValue(key);
            AddValue(value);
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        //   EOF - Public Add to package functions
        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public List<KeyValuePair<string, string>> GetParsedResult()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            int j = 0;

            while (mBuffer[mBufferIndex] != LastByteMarker)
            {
                byte byteValue = ReadValueType();
                switch (byteValue)
                {
                    case StringType:
                        result.Add(new KeyValuePair<string, string>("Actual" + j++, ReadString()));
                        break;
                    case IntType:
                        result.Add(new KeyValuePair<string, string>("Actual" + j++, "" + ReadInt()));
                        break;
                    case EnumValueType:
                        result.Add(new KeyValuePair<string, string>("Actual" + j++, ReadString()));
                        break;

                    case StringUTF16Type:
                        result.Add(new KeyValuePair<string, string>("Actual" + j++, ReadUnicodeString()));
                        break;

                    case ListStringType:
                        int count = ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            result.Add(new KeyValuePair<string, string>("Actual" + j++, ReadString()));
                        }

                        break;

                    case KeyValuePair:
                        result.Add(ReadKeyValuePair());
                        break;

                    default:
                        throw new Exception("Parsing Error/Wrong Value Type b =" + byteValue + ". Name of the Payload is " + Name + " & Buffer Index is " + mBufferIndex);
                }
            }
            return result;
        }

        public string GetErrorValue()
        {
            return "Error:- " + GetValueInt() + ":" + GetValueString();
        }

        public Point GetValuePoint()
        {
            Point pt = new Point(-1, -1);

            byte pointByte = ReadValueType();

            if(pointByte == StringType)
            {
                string str = ReadString();

                string[] ptArr = str.Split('_');
                if (ptArr.Length == 2)
                {
                    pt.X = int.Parse(ptArr[0]);
                    pt.Y = int.Parse(ptArr[1]);

                    return pt;
                }
                else
                {
                    throw new Exception("String Parsing Error/wrong value type str=" + str + " Name of the payload is" + Name + " Buffer Index is: " + mBufferIndex);
                }
            }
            else
            {
                throw new Exception("String Parsing Error/wrong value type pointByte=" + pointByte + " Name of the payload is" + Name + " Buffer Index is: " + mBufferIndex);
            }
        }

        public string GetValueString()
        {
            byte b = ReadValueType();
            
            if (b == StringType)
            {                
                string s = ReadString();
                return s;
            }
            else
            {
                throw new Exception("String Parsing Error/wrong value type b=" + b + " Name of the payload is" + Name + " Buffer Index is: " + mBufferIndex);
            }
        }

        public int GetValueInt()
        {
            byte b = ReadValueType();
            
            if (b == IntType)
            {
                int val = ReadInt();
                return val;
            }
            else
            {
                throw new Exception("Int Parsing Error/wrong value type");
            }
        }

        public string GetValueEnum()
        {
            byte b = ReadValueType();
            
            if (b == EnumValueType)
            {
                string s = ReadString();
                return s;
            }
            else
            {
                throw new Exception("Enum Parsing Error/wrong value type");
            }
        }

        public string GetStringUTF16()
        {
            
            byte b = ReadValueType();
            
            if (b == StringUTF16Type)
            {
                string s = ReadUnicodeString();
                return s;
            }
            else
            {
                throw new Exception("String UTF16 Parsing Error/wrong value type");
            }
        }

        public List<string> GetListString()
        {
            List<string> list = new List<string>();

            byte b = ReadValueType();        
            
            if (b == ListStringType)
            {
                int count = ReadInt();
                for(int i=0;i<count;i++)
                {
                    string s = ReadString();
                    list.Add(s);
                }                
                return list;
            }
            else
            {
                throw new Exception("List String Parsing Error/wrong value type");
            }
        }

        // Use to write screen shot or any binary data
        private void WriteBytes(byte[] Bytes)
        {
            CheckBuffer(Bytes.Length + 4);

            Buffer.BlockCopy(Bytes, 0, mBuffer, mBufferIndex, Bytes.Length);            
            mBufferIndex += Bytes.Length;
        }

        public List<PayLoad> GetListPayLoad()
        {
            List<PayLoad> list = new List<PayLoad>();

            byte b = ReadValueType();           
            if (b == ListPayLoadType)
            {
                int count = ReadInt(); // How many Payloads we have
                for (int i = 0; i < count; i++)
                {
                    PayLoad PL = ReadPayLoad();                    
                    list.Add(PL);
                }
                return list;
            }
            else
            {
                throw new Exception(PAYLOAD_PARSING_ERROR);
            }
        }

        private PayLoad ReadPayLoad()
        {
            int len = ReadInt();
            mBufferIndex -= 4;
            Byte[] Bytes = new byte[len + 4];
            Buffer.BlockCopy(mBuffer, mBufferIndex, Bytes, 0, len +4);
            mBufferIndex += len + 4;
            PayLoad PL = new PayLoad(Bytes);
            return PL;
        }

        //For Easy debugging and enable to see the payload we override toString
        public override string ToString()
        {
            string s = "Packet Dump: " + Environment.NewLine;
            int CurrentBufferIndex = mBufferIndex; // Keep the current index and restore later
            mBufferIndex = 4;
            s += "Len = " + GetDataLen() + Environment.NewLine;
            s += ",Name = " + ReadString() + Environment.NewLine;

            byte ValueType = mBuffer[mBufferIndex];
            int idx = 0;
            while (ValueType != LastByteMarker)
            {
                idx++;
                s += ", [" + idx + "] ";
                switch (ValueType)
                {
                    case 1:
                        string s1 = GetValueString();
                        s += "String = " + s1 + Environment.NewLine;
                        break;
                    case 2:
                        int i = GetValueInt();
                        s += "Int = " + i + Environment.NewLine;
                        break;
                    case 3:
                        string e1 = GetValueEnum();
                        s += "Enum = " + e1 + Environment.NewLine;
                        break;
                    case 4:
                        string s16 = GetStringUTF16();
                        s += "StringUTF16 = " + s16 + Environment.NewLine;
                        break;
                    case ListStringType:
                        List<string> list = GetListString();
                        string sList = "";
                        for (int iCount = 0; iCount < list.Count(); iCount++)
                            sList = sList + "::" + list[iCount];
                        s += "List= " + sList + Environment.NewLine;
                        break;
                    case 6:
                            // List of Payloads          
                        List<PayLoad> PLs = GetListPayLoad();
                        string sPLList = "List of Payloads, len=" + PLs.Count + Environment.NewLine;
                        int PLi = 0;
                        for (PLi = 0; PLi < PLs.Count; PLi++)
                        {

                            string PLDump = PLs[PLi].ToString();
                            sPLList += "Payload #" + PLi + Environment.NewLine + PLDump + Environment.NewLine;
                        }
                        s += "List of Payloads= " + sPLList + Environment.NewLine;
                        break;
                    case 7:
                        byte[] b =  GetBytes();
                          //Bytes - for screen shot or any binary
                        s += "Bytes(Binary), Len=" + b.Length  + Environment.NewLine;
                        break; 
                    default:
                        throw new Exception("Payload.ToString() Error - Unknown ValueType: " + ValueType);
                }

                ValueType = mBuffer[mBufferIndex];
            }
            mBufferIndex = CurrentBufferIndex;
            return s;
        }

        public void DumpToConsole()
        {
            Reporter.ToConsole(eLogLevel.INFO, this.ToString());
        }

        public static PayLoad Error(String ErrorMessage)
        {
            PayLoad PL = new PayLoad("ERROR");
            PL.AddValue(ErrorMessage);
            PL.ClosePackage();
            return PL;
        }

        public Boolean IsErrorPayLoad()
        {
            if (Name == "ERROR")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Boolean IsOK()
        {
            if (Name == "OK")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // Cretae a Payload with data in one line of code and Closethe Package
        // I.E.:  PayLoad p = new PayLoad("PLName", 123, "aaaa", "koko");
        public PayLoad(string Name, params object[] items)
        {
            this.Name = Name;
            WriteString(Name);

            foreach (object o in items)
            {
                if (o is string)
                {
                    AddValue((string)o);
                }
                else if (o is int)
                {
                    AddValue((int)o);
                }
                else if (o is List<string>)
                {
                    AddValue((List<string>)o);
                }
                else if (o is List<PayLoad>)
                {
                    AddListPayLoad((List<PayLoad>)o);
                }
                else if (o is Enum)
                {
                    AddEnumValue(o);
                }
                    //TODO: add all types...
                else
                {
                    throw new Exception("Unhandled PayLoad item type: " + o.GetType().Name + "  - " + o.ToString());
                }
            }
            ClosePackage();
        }

        public Byte[] GetBytes()
        {
            byte b = ReadValueType();                        
            if (b == BytesType)
            {
                int len = ReadInt();
                Byte[] Bytes = new byte[len];
                Buffer.BlockCopy(mBuffer, mBufferIndex, Bytes, 0, len);
                mBufferIndex += len;
                return Bytes;
            }
            else
            {
                throw new Exception("Byte[] Parsing Error/wrong value type");
            }
        }

        public int PackageLen()
        {
            return mBuffer.Length;
        }

        public enum ErrorCode
        {
            ElementNotFound=404,
            CommandTimeOut=408,
            Unknown=0
        }
    }
}