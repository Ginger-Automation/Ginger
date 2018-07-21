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

package com.amdocs.ginger;
import java.io.UnsupportedEncodingException;
import java.nio.charset.CharacterCodingException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

public class PayLoad {

	
	byte[] mBuffer = new byte[1024];
    int mBufferIndex = 4;
	
    byte LastByteMarker = (byte)255;
    
    public String Name;
    
	final int cNULLStringLen = -1;

    PayLoad(String Name)
    {
        this.Name = Name;
        WriteString(Name);
    }

    /// Create PayLoad from Bytes
    public PayLoad(byte[] bytes)
    {
        mBuffer = bytes;
        Name = ReadString();
    }    
    
    public byte[] GetPackage()
    {
        return mBuffer;
    }

    public String GetHexString()
    {
    	String hexStr=javax.xml.bind.DatatypeConverter.printHexBinary(mBuffer);
        
    	return hexStr; 
    }
    
    public static PayLoad GetPayLoadFromHexString(String hexStr)
    {	
    	GingerAgent.WriteLog("Start GetPayLoadFromHexString::" + hexStr);
    	
    	byte[] responseBytes=javax.xml.bind.DatatypeConverter.parseHexBinary(hexStr);    	
    	
    	GingerAgent.WriteLog("Inside GetPayLoadFromHexString");
    	
    	return new PayLoad(responseBytes);
    }	
    
    public void ClosePackage()
    {
    	CheckBuffer(1);
        mBuffer[mBufferIndex] = LastByteMarker;
        mBufferIndex++;

        SetDataLen(mBufferIndex-4);

        //Reduce buffer size ot the len of the packet
        //TODO: find if there is better way
        byte[] bytesfinal = new byte[mBufferIndex]; 
        System.arraycopy(mBuffer, 0,bytesfinal , 0, mBufferIndex);        
        mBuffer = bytesfinal;  
        
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
        mBuffer[0] = (byte)((Len >> 24) & 0xFF);
        mBuffer[1] = (byte)((Len >> 16) & 0xFF);
        mBuffer[2] = (byte)((Len >> 8) & 0xFF);
        mBuffer[3] = (byte)(Len & 0xFF);            
    }

    private int GetDataLen()
    {    	
        int Len = ((mBuffer[0] & 0xFF) << 24) + ((mBuffer[1] & 0xFF)<< 16) + ((mBuffer[2] & 0xFF)<< 8) + (mBuffer[3] & 0xFF);
        return Len;
    }

    // UTF8 String
    private void WriteString(String val)
    {
        // String is combined of length = dynamic, then the value                           
		try {
			if (val!=null)
			{
				byte[] bytes;
				bytes = val.getBytes("UTF-8");			
				WriteLen(bytes.length);
				System.arraycopy(bytes,0,mBuffer, mBufferIndex,bytes.length);
		        mBufferIndex += bytes.length;
			}
			else
			{
				WriteLen(cNULLStringLen);
			}
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}        
    }
    private void WriteUnicodeString(String val)
    {                      
		try {
			if (val!=null)
			{
				byte[] bytes;
				bytes = val.getBytes("UTF-16");			
				WriteLen(bytes.length);
				System.arraycopy(bytes,0,mBuffer, mBufferIndex + 4 ,bytes.length);
		        mBufferIndex += bytes.length;
			}
			else
			{
				WriteLen(cNULLStringLen);
			}
		} catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}        
    }
    private String ReadString()
    {        
		try {
			int len = ReadLen(); 
			 if (len != cNULLStringLen)
	         {
				  byte[] bytes = new byte[len];        
			        System.arraycopy(mBuffer,mBufferIndex,bytes, 0 ,len);
			        mBufferIndex += len;
			        String s = new String(bytes, "UTF-8");
			        return s;
	         }
			 else
			 {
				 return null;
			 }
	      
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        return null;
    }
    private String ReadUnicodeString()
    {
    	try {
		    	int len = ReadLen(); 
		        byte[] bytes = new byte[len];   
		        System.arraycopy(mBuffer, mBufferIndex, bytes, 0, len);
		        mBufferIndex += len; 
		        String decodedString = new String(bytes, "UTF-16LE");
		        return decodedString;   
    		}
    	catch (UnsupportedEncodingException e) {
		e.printStackTrace();
        }
    	return null;
    }

    
    private String ReadStringOrUnicodeString()
    {
    	try {
	    	int len = ReadLen(); 
	        byte[] bytes = new byte[len];   
	        System.arraycopy(mBuffer, mBufferIndex, bytes, 0, len);
	        mBufferIndex += len; 
	        
	        if (isValidUTF8(bytes))
	        {
	        	 String s = new String(bytes, "UTF-8");
			     return s;
	        }
	        else
	        {
		        String decodedString = new String(bytes, "UTF-16LE");
		        return decodedString;   
	        }
		}
    	catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		    }
    	return null;
    }
    
    private HashMap<String, String> ReadKeyValuePair()
    {
        byte byteValue = ReadValueType();
        String key = ReadString();
        
		byteValue = ReadValueType();
        String value = ReadString();
        
		HashMap<String,String> map=new HashMap<String,String>();
        map.put(key, value);
        
		return map;
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
    
    private void WriteLen(int len)
    {
    	
        mBuffer[mBufferIndex] = (byte)((len >> 24) & 0xFF);
        mBufferIndex++;
        mBuffer[mBufferIndex] = (byte)((len >> 16) & 0xFF);
        mBufferIndex++;
        mBuffer[mBufferIndex] = (byte)((len >> 8) & 0xFF);
        mBufferIndex++;
        mBuffer[mBufferIndex] = (byte) (len & 0xFF);
        mBufferIndex++;
    }

    
    private int ReadLen()
    {
    	// Len is always positiive 
    	// & 0xFF - is beacuse java byte is unsigned so make it int and shift
        int val = ((mBuffer[mBufferIndex] & 0xFF) << 24) + ((mBuffer[mBufferIndex + 1] & 0xFF)<< 16) + ((mBuffer[mBufferIndex + 2] & 0xFF)<< 8) + (mBuffer[mBufferIndex +3] & 0xFF);            
        mBufferIndex += 4;
        return val;          
    }
    
    
    private int ReadInt()
    {    	  
    	int val = ((mBuffer[mBufferIndex] & 0xFF) << 24) + ((mBuffer[mBufferIndex + 1] & 0xFF)<< 16) + ((mBuffer[mBufferIndex + 2] & 0xFF)<< 8) + (mBuffer[mBufferIndex +3] & 0xFF);   
    	     
        mBufferIndex += 4;
        return val;          
    }

        /// <summary>
    /// Check that we have enought space to add Len bytes of data, if not space will be addded, so it make sure data will be added with no err
    /// </summary>
    /// <param name="Len">Length of data to be added</param>
    private void CheckBuffer(int Len)
    {
        if (mBufferIndex + Len > mBuffer.length)
        {
            int SpaceToAdd = 1024;
            if (Len > SpaceToAdd) 
            {
                SpaceToAdd = Len + 1024;  // Make sure that we add enought space to hold the new data                   
            }
            //TODO: find a way to avoid realloc of new array, just to resize to bigger
            byte[] NewBytes = new byte[mBuffer.length + SpaceToAdd]; 
            System.arraycopy(mBuffer, 0, NewBytes , 0, mBufferIndex);        
            mBuffer = NewBytes;      
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

	private boolean isValidUTF8(final byte[] bytes) {

	    try
		{
	        Charset.availableCharsets().get("UTF-8").newDecoder().decode(java.nio.ByteBuffer.wrap(bytes));
	    }
		catch (CharacterCodingException e)
		{
	        return false;
	    }
	    return true;
	}

    //1 - Add String
    public void AddValue(String s)
    {
    	if(s!=null)
    	{
    		CheckBuffer(s.length() + 5); // String is 1(type) + 4(len) + data	
    	}
    	else
    	{
    		CheckBuffer(6); 
    	}
    	
        WriteValueType((byte)1);// 1 = string type
        WriteString(s);
    }

    //2 - Add Int
    public void AddValue(int val)
    {
        CheckBuffer(5); // type(1) + int size 4
        WriteValueType((byte)2);// 2 = Int type            
        WriteInt(val);
    }

    //3 Add - Enum
    public void AddEnumValue(Object val)
    {
        CheckBuffer(val.toString().length() + 5);
        WriteValueType((byte)3);// 3 = Enum Value
        WriteString(val.toString());
    }

    public void AddStringUTF16(String s)
    {    	   
    	try {
    	   int len = s.getBytes("UTF-16").length;
           CheckBuffer(5);
           WriteValueType((byte)4);
           WriteUnicodeString(s);
    	}
    	catch (UnsupportedEncodingException e) {
			e.printStackTrace();
		}
    }

    
    //5 Add List of Strings
	public void AddValue(List<String> list) 
	{
		// List is #5
		// First we write the zise of the List and then String one after another 
		
		//TODO:  CheckBuffer(val.toString().length() + 5);
		WriteValueType((byte)5);
		WriteLen(list.size());
		for (String s : list)
		{
			WriteString(s);	
		}
		
	}

	
	// 6 List of PayLoad
	public void AddListPayLoad(List<PayLoad> elements) 
	{
		CheckBuffer(5);
		WriteValueType((byte)6);
		WriteLen(elements.size());
		
		for (PayLoad PL: elements)
		{
			WriteBytes(PL.GetPackage());
		}
		
	}
	// 7 List of PayLoad
	public void AddBytes(byte[] Bytes) 
	{
		CheckBuffer(5);
		WriteValueType((byte)7);
		WriteLen(Bytes.length);
		WriteBytes(Bytes);
		
	}
	
	//8 Key Value pair
	public void AddKeyValuePair(String key,String value)
	{
		 CheckBuffer(2);
		 WriteValueType((byte)8);
		 AddValue(key);
		 AddValue(value);
	}	
	
    // Use to write screen shot or any binary data
    private void WriteBytes(byte[] Bytes) 
    {
    	//Check if + 4 is needed 
    	CheckBuffer(Bytes.length + 4);   	
    	System.arraycopy(Bytes,0, mBuffer, mBufferIndex, Bytes.length);
        mBufferIndex += Bytes.length;        
	}


	public String GetValueString()
    {
		byte b = ReadValueType();
		
		//Verify it is String = 1
        if (b == 1)
        {                
            String s = ReadString();
            return s;
        }
		// to read Unicoded string (UTF-16)
        if (b == 4)
        {
      	    String s = ReadUnicodeString();
            return s;
        }
        else
        {
        	// TODO: throw
        	return "err";
        }
    }
	  public String GetStringUTF16()
      {
		  byte b = ReadValueType();
          if (b == 4)
          {
        	  String s = ReadUnicodeString();
              return s;
          }
          else
          {
        		return "err";
          }
      }
    public int GetValueInt()
    {
        byte b = ReadValueType();

        //Verify it is Int = 2
        if (b == 2)
        {
            int val = ReadInt();
            return val;
        }
        else
        {
        	//TODO: throw 
        	return -1;
            // throw new Exception("Int Parsing Error/wrong value type");
        }
    }

    public String GetValueEnum()
    {
        byte b = ReadValueType();

        //Verify it is Enum = 3
        if (b == 3)
        {
            String s = ReadString();
            return s;
        }
        else
        {
        	// TODO: throw
        	return "Err - not an Enum";
        }
    }
	public HashMap GetParamHashMap(List<PayLoad> paramList)
    {
    	String paramKey = "";
    	String paramValue = "";
    	HashMap hm = new HashMap(); 
    	if(paramList!=null)
    	{
	    	for(PayLoad p : paramList)
			{    	
	    		paramKey = p.GetValueString();
	    		paramValue = p.GetValueString();
	    		hm.put(paramKey, paramValue);	    	
			}
	    	GingerAgent.WriteLog("HashMap : " +hm.toString());
    	}   	

    	return hm;
    }
    
    public String GetParamValue(HashMap paramList, String value)
    {
    	String pValue = "";
    	Iterator it = paramList.entrySet().iterator();
        while (it.hasNext()) {
            Map.Entry pair = (Map.Entry)it.next();
            if (pair.getKey().toString().equalsIgnoreCase(value))
            {
            	pValue = (String) pair.getValue();
            	GingerAgent.WriteLog(pair.getKey() + " = " + pValue);
            }
        }
        return pValue;
    } 
    public List<String> GetListString() 
    {
        List<String> list = new ArrayList<String>();

        byte b = ReadValueType();            
        //Verify it is List String  = 5
        if (b == 5)
        {
            int count = ReadLen();
            for(int i=0;i<count;i++)
            {
            	String s  = ReadStringOrUnicodeString();            	  	
                list.add(s);
            }                
            return list;
        }
        else
        {
        	return null;
        }
    }
    
    
    public List<PayLoad> GetListPayLoad()
    {
        List<PayLoad> list = new ArrayList<PayLoad>();

        byte b = ReadValueType();
        //Verify it is List Payload  = 6
        if (b == 6)
        {
            int count = ReadLen(); // How many Payload we have
            for (int i = 0; i < count; i++)
            {
                PayLoad PL = ReadPayLoad();                    
                list.add(PL);
            }
            return list;
        }
        else
        {
        	//TODO: at least sysout err
        	return null;
        }
    }

    private PayLoad ReadPayLoad()
    {
        int len = ReadLen();
        mBufferIndex -= 4;        
        byte[] Bytes = new byte[len + 4];
        System.arraycopy(mBuffer, mBufferIndex, Bytes, 0, len +4);
        mBufferIndex += len + 4;
        PayLoad PL = new PayLoad(Bytes);
        return PL;
    }

    //Use toString instead
    public void DumpToConsole()
    {
        String s = "Packet Dump" + "\n\r";
        int CurrentBufferIndex = mBufferIndex; // Keep the current index and restore later
        mBufferIndex = 4;
        s += "Len = " + GetDataLen() + "\n\r";
        byte ValueType = mBuffer[mBufferIndex];

        while (ValueType != LastByteMarker)
        {
            switch (ValueType)
            {
                case 1:
                    String s1 = GetValueString();
                    s+="String = " + s1 + "\n\r";
                    break;
                case 2:
                    int i = GetValueInt();
                    s += "Int = " + i + "\n\r";
                    break;
                case 3:
                    String e1 = GetValueEnum();
                    s += "Enum = " + e1 + "\n\r";
                    break;
                default:
                	break;                	
            }
            ValueType = mBuffer[mBufferIndex];
        }

        mBufferIndex = CurrentBufferIndex;
        GingerAgent.WriteLog(s);
    }
    
    public static PayLoad Error(String ErrorMessage) 
    {
    	GingerAgent.WriteLog("Returing ERROR PayLoad: " + ErrorMessage);
    	
		PayLoad PL = new PayLoad("ERROR");
		PL.AddValue(ErrorMessage);
		PL.ClosePackage();
		return PL;
	}
    
    public Boolean IsErrorPayLoad()
    {
    	if ("ERROR".equals(Name))
		{
			return true;
		}
    	else
    	{
    		return false;
    	}
    }

	public static PayLoad OK(String ExInfo) 
	{
		PayLoad PL = new PayLoad("OK");
		PL.AddValue(ExInfo);
		PL.ClosePackage();
		return PL;		
	}
    
	public String toString()
	{
        StringBuffer SB = new StringBuffer();
        SB.append("-------------------------------------------------------------------------\n");
        SB.append("Payload Content:\n");
        int CurrentBufferIndex = mBufferIndex; // Keep the current index and restore later
        mBufferIndex = 4;
        SB.append("Name = " + ReadString() + "\n");
        SB.append("Len = " + GetDataLen() + "\n");        

        byte ValueType = mBuffer[mBufferIndex];
        while (ValueType != LastByteMarker)
        {
            switch (ValueType)
            {
                case 1:
                    String s1 = GetValueString();
                    SB.append("String = " + s1 + "\n");
                    break;
                case 2:
                    int i = GetValueInt();
                    SB.append("Int = " + i + "\n");
                    break;
                case 3:
                    String e1 = GetValueEnum();
                    SB.append("Enum = " + e1 + "\n");
                    break;
                case 4:                	
                     String s16 = GetValueString();
                     SB.append("StringUTF16 = " + s16 + "\n");
                    break;
                case 5:
                	List<String> list = GetListString();      
                	SB.append("List<String> = " + "\n");
                	SB.append("List Size = " + list.size() + "\n");
                	int si=0;
            		for (String s : list)
            		{
            			SB.append( si + ": " + s + "\n");
            			si++;
            		}
                case 6: // List of Payloads
                	List<PayLoad> plist = GetListPayLoad();
                	SB.append("List<PayLoad> = " + "\n");
                	SB.append("List Size = " + plist.size() + "\n");
                	int pli = 0;
                	for (PayLoad pl : plist)
            		{
            			SB.append( pli + ": " + pl.toString() + "\n");
            			pli++;
            		}
                default:
            }
            ValueType = mBuffer[mBufferIndex];
        }
        
        SB.append("-------------------------------------------------------------------------");
        
        mBufferIndex = CurrentBufferIndex;
        return SB.toString();		
	}
}