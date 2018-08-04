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

import android.content.Context;
import android.content.Intent;
import android.os.IBinder;
import android.support.test.InstrumentationRegistry;
import android.support.test.runner.AndroidJUnit4;

import android.app.Instrumentation;
import android.util.Log;

import org.junit.Test;
import org.junit.runner.RunWith;

import android.content.ServiceConnection;
import android.content.ComponentName;

/**
 * Instrumentation test, which will execute on an Android device.
 *
 * @see <a href="http://d.android.com/tools/testing">Testing documentation</a>
 */
@RunWith(AndroidJUnit4.class)
public class GingerServiceStarter {

    Context appContext;

    GingerService mService;
    boolean mBound = false;

    @Test
    public void useAppContext() throws Exception {
        // Context of the app under test.
        appContext = InstrumentationRegistry.getTargetContext();

        Instrumentation inst = InstrumentationRegistry.getInstrumentation();

        // strat GingerService and create binding to send the data of instrumentation
        Intent GS = new Intent(appContext, GingerService.class);
        appContext.bindService(GS, mConnection, Context.BIND_AUTO_CREATE);

        //Wait for the service to be up
        // TODO: add time out
        while (!mBound)
        {
            Log.d("Ginger", "mBound=" + mBound);
            Sleep(100);
        }

        if (mBound) {
            mService.SetInst(inst);
        }
    }

    private void Sleep(int i) {
        try {
            Thread.sleep(i);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    private ServiceConnection mConnection = new ServiceConnection() {

        @Override
        public void onServiceConnected(ComponentName className,
                                       IBinder service) {
            Log.d("Ginger", "mConnection - onServiceConnected");

            // We've bound to LocalService, cast the IBinder and get LocalService instance
            GingerService.LocalBinder binder = (GingerService.LocalBinder) service;
            mService = binder.getService();
            mBound = true;
        }

        @Override
        public void onServiceDisconnected(ComponentName arg0) {
            Log.d("Ginger", "mConnection - onServiceDisconnected");
            mBound = false;
        }
    };
}
