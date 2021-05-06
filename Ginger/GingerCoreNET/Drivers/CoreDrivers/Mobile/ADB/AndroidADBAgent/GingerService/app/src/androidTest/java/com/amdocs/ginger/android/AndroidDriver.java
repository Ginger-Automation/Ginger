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

package com.amdocs.ginger.android;

import android.app.Instrumentation;
import android.app.UiAutomation;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Bitmap;
import android.net.wifi.WifiManager;
import android.os.BatteryManager;
import android.os.RemoteException;
import android.support.test.InstrumentationRegistry;
import android.support.test.uiautomator.By;
import android.support.test.uiautomator.UiDevice;
import android.support.test.uiautomator.UiObject;
import android.support.test.uiautomator.UiObject2;
import android.support.test.uiautomator.UiObjectNotFoundException;
import android.support.test.uiautomator.UiSelector;
import android.telephony.PhoneStateListener;
import android.telephony.TelephonyManager;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.List;
import com.amdocs.ginger.Sockets.*;
import java.io.*;

/**
 * Created on 10/11/2016.
 */

public class AndroidDriver {

    //TODO: replace hard code with LogTag
    private UiDevice mDevice;
    private UiAutomation mAutomation;
    private Instrumentation.ActivityMonitor am;
    private Bitmap  mLastScreenShot = null;
    Instrumentation mInstrumentation;
    GingerService mGingerService;

    public void Init(Instrumentation inst, GingerService GS)
    {
        mInstrumentation = inst;
        mGingerService = GS;
        mDevice = UiDevice.getInstance(inst);
        mAutomation = inst.getUiAutomation();

        //TODO: do we need it? is it for browser access?
        // Wake Up device
        try {
            if (!mDevice.isScreenOn()) {
                mDevice.wakeUp();
            }
        } catch (RemoteException e) {
            e.printStackTrace();
        }
    }

    public PayLoad ProcessCommand(PayLoad PL) {

        try {

            Log.d(GingerService.LOG_TAG, "ProcessCommand: " + PL.Name);
            //TODO: put most common at the top - speed
            if ("GetPageSource".equals(PL.Name)) {
                return GetPageSource();
            } else if ("GetScreenShot".equals(PL.Name)) {
                return HandleGetScreenShot();
            } else if ("Press".equals(PL.Name)) {
                String key = PL.GetValueString();
                return HandlePress(key);
            } else if ("Wifi".equals(PL.Name)) {
                String action = PL.GetValueString();
                return HandleWifi(action);
            } else if ("Shell".equals(PL.Name)) {
                String command = PL.GetValueString();
                return HandleShell(command);
            } else if ("UIElementAction".equals(PL.Name)) {
                //TODO: fixme enum to work
                String LocateBy = PL.GetValueString();
                String LocateValue = PL.GetValueString();
                //TODO: need to be get enum
                String ElementType = PL.GetValueString();
                //TODO: need to be get enum
                String ElementAction = PL.GetValueString();
                List<PayLoad> Params = PL.GetListPayLoad();
                return HandleUIElementAction(LocateBy, LocateValue, ElementType, ElementAction, Params);
            } else if ("ClickXY".equals(PL.Name)) {
                int x = PL.GetValueInt();
                int y = PL.GetValueInt();
                return ClickXY(x,y);
            } else if ("PressKeyCode".equals(PL.Name)) {
                int x = PL.GetValueInt();
                mDevice.pressKeyCode(x);
                return PayLoad.OK("Done");
            } else if ("DeviceAction".equals(PL.Name)) {
                return PayLoad.OK("Done");
            } else if ("Swipe".equals(PL.Name)) {
                int xs = PL.GetValueInt();
                int ys = PL.GetValueInt();
                int xe = PL.GetValueInt();
                int ye = PL.GetValueInt();
                int steps = PL.GetValueInt();
                Log.d(GingerService.LOG_TAG, "Swipe X1=" + xs + ", Y1=" + ys + ", X2=" + xe + ", Y2=" + ye+ ", Steps=" + steps);
                mDevice.swipe(xs,ys, xe, ye, steps);

                return PayLoad.OK("Done");
            } else if ("GetClickables".equals(PL.Name)) {

                return HandleGetClickables();
            } else if ("GetVersion".equals(PL.Name)) {
                return PayLoad.OK("v1.0.0"); // TODO: put in top var and change every build based on beta version
            } else if ("StartRecording".equals(PL.Name)) {

                return StartRecording();
            } else if ("ScreenRecord".equals(PL.Name)) {
                String size = PL.GetValueString();
                int bitrate = PL.GetValueInt();
                int timelimit = PL.GetValueInt();
                return ScreenRecord(size, bitrate, timelimit);
            }
             else if ("Phone".equals(PL.Name)) {
                return HandlePhoneAction(PL);
            }
            else if ("Battery".equals(PL.Name)) {
                return HandleBatteryAction(PL);
            }
            else if ("Media".equals(PL.Name)) {
                return HandleMediaAction(PL);
            }
            else if ("SMS".equals(PL.Name)) {
                return HandleSMSAction(PL);
            }
            else
            {
                return PayLoad.Error("Unknown Command: " + PL.Name);
            }
        }
        catch (Exception e)
        {
            Log.e(GingerService.LOG_TAG, e.getMessage());
        }

        // / TODO: return err payload
        return PayLoad.Error("Unknown Command " + PL.Name);
    }

    private PayLoad HandleSMSAction(PayLoad pl) {
        try {
            String rc = SendSMS();
            return  PayLoad.OK("Done - " + rc);
        } catch (Exception e) {
            return  PayLoad.Error("HandleSMSAction Error: " + e.getMessage());
        }
    }


    private PayLoad HandleMediaAction(PayLoad pl) {
        Log.d(GingerService.LOG_TAG, "Audio Command - Start Recording");

        //TODO: need to get temp file and delete when done
        String FileName = "/sdcard/777.mpeg4";
        Log.d(GingerService.LOG_TAG, "File Name - " + FileName);
        mGingerService.GrantPermission("android.permission.RECORD_AUDIO");
        mGingerService.GrantPermission("android.permission.WRITE_EXTERNAL_STORAGE");

        Audio audio = new Audio();
        audio.StartRecording(FileName);

        PayLoad PL = new PayLoad("Audio Recording");
        PL.AddValue("777.mpeg4");
        AddFileToPayLoad(FileName, PL);
        PL.ClosePackage();
        return  PL;
    }

    private void AddFileToPayLoad(String FileName, PayLoad PL)
    {
        //TODO: Check if we can give permssion once during installation
        mGingerService.GrantPermission("android.permission.READ_EXTERNAL_STORAGE");
        try {
            File file = new File(FileName);
            byte[] fileData = new byte[(int) file.length()];
            DataInputStream dis = new DataInputStream(new FileInputStream(file));
            dis.readFully(fileData);
            dis.close();
            PL.AddBytes(fileData);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private PayLoad HandlePhoneAction(PayLoad PL) {

        String PhoneAction = PL.GetValueString();

        Log.d(GingerService.LOG_TAG, "HandlePhoneAction: " + PhoneAction);

        if ("Dial".equals(PhoneAction)) {
            String value = PL.GetValueString();
            try {
                Log.d(GingerService.LOG_TAG, "Dial: " + value);
                String cmd = "am start -a android.intent.action.CALL -d tel:" + value;
                mDevice.executeShellCommand(cmd);
                return PayLoad.OK("Done");
            } catch (IOException e) {
                return PayLoad.Error("HandlePhoneAction Dial Error: " + e.getMessage());
            }
        }
            if ("EndCall".equals(PhoneAction)) {
                try {
                    Log.d(GingerService.LOG_TAG, "EndCall shell with input keyevent KEYCODE_ENDCALL");
                    String cmd2 = "input keyevent KEYCODE_ENDCALL";
                    mDevice.executeShellCommand(cmd2);
                    return PayLoad.OK("Done");
                } catch (IOException e) {
                    return PayLoad.Error("HandlePhoneAction EndCall Error: " + e.getMessage());
                }
            }
        return PayLoad.Error("Unknown Phone action - " + PhoneAction);
    }


    private PayLoad ScreenRecord(String size, int bitrate, int timelimit) {
        Log.d(GingerService.LOG_TAG, "ScreenRecord");

        try {
            // file Name
            String Params = "/sdcard/video.mp4";
            if (timelimit > 0);
            {
                Params += " --time-limit " + timelimit;
            }

            if (size != null && !size.equals(""))
            {
                Params += " --size " + size;
            }

            if (bitrate >0) {
                Params += " --bit-rate " + bitrate;
            }

            // TODO: add parameter if to wait for end of recording or exist - async
            // Need to call shel and no wait
            // Add file name
            String cmd = "screenrecord " + Params ;
            Log.d(GingerService.LOG_TAG, "cmd: " + cmd);
            // if with wait then call
			if (1==2) {
                mDevice.executeShellCommand(cmd);
            }
            else {
                // try with & at the end = background
                mAutomation.executeShellCommand(cmd);
            }

            return  PayLoad.OK("Screen Recording started");
        } catch (IOException e) {
            e.printStackTrace();
            return  PayLoad.Error("Error ScreenRecord: " + e.getMessage());
        }
    }

    private PayLoad ClickXY(int x, int y) {
        Log.d(GingerService.LOG_TAG, "ClickXY x=" + x + ", y=" + y);
        mDevice.click(x,y);

        //TODO: config on action if to wait for idle
        mDevice.waitForIdle();

        return  PayLoad.OK("Done");
    }

    private PayLoad HandleUIElementAction(String LocateBy,String LocateValue, String ElementType, String ElementAction, List<PayLoad> Params) {

        Log.d(GingerService.LOG_TAG, "UIElementAction: " + LocateBy + "=" + LocateValue + " ElementType=" + ElementType + " ElementAction=" + ElementAction);

        //Create hashmap for Params for easy access when needed by name
        HashMap<String, String> ParamList = new HashMap<String, String>();
        for (PayLoad pl : Params)
        {
            String Param = pl.GetValueString();
            String Value = pl.GetValueString();
            ParamList.put(Param, Value);
        }

        UiObject UIO =  FindElement(LocateBy, LocateValue, ElementType);

        if (UIO == null)
        {
            Log.d(GingerService.LOG_TAG, "Element not found: LocateBy=" + LocateBy + ", LocateValue" + LocateValue);
            return PayLoad.Error("Element not found: " + LocateBy + "=" + LocateValue + " of Type=" + ElementType);
        }

        try {
            Log.d(GingerService.LOG_TAG, "Element found: Exist=" + UIO.exists() + ", ContentDescription=" + UIO.getContentDescription() );
            switch (ElementAction)
            {
                case "Click":
                    if (UIO.isClickable()) {
                        Boolean bClicked = UIO.click();
                        // TODO: check bClicked - see what happent on different elemenents
                        return PayLoad.OK("Element Clicked");
                    }
                    else
                    {
                        return  PayLoad.Error("Element not clickable");
                    }
                case "SetText":
                    if (UIO.isEnabled()) {
                        String Value = ParamList.get("Value");
                        UIO.setText(Value);
                        // validate the setText result
                        String txt = UIO.getText();
                        if (Value.equals(txt)) {
                            return PayLoad.Error("Problem in setting element value - Expected Value= " + Value + " but actual= " + txt);
                        }
                        return PayLoad.OK("Element value set to: " + txt);
                    }
                    else
                    {
                        return  PayLoad.Error("Element not Enabled");
                    }


                case "GetValue":
                    String elemtxt = UIO.getText();
                    PayLoad plrc = new PayLoad("ElemetValue");
                    plrc.AddValue(elemtxt);
                    plrc.getClass();
                    return plrc;
                default:
                    return PayLoad.Error("Unknown element action: " + ElementAction);

            }

        } catch (UiObjectNotFoundException e) {
            e.printStackTrace();
        }
        return PayLoad.OK("Done");
    }

    private UiObject FindElement(String LocateBy, String LocateValue, String ElementType) {

        String ClassName = GetClassNameForElementType(ElementType);

        Log.d(GingerService.LOG_TAG, "ClassName for '" + ElementType + "' = " + ClassName);

        switch (LocateBy)
        {
            case "ByResourceID":
                Log.d(GingerService.LOG_TAG, "Searching for element by resource ID=" + LocateValue);
                return mDevice.findObject(new UiSelector().resourceId(LocateValue));

            case "ByContentDescription":// this is Content-Description
                return mDevice.findObject(new UiSelector().description(LocateValue));
            case "ByText":// this is Content-Description
                return mDevice.findObject(new UiSelector().text(LocateValue));
            case "ClassName":  // this is Content-Description
                return mDevice.findObject(new UiSelector().className(LocateValue));
            case "ByXY":
                //TODO: Get all elements and find which one in bounds of the X,Y...
                List<UiObject2> list = mDevice.findObjects(By.descContains(""));  // Maybe try clickable first
                int X = 100; // TODO get from LocValue
                int Y = 100;
                for (UiObject2 o : list) {
                    // Scan and find the smallest rect which contains X,Y
                    if (o.getVisibleBounds().contains(X,Y)) {
                        // return o.;
                    }
                }

                // scan the list
                //FIXME: to be device X,Y
                return mDevice.findObject( new UiSelector().index(0));
        }
        return  null;
    }

    private String GetClassNameForElementType(String ElementType) {

        switch (ElementType)
        {
            case "Button":
                return "android.widget.Button";
            case "TextBox":
                return "android.widget.EditText";

            //TODO: all the rest
            default:
                return null;
        }
    }

    private PayLoad StartRecording() {
        return PayLoad.OK("Done");
    }

    private PayLoad HandleGetClickables() {
        Log.d(GingerService.LOG_TAG, "HandleGetClickables");
        List<UiObject2> col = mDevice.findObjects(By.clickable(true));

        String rc = "";

        for (UiObject2 o: col) {
            rc += o.getResourceName() + " " + o.getClassName() + " " + o.getContentDescription() + "\r\n";
        }

        Log.d(GingerService.LOG_TAG, "HandleGetClickables rc = " + rc);

        PayLoad plrc = new PayLoad("clickables");
        plrc.AddValue(rc);
        plrc.ClosePackage();
        return plrc;
    }

    private PayLoad HandleShell(String command) {
        String rc = null;
        try {
            rc = mDevice.executeShellCommand(command);
            PayLoad plrc = new PayLoad("Shell Result");
            plrc.AddValue(rc);
            plrc.ClosePackage();
            return plrc;
        } catch (IOException e) {
            e.printStackTrace();
            return PayLoad.Error("Err Executing shell: " + e.getMessage());
        }
    }

    private PayLoad HandleWifi(String action) {
        final Intent intent = new Intent(WifiManager.ACTION_PICK_WIFI_NETWORK);
        Context mContext = InstrumentationRegistry.getContext();
        mContext.startActivity(intent);

        return  PayLoad.OK("Done");
    }

    private PayLoad HandlePress(String key) {

        Log.d(GingerService.LOG_TAG, "Handle Press: " + key);

        switch (key) {
            case "Home":
                mDevice.pressHome();
                break;
            case "Menu":
                mDevice.pressMenu();
                break;
            case "Search":
                mDevice.pressSearch();
                break;
            case "Back":
                mDevice.pressBack();
                break;
            case "Delete":
                mDevice.pressDelete();
                break;
            case "DPadCenter":
                mDevice.pressDPadCenter();
                break;
            case "DPadDown":
                mDevice.pressDPadDown();
                break;
            case "DPadLeft":
                mDevice.pressDPadLeft();
                break;
            case "DPadRight":
                mDevice.pressDPadRight();
                break;
            case "DPadUp":
                mDevice.pressDPadUp();
                break;
            case "Enter":
                mDevice.pressEnter();
                break;
            case "RecentApps":
                try {
                    mDevice.pressRecentApps();
                } catch (RemoteException e) {
                    e.printStackTrace();
                    return PayLoad.Error("Error in Press RecentApps: " + e.getMessage());
                }
                break;

                //TODO add all press commands

            default:
                return PayLoad.Error("Unknown key for Press: " + key);
        }

        Log.d(GingerService.LOG_TAG, "After Press");
        return  PayLoad.OK("Done");
    }
	
    // Add Quality as optional, default to ??
    private PayLoad HandleGetScreenShot() {

        long tStart = System.currentTimeMillis();

        Bitmap bitmap = mAutomation.takeScreenshot();

        if (bitmap == null)   // can happen on secured screen apps
        {
            Log.d(GingerService.LOG_TAG, "Screen Shot - bitmap = null, can be secured screen");
            return  PayLoad.Error("Cannot take screen shot of secured app");
        }

        if (mLastScreenShot != null && bitmap.sameAs(mLastScreenShot))
        {
            PayLoad plrc = new PayLoad("ScreenShotIsSame");
            plrc.ClosePackage();
            return plrc;
        }
        else
        {
            mLastScreenShot = bitmap;
        }


        long tEnd = System.currentTimeMillis();
        long tDelta = tEnd - tStart;

        Log.d(GingerService.LOG_TAG, "Screen Shot Elapsed = " + tDelta);

        ByteArrayOutputStream stream = new ByteArrayOutputStream();

        PayLoad plrc = new PayLoad("ScreenShot");
        //TODO: add quality config in driver - 50 is good and fast, if there are issues change to 100 but check frame rate speed
        // Can try a few and check the frame rate, device with high resolution will benefit most
        bitmap.compress(Bitmap.CompressFormat.JPEG, 50, stream);

        plrc.AddBytes(stream.toByteArray());

        plrc.ClosePackage();
        return plrc;
    }

    private PayLoad GetPageSource() {


        ByteArrayOutputStream os = new ByteArrayOutputStream();
        try {
            mDevice.dumpWindowHierarchy(os);
        } catch (IOException e) {
            e.printStackTrace();
        }

        Log.d(GingerService.LOG_TAG, "Dump Page Source: Len=" + os.size());
        PayLoad plrc = new PayLoad("PageSource");
        plrc.AddBytes(os.toByteArray());
        plrc.ClosePackage();
        return plrc;
    }

    private PayLoad HandleBatteryAction(PayLoad pl) {
        return  getBatteryLevel();
    }

        public PayLoad getBatteryLevel() {

        Intent batteryIntent = mGingerService.registerReceiver(null, new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
        int level = batteryIntent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
        int scale = batteryIntent.getIntExtra(BatteryManager.EXTRA_SCALE, -1);

        // Error checking that probably isn't needed but I added just in case.
        if(level == -1 || scale == -1) {
            return  PayLoad.Error("level == -1 || scale == -1");
        }

        String s = (((float)level / (float)scale) * 100.0f) + "%";
        PayLoad PL = new PayLoad("BatteryLevel");
        PL.AddValue(s);
        PL.ClosePackage();
        return PL;
    }

    private void WaitForPhoneAcitivity() {

        //TODO: make it wait for Ring or other phone activity

        Log.d("Ginger", "WaitForPhoneAcitivity");
        PhoneCallListener phoneListener = new PhoneCallListener();
        TelephonyManager telephonyManager = (TelephonyManager) mGingerService.getSystemService(Context.TELEPHONY_SERVICE);
        telephonyManager.listen(phoneListener, PhoneStateListener.LISTEN_CALL_STATE);

    }

    private void RotateDevice() {
        Log.d("Ginger", "RotateDevice");
        try {
            // TODO: riate based on param
            mDevice.setOrientationRight();
            mDevice.setOrientationLeft();
            mDevice.setOrientationNatural();
        } catch (RemoteException e) {
            e.printStackTrace();
        }
    }

    private String SendSMS() {
        Log.d("Ginger", "SendSMS");
        //TODO: run only once - cache if already done - check if can be done during installation one time for package
        mGingerService.GrantPermission("android.permission.SEND_SMS");

        SMS sms = new SMS(mGingerService);
        String rc = sms.sendSMS("12345","hello");
        Log.d("Ginger", "SendSMS RC=" + rc);

        return  rc;
    }
}






