#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GingerCoreNET.Drivers.CommunicationProtocol
{
    // Prep for better and faster communication Protocol between platforms: C#, JS, Java    
    // We will have the same pack/unpack - C#, JS, Java
    // Payload is the data we want to pass between 2 end points
    // Can be from Ginger socket C# client/server  
    // Can be from C# to Java or HTML/JS
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

    // Payload - first 4 bytes is len, PayloadType 1 byte, Payload name, then data, then last byte=255 for integrity checks
    // We do value type so we can do dump of package to see what exist

    public class NewPayLoad
    {
        // static for reuse and speed

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
        const byte GuidType = 8;    // GUID

        // bool is special case we save one byte as the type include the value
        const byte BoolFalse = 9;    // bool = false
        const byte BoolTrue = 10;    // bool = true

        // Last char is 255 - looks like space but is not and marking end of packaet
        const byte LastByteMarker = 255;
        const int cNULLStringLen = -1;    // if the string we write is null we write len = -1 - save space and parsing time

        //TODO: create the bytes 1024 buffer only if need on create, sometime we give the buffer so waste fix me !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        byte[] mBuffer = new byte[1024];  // start with initial buffer of 1024, will grow if needed
        int mBufferIndex = 4; // We strat to write data at position 4, the first 4 bytes will be the data length

        public string Name {get; set;}
        
        /// Create new empty Payload with name 
        public NewPayLoad(string Name)
        {
            this.Name = Name;
            WritePayloadType(); // temp to save place holder, can be overwritten later
            WriteString(Name);
        }

        /// Create Payload from Bytes
        /// IgnoreExtraSpace - the buffer size might ve bigger then the length defined but there will be End of Packet -255 char at the length postiion - so valid with extra unused space, need to save copy bytes for resize in socket
        public NewPayLoad(byte[] bytes, bool IgnoreExtraSpace = false)
        {
            //TODO: check if we need to do memcopy, to dup and not use the original, since we do not change packets it should be OK.
            mBuffer = bytes;
            ReadPayloadType();
            Name = ReadString();   

            //Verify integrity
            if (IgnoreExtraSpace)
            {
                // verify Last byte marker at the index len
                int len = GetDataLen();
                byte b = bytes[len +3];
                if (b != LastByteMarker)
                {
                    throw new Exception("PayLoad Integrity Error - last byte != 255");
                }
            }
            else
            {
                // verify Last byte marker at the end
                byte b = bytes[bytes.Length - 1];
                if (b != LastByteMarker)
                {
                    throw new Exception("PayLoad Integrity Error - last byte != 255");
                }
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
            //Because if doing PL2 = PL1 when PL generated on same side then buffer index is already at end
            ResetBufferIndex();
        }

        private void ResetBufferIndex()
        {
            mBufferIndex = 5;  // We skip Payload len 4, and Payloadtype 1
            int PayloadNameLen = ReadInt();  // We skip the Payload name by reading the name len
            mBufferIndex += PayloadNameLen; // Now we are back at start of data
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
                    SpaceToAdd = Len + 1024;  // Make sure that we add enought space to hold the new data
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
        //   Payload type - new in GingerCoreNet for async communication
        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        
            //TODO: use the same enum style for val type for enum of byte

        public enum ePaylodType : byte
        {
            Unknown = 0,   // default so need to be set
            SocketRequest = 1,
            SocketResponse =2,
            RequestPayload =3,
            ResponsePayload =4,
            DriverRequest = 5 // for example attach Display
        }

        private ePaylodType mPaylodType = ePaylodType.Unknown;
        public ePaylodType PaylodType
        {
            get { return mPaylodType; }
            set
            {
                mPaylodType = value;
                mBuffer[4] = (byte)value;
            }
        }

        private void WritePayloadType()
        {
            mBuffer[mBufferIndex] = (byte)PaylodType;
            mBufferIndex++;
        }

        private void ReadPayloadType()
        {
            mPaylodType = (ePaylodType)mBuffer[mBufferIndex];
            mBufferIndex++;            
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
		    // First we write the zise of the List and then String one after another 
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
			    WriteString(s);	
		    }
	    }

        // 6 List of PayLoad
        public void AddListPayLoad(List<NewPayLoad> elements)
        {
            CheckBuffer(5);
            WriteValueType(ListPayLoadType);
            //TODO: replace to WriteLen
            WriteInt(elements.Count);

            foreach (NewPayLoad PL in elements)
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

        // 8 Add Guid
        public void AddValue(Guid guid)
        {
            CheckBuffer(1 + 16);  // Guid to byte[] is 16 bytes length + 1 for type
            byte[] bytes = guid.ToByteArray();
            WriteValueType(GuidType);     
            Buffer.BlockCopy(bytes, 0, mBuffer, mBufferIndex, bytes.Length);
            mBufferIndex += 16;
        }

        // 9/10 Add bool = false/true
        public void AddValue(bool value)
        {
            CheckBuffer(1);  // we write bool in one byte since the ValueType type is also the value
            if (value)
            {
                WriteValueType(BoolTrue);
            }
            else
            {
                WriteValueType(BoolFalse);
            }            
        }



        public void AddValueByObjectType(object obj)
        {
            string pType = obj.GetType().Name;            
            switch (pType)
            {
                case nameof(String):
                    AddValue((string)obj);
                    break;
                case "Int32": // nameof(int)?
                    AddValue((int)obj);
                    break;
                case "bool": // nameof(bool)?
                    AddValue((bool)obj);
                    break;
                default:
                    throw new Exception("Unhandled param type - " + pType);
                    // TODO: handle other types
            }
        }

        public object GetValueByObjectType()
        {            
            byte bType = mBuffer[mBufferIndex];  // peek in the next value type             
            switch (bType)
            {                
                case IntType:
                    int i = GetValueInt();
                    return i;                    
                case StringType:
                    string s = GetValueString();
                    if (s == "NULL")
                    {
                        return null;
                    }
                    else
                    {
                        return s;
                    }
                case BoolFalse:
                    return false;
                case BoolTrue:
                    return true;
                //TODO: add other types
                default:
                    throw new Exception("unhandled return type - " + bType);
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        //   EOF - Public Add to package functions
        // ----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public List<object> GetParsedResult()
        {
            List<object> result = new List<object>();
            byte byteValue = ReadValueType();
            switch (byteValue)
                {
                    case StringType:
                        result.Add(ReadString());
                        break;
                    case IntType:
                        result.Add(ReadInt());
                        break;
                    case EnumValueType:                    
                        result.Add(ReadString());
                        break;
                    case StringUTF16Type:
                        result.Add(ReadUnicodeString());                         
                        break;
                    case ListStringType:
                        int count = ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            string s = ReadString();
                            result.Add(s);
                        }
                        break;
                    default:
                        throw new Exception("Parsing Error/Wrong Value Type b =" + byteValue + ". Name of the Payload is " + Name + " & Buffer Index is " + mBufferIndex);
                }           
            return result;
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

        public bool GetValueBool()
        {
            byte b = ReadValueType();
            if (b == BoolTrue)
            {
                return true;
            }
            if (b == BoolFalse)
            {
                return false;
            }
            throw new Exception("bool Parsing Error/wrong value type (not 9 or 10)");
        }

        // Use to write screen shot or any binary data
        private void WriteBytes(byte[] Bytes)
        {
            CheckBuffer(Bytes.Length + 4);

            Buffer.BlockCopy(Bytes, 0, mBuffer, mBufferIndex, Bytes.Length);            
            mBufferIndex += Bytes.Length;
        }

        public List<NewPayLoad> GetListPayLoad()
        {
            List<NewPayLoad> list = new List<NewPayLoad>();

            byte b = ReadValueType();           
            if (b == ListPayLoadType)
            {
                int count = ReadInt(); // How many Payloads we have
                for (int i = 0; i < count; i++)
                {
                    NewPayLoad PL = ReadPayLoad();                    
                    list.Add(PL);
                }
                return list;
            }
            else
            {
                throw new Exception("List PayLoad Parsing Error/wrong value type");
            }
        }

        private NewPayLoad ReadPayLoad()
        {
            int len = ReadInt();
            mBufferIndex -= 4;
            Byte[] Bytes = new byte[len + 4];
            Buffer.BlockCopy(mBuffer, mBufferIndex, Bytes, 0, len +4);
            mBufferIndex += len + 4;
            NewPayLoad PL = new NewPayLoad(Bytes);
            return PL;
        }

        //For Easy debugging and enable to see the payload we override toString
        public override string ToString()
        {
            string s = "Packet Dump: " + Environment.NewLine;
            int CurrentBufferIndex = mBufferIndex; // Keep the current index and restore later
            mBufferIndex = 4;
            s += "Len = " + GetDataLen() + Environment.NewLine;
            ReadPayloadType();
            s += "PayloadType = " + PaylodType.ToString() + Environment.NewLine;
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
                        List<NewPayLoad> PLs = GetListPayLoad();
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
                    case 8: // Guid
                        Guid g = GetGuid();                                              
                        s += "GUID= " + g.ToString() + Environment.NewLine;
                        break;
                    case 9: // bool false                        
                        s += "bool=false " + Environment.NewLine;
                        break;
                    case 10: // bool true                        
                        s += "bool=true " + Environment.NewLine;
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
            String s = this.ToString();
            Console.WriteLine(s);
        }

        public static NewPayLoad Error(String ErrorMessage)
        {
            NewPayLoad PL = new NewPayLoad("ERROR");
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

        // Cretae a Payload with data in one line of code and Close the Package
        // I.E.:  PayLoad p = new PayLoad("PLName", 123, "aaaa", "koko");
        public NewPayLoad(string Name, params object[] items)
        {
            this.Name = Name;
            WritePayloadType();
            WriteString(Name);            

            foreach (object item in items)
            {
                if (item is string)
                {
                    AddValue((string)item);
                }
                else if (item is int)
                {
                    AddValue((int)item);
                }
                else if (item is List<string>)
                {
                    AddValue((List<string>)item);
                }
                else if (item is List<NewPayLoad>)
                {
                    AddListPayLoad((List<NewPayLoad>)item);
                }
                else if (item is Enum)
                {
                    AddEnumValue(item);
                }
                else if (item is Guid)
                {
                    AddValue((Guid)item);
                }
                else if (item is bool)
                {
                    AddValue((bool)item);
                }


                //TODO: add all types...
                else
                {
                    throw new Exception("Unhandled PayLoad item type: " + item.GetType().Name + "  - " + item.ToString());
                }
            }
            ClosePackage();
        }

        public Guid GetGuid()
        {
            byte b = ReadValueType();
            if (b == GuidType)
            {
                int len = 16; //fixed length size of Guid.ToBinary is 16
                Byte[] Bytes = new byte[len];
                Buffer.BlockCopy(mBuffer, mBufferIndex, Bytes, 0, len);
                mBufferIndex += len;
                return new Guid(Bytes);
            }
            else
            {
                throw new Exception("Guid Parsing Error/wrong value type");
            }
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

        internal int PackageLen()
        {
            return mBuffer.Length;
        }
    }
}
