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
import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;
import com.amdocs.ginger.Sockets.*;


public class GingerService extends Service implements MessageEvent {

    public static final String LOG_TAG = "Ginger";

    private final IBinder mBinder = new LocalBinder();

    Instrumentation mInstrumentation;

    GingerSocketServer mGingerSocketServer;

    AndroidDriver mAndroidDriver;

    @Override
    public PayLoad GingerSocketMessage(GingerSocket.eProtocolMessageType MessageType, Object obj) {
        // When Client send Message/Requests using GingerSocketProtocol they will arriv here for processing
        switch (MessageType)
        {
            case GetVersion:
            case PayLoad:
                PayLoad PL =  (PayLoad)obj;
                Log.d("Ginger", "GingerSocketMessage - Received PayLoad - " + PL.Name);
                PayLoad rc = mAndroidDriver.ProcessCommand(PL);
                return rc;

            //TODO: handle the rest
            case CloseConnection:
                break;
            case LostConnection:
                break;
            case CommunicationError:
                break;
        }
        return  PayLoad.Error("GingerSocketMessage Error - Message type not handled - "  + MessageType.toString());
    }

    public class LocalBinder extends Binder {
        GingerService getService() {
            // Return this instance of LocalService so clients can call public methods
            return GingerService.this;
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        return mBinder;
    }

    //TODO: Clean
    // Public function to call using this service
    public int getRandomNumber() {
        Sleep(3000);
        return 123;
    }

    public void SetInst(Instrumentation inst) {
        mInstrumentation = inst;

        mAndroidDriver = new AndroidDriver();
        mAndroidDriver.Init(inst, this);
        mGingerSocketServer = new GingerSocketServer();
        mGingerSocketServer.AddMEssageEventObserver(this);
        mGingerSocketServer.StartServer();
    }

    private void Sleep(int i) {
        try {
            Thread.sleep(i);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public GingerService() {
        Log.d("Ginger", "GingerService Class Created");
    }

    @Override
    public void onCreate() {
        // Code to execute when the service is first created
        Log.d("Ginger", "GingerService onCreate");
    }

    @Override
    public void onDestroy() {

        Log.d("Ginger", "Service onDestroy");
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startid) {

        Log.d("Ginger", "Service onStartCommand");
        return START_STICKY;
    }

    private void stopService() {
    }



    public void GrantPermission(String permsision)
    {
        String cmd = "pm grant " + mInstrumentation.getTargetContext().getPackageName() + " " + permsision;
        Log.d("Ginger" , cmd);

        // mInstrumentation.de
        mInstrumentation.getUiAutomation().executeShellCommand(cmd);
    }
}
