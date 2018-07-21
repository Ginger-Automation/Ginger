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

import android.content.ContentValues;
import android.media.MediaRecorder;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;

import java.io.IOException;

/**
 * Created on 2/13/2017.
 */

public class Audio {


    public void StartRecording(String fileName)
    {

        try {
            final MediaRecorder recorder = new MediaRecorder();

            ContentValues values = new ContentValues(3);
            values.put(MediaStore.MediaColumns.TITLE, fileName);

            //TODO: add in action config of which source to use
            recorder.setAudioSource(MediaRecorder.AudioSource.VOICE_RECOGNITION);

            // the options below are not working on some devices...
            recorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
            recorder.setAudioEncoder(MediaRecorder.AudioEncoder.DEFAULT);
            recorder.setOutputFile(fileName);
            recorder.prepare();
            recorder.start();

            //TODO: get from action or wait to stop command
            Thread.sleep(10000);

            recorder.stop();
            recorder.release();
        } catch (Exception e){
            Log.e(GingerService.LOG_TAG, "StartRecording Error: " +  e.getMessage());
        }
    }

    private void AudioRecord() {
        try {
            String mFileName = null;


            mFileName = Environment.getExternalStorageDirectory().getAbsolutePath();
            mFileName += "/audiorecordtest.3gp";

            MediaRecorder mRecorder = new MediaRecorder();

            mRecorder.setAudioSource(MediaRecorder.AudioSource.CAMCORDER);
            mRecorder.setOutputFormat(MediaRecorder.OutputFormat.THREE_GPP);
            mRecorder.setOutputFile(mFileName);

            mRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AMR_NB);

            mRecorder.prepare();

            mRecorder.start();

            try {
                Thread.sleep(5000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }

            mRecorder.stop();
            mRecorder.release();
            mRecorder = null;


        } catch (IOException e) {
            Log.e(GingerService.LOG_TAG, "AudioRecord Error: " + e.getMessage());
        }
    }
}
