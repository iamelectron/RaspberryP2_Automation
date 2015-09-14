package com.example.anand.windowsiotautomation;

import android.app.Activity;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.SeekBar;
import android.widget.Switch;


import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.io.IOException;
import android.os.StrictMode;
import com.microsoft.windowsazure.mobileservices.*;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.TableOperationCallback;

public class main extends Activity {

    DatagramSocket js;
    private InetAddress group;
    final static String INET_ADDR = "224.3.0.5";
    final static int PORT = 22113;
    MulticastSocket clientSocket;
    InetAddress address;
    SeekBar mSeek;
    Switch mTriac;
    Switch mBuzz;

    //Azure Mobile Service Client Object
    private MobileServiceClient mClient;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();

        StrictMode.setThreadPolicy(policy);

        try {

            //Create Azure mobile service object
            mClient = new MobileServiceClient(
                    "https://myandroid.azure-mobile.net/",
                    "REPLACE THIS WITH YOURS",
                    this
            );
                ///New Socket object
                SSDPSocket spp = new SSDPSocket();
                //Multi cast model number
                spp.send( Build.MODEL.toLowerCase());

        }catch (IOException EX)
        {
            //TextView tv = (TextView)(findViewById(R.id.textView));
            //tv.setText(EX.toString());

        }


         mSeek = (SeekBar)findViewById(R.id.seekBar);

        //Seekbar onProgressChanged event body
        mSeek.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override
            public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                EditText et = (EditText)findViewById(R.id.editText);

                //Send Progress
                try {
                    SSDPSocket spp = new SSDPSocket();
                    if(mSeek.getProgress()<10)                  //need to make of char 2 0 - 9 should be like 00- 09
                    spp.send("!M"+  et.getText() + "0" + (mSeek.getProgress()));        //!M is the command
                    else
                    spp.send("!M"+  et.getText() +  (mSeek.getProgress()));

                } catch (Exception ex) {

                }
            }

            @Override
            public void onStartTrackingTouch(SeekBar seekBar) {

            }

            @Override
            public void onStopTrackingTouch(SeekBar seekBar) {

            }
        });

        mBuzz = (Switch)findViewById(R.id.switch2);
        mTriac = (Switch)findViewById(R.id.switch1);


        ///Buzzer switch event and command statements
        mBuzz.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                EditText et = (EditText)findViewById(R.id.editText);
                try {
                    SSDPSocket spp = new SSDPSocket();
                    if(isChecked)
                        spp.send("!I"+ et.getText());
                    else
                        spp.send("!J"+ et.getText());

                } catch (Exception ex) {

                }
            }
        });

        ///2nd Triac switch's  event and command statements
        mTriac.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                EditText et = (EditText)findViewById(R.id.editText);
                Switch sw = (Switch)findViewById(R.id.switch1);
                try {
                    SSDPSocket spp = new SSDPSocket();
                    if(isChecked)
                        spp.send("!K"+ et.getText());
                    else
                        spp.send("!L"+ et.getText());

                } catch (Exception ex) {

                }
            }
        });

    }

    //Relay On/Off commands
    /////////////////////////////////////////////////////////////////////////////////////////////////
    public  void relay1on(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!A"+ et.getText());
        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay2on(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!B"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay3on(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!C"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay4on(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!D"+ et.getText());

        } catch (Exception ex) {

        }

    }
 ///////////////////////////////////////////////////////////////////////////////////////////////////
    public  void relay1off(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!E"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay2off(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!F"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay3off(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!G"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////
    public  void relay4off(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!H"+ et.getText());

        } catch (Exception ex) {

        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////

    public  void up_up(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!N"+ et.getText());
        } catch (Exception ex) {

        }
    }


    public  void down_down(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!O"+ et.getText());
        } catch (Exception ex) {

        }
    }

    public  void right_right(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!P"+ et.getText());
        } catch (Exception ex) {

        }
    }

    public  void left_left(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!Q"+ et.getText());
        } catch (Exception ex) {

        }
    }

    public  void send_text(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);
        EditText sndt = (EditText)findViewById(R.id.editText2);
        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send(  sndt.getText().toString());
            sndt.setText("");
        } catch (Exception ex) {

        }
    }

    public  void stop_stop(View vv)
    {
        EditText et = (EditText)findViewById(R.id.editText);

        try {
            SSDPSocket spp = new SSDPSocket();
            spp.send("!R"+ et.getText());
        } catch (Exception ex) {

        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();
        if (id == R.id.action_settings) {
            return true;
        }
        return super.onOptionsItemSelected(item);
    }


    ///Class for Azure DB
    public class piData
    {
        public String Id;
        public String text;
        public String commad;
        public String device_name;
        public int device_count;

    }



    //sENDS Multicast packets or Azure Cloud database update commands
    public class  SSDPSocket  extends AsyncTask< Void, String, String > {

        MulticastSocket mSSDPSocket;
        InetAddress broadcastAddress;
        final static String INET_ADDR = "224.3.0.5";
        final static int PORT = 22113;
        byte[] data_buff = new byte[256];

        public  SSDPSocket() throws IOException {
            mSSDPSocket = new MulticastSocket(22113);
            broadcastAddress = InetAddress.getByName(INET_ADDR);
            mSSDPSocket.joinGroup(broadcastAddress);
        }

        public void send(String data1) throws IOException {

            Switch netswitch = (Switch)findViewById(R.id.local_inter);
            ///Sends data to azure database
            if(netswitch.isChecked())
            {
                piData item = new piData();
                item.Id = "YOUR DATABASE ID";
                item.device_name = Build.MODEL;
                item.commad =  data1;
                mClient.getTable(piData.class).update(item);
            }
            ///Multicast command in local network
            else {
                DatagramPacket dp = new DatagramPacket(data1.getBytes(), data1.length(), broadcastAddress, PORT);
                mSSDPSocket.setTimeToLive(2);
                mSSDPSocket.send(dp);
            }

        }
        protected String doInBackground(Void... vv)
        {
            try {
             //   DatagramPacket dp = new DatagramPacket(data_buff, 256, broadcastAddress, PORT);
             //   TextView tx = (TextView)findViewById(R.id.textView);
              //  while(true) {
              //       mSSDPSocket.receive(dp);
                 //   publishProgress(data_buff.toString());
                  //  tx.setText(dp.getData().toString());
              //  }

            }
            catch (Exception ex)
            {
                //discard any exceptions :P
            }
            return null;
        }



    }
}
