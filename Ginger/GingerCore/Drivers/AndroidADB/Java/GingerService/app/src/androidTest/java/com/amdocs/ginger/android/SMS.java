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

import android.app.Activity;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.telephony.SmsManager;

/**
 * Created on 2/10/2017.
 */

public class SMS {

    GingerService mGingerService;
    String RC = "";

    public SMS(GingerService GingerService) {
        mGingerService = GingerService;
    }

    public String sendSMS(String phoneNumber, String message)
    {

        String SENT = "SMS_SENT";
        String DELIVERED = "SMS_DELIVERED";

        PendingIntent sentPI = PendingIntent.getBroadcast(mGingerService, 0,
                new Intent(SENT), 0);

        PendingIntent deliveredPI = PendingIntent.getBroadcast(mGingerService, 0,
                new Intent(DELIVERED), 0);

        //---when the SMS has been sent---
        mGingerService.registerReceiver(new BroadcastReceiver(){
            @Override
            public void onReceive(Context arg0, Intent arg1) {
                switch (getResultCode())
                {
                    case Activity.RESULT_OK:
                        RC = "SMS sent";
                        break;
                    case SmsManager.RESULT_ERROR_GENERIC_FAILURE:
                        RC =  "Generic failure";
                        break;
                    case SmsManager.RESULT_ERROR_NO_SERVICE:
                        RC =  "No service";
                        break;
                    case SmsManager.RESULT_ERROR_NULL_PDU:
                        RC =   "Null PDU";
                        break;
                    case SmsManager.RESULT_ERROR_RADIO_OFF:
                        RC =   "Radio off";
                        break;
                }
            }
        }, new IntentFilter(SENT));

        //---when the SMS has been delivered---
        mGingerService.registerReceiver(new BroadcastReceiver(){
            @Override
            public void onReceive(Context arg0, Intent arg1) {
                switch (getResultCode())
                {
                    case Activity.RESULT_OK:
                        RC += " - SMS delivered";
                        break;
                    case Activity.RESULT_CANCELED:
                        RC +=  "- SMS not delivered";


                        break;
                }
            }
        }, new IntentFilter(DELIVERED));

        SmsManager sms = SmsManager.getDefault();
        sms.sendTextMessage(phoneNumber, null, message, sentPI, deliveredPI);

        // TODO: wait for result
        return RC;
    }
}
