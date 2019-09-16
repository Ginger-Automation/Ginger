/*
Copyright © 2014-2019 European Support Limited

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

// Payload for communication - JavaScript to C# or Java

GingerPayLoadObj = null;

function GingerPayLoad(Name) {
    const LastByteMarker = 255;
    const cNULLStringLen = -1;
    
    this.Name = Name;
    this.mBufferIndex = 4;// We start to write data at position 4, the first 4 bytes will be the data length
   
    this.mBuffer = new ArrayBuffer(1024);  // start with initial buffer of 1024, will grow if needed
    this.dataView = new DataView(this.mBuffer);
    this.mBufferUint8 = new Uint8Array(this.mBuffer);

    this.mName = Name;

    this.WriteString(Name);
}

    GingerPayLoad.prototype.GetPackage = function () {
        //return only bytes used   
        return new Uint8Array(this.mBuffer, 0, this.mBufferIndex);
    }

    GingerPayLoad.prototype.ClosePackage = function () {
        // Write last byte marker       
        this.dataView.setUint8(this.mBufferIndex,255);
        this.mBufferIndex++;
        // Write packet len at position 0
        // -4 since len is not included in data size
        this.dataView.setInt32(0, this.mBufferIndex - 4);
        //Buffer size will be reduced to actual size when doing get Package
        this.PackageSize = this.mBufferIndex;      
        //TODO: Reset buffer index like we do in C# and java side.Not needed for now
    }

    GingerPayLoad.prototype.GetHexString = function () {
        //TODO: Find a faster way. This is time consuming but will work for now
        var hexString = new String();
        var data = this.GetPackage();
        for (var i = 0; i < data.length; i++) {            
            var hexSubStr = this.dataView.getUint8(i).toString(16);
            if (hexSubStr.length == 2)
                hexString += hexSubStr;
            else
                hexString += "0" + hexSubStr;
        }
        return hexString;
    }

    GingerPayLoad.prototype.WriteLen = function (len)
    {
        this.dataView.setInt32(this.mBufferIndex, len);
        this.mBufferIndex += 4;
    }

    GingerPayLoad.prototype.ReadLen = function () {  
        var len = this.dataView.getUint32(this.mBufferIndex);
        this.mBufferIndex += 4;       
        return len;
    }

    GingerPayLoad.prototype.WriteInt = function (val) {
    }

    //Public 
    GingerPayLoad.prototype.GetValueString = function()
    {
        var b = this.ReadValueType();

        //Verify it is String = 1
        if (b == 1)
        {
            var s = this.ReadString();
            return s;
        }
		if (b == 4)
		{
            var s = this.ReadUnicodeString();
            return s;
		}
        else
        {
            return "??? NOT a STRING ??? " + this.mBufferIndex + " b type=" + b;
        }
    }

    GingerPayLoad.prototype.ReadValueType = function () {
        var b = this.dataView.getUint8(this.mBufferIndex);
        this.mBufferIndex++;
        return b;
    }

    // Private
    GingerPayLoad.prototype.ReadString = function () {        
        var len = this.ReadLen();        
        if (len != -1)
        {
            var s = "";

            for (var i = 0; i < len; i++) {               
                 s += String.fromCharCode(this.dataView.getUint8(this.mBufferIndex + i));  
            }

            this.mBufferIndex += len;        
            return s;
        }
        else
        {
            return null;
        }
    }

    // Private
    GingerPayLoad.prototype.ReadUnicodeString = function () {
        var len = this.ReadLen();
        if (len != -1) {
            var s = "";

            for (var i = 0; i < len; i += 2) {
                s += String.fromCharCode(this.dataView.getUint16(this.mBufferIndex + i, true));
            }

            this.mBufferIndex += len;
            return s;
        }
        else {
            return null;
        }

    }

    // Public
    //1 - Add String Simple UTF8
    GingerPayLoad.prototype.AddValueString = function (s)
    {
        if (s!=undefined)
        {
            this.CheckBuffer(s.length + 5); // String is 1(type) + 4(len) + data
        }
        else
        {
            this.CheckBuffer(6); // String null is 1(type) + 4(len) + (-1)=null
        }

        this.WriteValueType(1);// 1 = string type
        this.WriteString(s);
    }


    //read string
    //var dataView = new DataView(buf);
    //var decoder = new TextDecoder('utf-8');
    //return decoder.decode(dataView);
    // Private
    GingerPayLoad.prototype.WriteString = function (val) {
        // String is combined of length = dynamic, then the value       
        if (val != null) {
            var len = val.length;
            this.WriteLen(len);
            for (var i = 0; i < len; i++) {
                this.dataView.setUint8(this.mBufferIndex + i ,val.charCodeAt(i));
            }
            this.mBufferIndex += len;
        }
        else {
            this.WriteInt(-1);
        }
    }

    //5 Add List of Strings

    GingerPayLoad.prototype.AddListPayLoad = function (PayLoadList) {
        this.CheckBuffer(5);      
        this.WriteValueType(6); // 6 is list of PayLoad   
        this.WriteLen(PayLoadList.length);
        
        for (var i = 0; i < PayLoadList.length; i++) {        
            var PL = PayLoadList[i];        
            this.WritePL(PL);
        }
    }

    GingerPayLoad.prototype.WritePL = function (PL) {
        try{
            this.CheckBuffer(PL.mBufferIndex);
        
            for (var i = 0; i < PL.mBufferIndex; i++) {            
                this.dataView.setUint8(this.mBufferIndex + i, PL.dataView.getUint8(i));            
            }     
            this.mBufferIndex += PL.mBufferIndex; 
        }
        catch (error) {
        }
    }

    GingerPayLoad.prototype.AddBytes = function (Bytes) {        
        this.CheckBuffer(Bytes.length);
        for (var i = 0; i < Bytes.length; i++) {            
            this.dataView.setUint8(this.mBufferIndex + i, Bytes[i]);            
        }        
    }

    GingerPayLoad.prototype.WriteBytes = function (Bytes)
    {
        this.CheckBuffer(Bytes.length);    
        //TODO: try to find a one shot copy bytes

        for (var i = 0; i < Bytes.length; i++) {      
            this.dataView.setUint8(this.mBufferIndex + i, Bytes[i]);
        }
        this.mBufferIndex += Bytes.length;
    }

    GingerPayLoad.prototype.WriteValueType = function(b)
    {
        this.dataView.setUint8(this.mBufferIndex, b);   
        this.mBufferIndex++;
    }

    GingerPayLoad.prototype.CheckBuffer = function (Len) {
        try{
            if (this.mBufferIndex + Len > this.mBuffer.byteLength)
            {
                var SpaceToAdd = this.mBuffer.byteLength;
                if (Len > SpaceToAdd)
                {
                    SpaceToAdd = Len + this.mBuffer.byteLength // Len + 1024;  // Make sure that we add enought space to hold the new data
                }
                GingerLib.Log("Increasing Buffer - SpaceToAdd=" + SpaceToAdd);
                
                // Copy the data
                var mBuffer2 = this.mBuffer;
                var dataView2 = new DataView(mBuffer2);
                var mBuffer2Uint8 = new Uint8Array(mBuffer2);

                // Resize
                this.mBuffer = new ArrayBuffer(this.mBuffer.byteLength + SpaceToAdd);
                this.dataView = new DataView(this.mBuffer);
                this.mBufferUint8 = new Uint8Array(this.mBuffer);

                // copy from Backup
                //TODO: find a way to do it faster
                for (var i = 0; i < this.mBufferIndex; i++) {
                    this.dataView.setUint8(i, dataView2.getUint8(i));    
                }
                GingerLib.Log("New Buffer Len = " + this.mBuffer.byteLength);
            }
        }catch(error){
        }
    }

    // need to be after all function to write it have been included in the prototpe
    // Write the name of the package

function define_GingerPayLoad() {
    GingerPayLoadObj = new GingerPayLoad();
}

function GingerPayLoadFromBytes(Bytes) {
    var PL = new GingerPayLoad("New PayLoad");
    PL.mBuffer = new ArrayBuffer(Bytes.length);
    PL.dataView = new DataView(PL.mBuffer);    
    PL.mBufferUint8 = new Uint8Array(PL.mBuffer);
    PL.mBufferIndex = 0;
    PL.AddBytes(Bytes);
    PL.mBufferIndex = 4;
    var PLName = PL.ReadString();
    PL.mName = PLName;
    PL.Name = PLName;
    return PL;
}


function GingerPayLoadFromHexString(str) {
    var a = [];
    for (var i = 0; i < str.length; i += 2) {
        a.push("0x" + str.substr(i, 2));
    }
    return GingerPayLoadFromBytes(a);
}

GingerPayLoad.prototype.GetListPayLoad = function()
{
    var listPayload =[];
    var b = this.ReadValueType();

    //Verify it is List Payload  = 6
    if (b === 6) {
        // How many Payload we have
        var count = this.ReadLen(); 

        for (var i = 0; i < count; i++)
        {
            var PL = this.ReadPayLoad();
            listPayload.push(PL);
        }
        return listPayload;
    }
    else {
        return null;
    }
}

GingerPayLoad.prototype.ReadPayLoad = function ()
{
    var len = this.ReadLen();
    this.mBufferIndex -= 4;

    var byteArray = new Uint8Array(this.mBuffer);

    //create array with size = len + 4
    var Bytes = new Uint8Array(len + 4);

    //copy data from byteArray to Bytes
    for (var i = 0, j = this.mBufferIndex; i < len + 4; i++ , j++) {
        Bytes[i] = byteArray[j];
    }

    this.mBufferIndex += len + 4;
    var PLResp = new GingerPayLoadFromBytes(Bytes);

    return PLResp;
}
GingerPayLoad.prototype.GetInputValues = function (paramList)
{
    var mapList = {};
    for (var i = 0; i < paramList.length; i++)
    {
        var pl = paramList[i];
        paramKey = pl.GetValueString();
        paramValue = paramList[i].GetValueString();
        mapList[paramKey] = paramValue;
    }
    return mapList;
}
