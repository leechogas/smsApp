using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android.Telephony;
using System;
using Android;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Util;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Text;
using System.Threading;
using Android.Views.Animations;
using System.Collections.Generic;
using System.Collections;

namespace MhylesOrderingApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {      
        static readonly int REQUEST_SENDSMS = 0;
        private SmsManager _smsManager;
        private BroadcastReceiver _smsSentBroadcastReceiver, _smsDeliveredBroadcastReceiver;
        View layout;
        static View hiddenLayout;
        static View scrollView;
        static ListView listView;
        static string myResult { get; set; }
        static List<string> smsList = new List<string>();
        static ArrayAdapter<string> arrayAdapter;
        static EditText phoneNum, smsCustomer, smsAddress, smsCustNo, smsOrderProd, smsProdQty, smsUnit, smsAgentName;
        static string smsDate;
#pragma warning disable CS0618 // Type or member is obsolete
        static Android.App.ProgressDialog progress;
#pragma warning restore CS0618 // Type or member is obsolete
                              
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.activity_main);

            SupportActionBar.Title = "Mhyles Ordering Messenger App";

            layout = FindViewById(Resource.Id.sample_main_layout);
            hiddenLayout = FindViewById(Resource.Id.hidden_layout);
            scrollView = FindViewById(Resource.Id.scrollViewing);
            listView = FindViewById<ListView>(Resource.Id.lstView);


            var smsBtn = FindViewById<Button>(Resource.Id.btnSend);
            phoneNum = FindViewById<EditText>(Resource.Id.phoneNum);
            smsDate = "";
             smsCustomer = FindViewById<EditText>(Resource.Id.txtCustomer);
             smsAddress = FindViewById<EditText>(Resource.Id.txtAddress);
            smsCustNo = FindViewById<EditText>(Resource.Id.txtCustomerNo);
            smsOrderProd = FindViewById<EditText>(Resource.Id.txtOrderedProduct);
            smsProdQty = FindViewById<EditText>(Resource.Id.txtProductQty);
            smsUnit = FindViewById<EditText>(Resource.Id.txtUnit);
            smsAgentName = FindViewById<EditText>(Resource.Id.txtAgentName);
            var btnNewOders = FindViewById<Button>(Resource.Id.btnNewOrders);
         
            DateTime now = DateTime.Now.ToLocalTime();
            smsDate = now.ToString();
            smsBtn.SetBackgroundResource(Resource.Drawable.buttonBackground);
            btnNewOders.SetBackgroundResource(Resource.Drawable.buttonDesign);
            phoneNum.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsCustomer.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsAddress.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsCustNo.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsOrderProd.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsProdQty.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsUnit.SetBackgroundResource(Resource.Drawable.EditTextStyle);
            smsAgentName.SetBackgroundResource(Resource.Drawable.EditTextStyle);
         
#pragma warning disable CS0618 // Type or member is obsolete
            progress = new Android.App.ProgressDialog(this);
#pragma warning restore CS0618 // Type or member is obsolete
            progress.Indeterminate = true;
            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            progress.SetMessage("Sending.. Please wait!");
            progress.SetCancelable(false);
            _smsManager = SmsManager.Default;

            btnNewOders.Click += (s, e) =>
            {
                clearInput();
                if (hiddenLayout.Visibility == ViewStates.Visible)
                {
                    AlphaAnimation disappearAnimation = new AlphaAnimation(1, 0);
                    disappearAnimation.Duration = 2000;
                    hiddenLayout.StartAnimation(disappearAnimation);
                    hiddenLayout.Visibility = ViewStates.Invisible;
                    scrollView.SetBackgroundResource(0);
                    if(hiddenLayout.Visibility == ViewStates.Invisible)
                    {

                        Handler handler = new Handler();
                        Action action = () =>
                        {
                            hiddenLayout.Visibility = ViewStates.Gone;
                        };
                        handler.PostDelayed(action, 2000);
                    }
                }
                else
                {
                    AlphaAnimation disappearAnimation = new AlphaAnimation(0, 1);
                    disappearAnimation.Duration = 2000;
                    hiddenLayout.StartAnimation(disappearAnimation);
                    hiddenLayout.Visibility = ViewStates.Visible;
                    scrollView.SetBackgroundResource(Resource.Drawable.TextViewStyle);
                }
            };
            smsBtn.Click += (s, e) =>
            {
                progress.Show();
                if (TextUtils.IsEmpty(smsDate) || TextUtils.IsEmpty(phoneNum.Text) || TextUtils.IsEmpty(smsCustomer.Text)
                    || TextUtils.IsEmpty(smsAddress.Text) || TextUtils.IsEmpty(smsCustNo.Text) || TextUtils.IsEmpty(smsOrderProd.Text)
                    || TextUtils.IsEmpty(smsProdQty.Text) || TextUtils.IsEmpty(smsUnit.Text) || TextUtils.IsEmpty(smsAgentName.Text))
                {
                    new Thread(new ThreadStart(delegate
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "Please fill out all the fields", ToastLength.Long).Show());
                        RunOnUiThread(() => progress.Hide());
                    })).Start();                  
                    return;
                }
                else
                {
                    var phone = phoneNum.Text;
                    var message = smsDate + "|" + smsCustomer.Text + "|" + smsAddress.Text + "|" + smsCustNo.Text
                                  + "|" + smsOrderProd.Text + "|" + smsProdQty.Text + "|" + smsUnit.Text + "|" + smsAgentName.Text;
                    var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
                    var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);
                    if ((int)Build.VERSION.SdkInt < 23)
                    {                        
                        _smsManager.SendTextMessage(phone, null, message, piSent, piDelivered);
                        new Thread(new ThreadStart(delegate
                        {
                            RunOnUiThread(() => Toast.MakeText(this, "Sending Orders error! please restart the app", ToastLength.Long).Show());
                            RunOnUiThread(() => progress.Hide());
                        })).Start();                     
                        return;
                    }
                    else
                    {
                        if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) != (int)Permission.Granted)
                        {                          
                            RequestSendSMSPermission();            
                        }
                        else
                        {
                            _smsManager.SendTextMessage(phone, null, message, piSent, piDelivered);
                        }
                    }
                }
            };           
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public void RequestSendSMSPermission()
        {
            Log.Info("MainActivity", "Message permission has NOT been granted. Requesting permission.");
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.SendSms))
            {
                Log.Info("MainActivity", "Displaying message permission rationale to provide additional context.");
                Snackbar.Make(layout, "Message permission is needed to send SMS.",
                    Snackbar.LengthIndefinite).SetAction("OK", new Action<View>(delegate (View obj) {
                        ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.SendSms }, REQUEST_SENDSMS);
                    })).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.SendSms }, REQUEST_SENDSMS);
            }
        }
        public static void clearInput()
        {
            phoneNum.Text = "";
            smsCustomer.Text = "";
            smsAddress.Text = "";
            smsCustNo.Text = "";
            smsOrderProd.Text = "";
            smsProdQty.Text = "";
            smsUnit.Text = "";
            smsAgentName.Text = "";
        }
        protected override void OnResume()
        {                      
            base.OnResume();
            _smsSentBroadcastReceiver = new SMSSentReceiver();
            _smsDeliveredBroadcastReceiver = new SMSDeliveredReceiver();
            RegisterReceiver(_smsSentBroadcastReceiver, new IntentFilter("SMS_SENT"));
            RegisterReceiver(_smsDeliveredBroadcastReceiver, new IntentFilter("SMS_DELIVERED"));         
        }
        protected override void OnPause()
        {
            base.OnPause();       
            UnregisterReceiver(_smsSentBroadcastReceiver);
            UnregisterReceiver(_smsDeliveredBroadcastReceiver);
        }
        //[BroadcastReceiver(Exported = true, Permission = "//receiver/@android:android.permission.SEND_SMS")]
        [BroadcastReceiver]
        public class SMSSentReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                switch ((int)ResultCode)
                {
                    case (int)Result.Ok:
                        Toast.MakeText(Application.Context, "SMS has been sent", ToastLength.Short).Show();
                        myResult = "Orders Sent";
                        progress.Hide();
                        break;
                    case (int)SmsResultError.GenericFailure:
                        Toast.MakeText(Application.Context, "Generic Failure", ToastLength.Short).Show();
                        myResult = "Orders Send Faild";
                        progress.Hide();
                        break;
                    case (int)SmsResultError.NoService:
                        Toast.MakeText(Application.Context, "No Service", ToastLength.Short).Show();
                        myResult = "Orders Send Faild";
                        progress.Hide();
                        break;
                    case (int)SmsResultError.NullPdu:
                        Toast.MakeText(Application.Context, "Null PDU", ToastLength.Short).Show();
                        myResult = "Orders Send Faild";
                        progress.Hide();
                        break;
                    case (int)SmsResultError.RadioOff:
                        Toast.MakeText(Application.Context, "Radio Off", ToastLength.Short).Show();
                        myResult = "Orders Send Faild";
                        progress.Hide();
                        break;
                    default:
                        break;
                }
                if (hiddenLayout.Visibility == ViewStates.Visible)
                {
                    AlphaAnimation disappearAnimation = new AlphaAnimation(1, 0);
                    disappearAnimation.Duration = 2000;
                    hiddenLayout.StartAnimation(disappearAnimation);
                    hiddenLayout.Visibility = ViewStates.Invisible;
                    scrollView.SetBackgroundResource(0);
                    if (hiddenLayout.Visibility == ViewStates.Invisible)
                    {

                        Handler handler = new Handler();
                        Action action = () =>
                        {
                            hiddenLayout.Visibility = ViewStates.Gone;
                        };
                        handler.PostDelayed(action, 2000);
                    }
                }
                else
                {
                    AlphaAnimation disappearAnimation = new AlphaAnimation(0, 1);
                    disappearAnimation.Duration = 2000;
                    hiddenLayout.StartAnimation(disappearAnimation);
                    hiddenLayout.Visibility = ViewStates.Visible;
                    scrollView.SetBackgroundResource(Resource.Drawable.TextViewStyle);
                }
                
                    smsList.Add(phoneNum.Text.ToString());
                    smsList.Add(smsCustomer.Text.ToString());
                    smsList.Add(smsAddress.Text.ToString());
                    smsList.Add(smsCustNo.Text.ToString());
                    smsList.Add(smsOrderProd.Text.ToString());
                    smsList.Add(smsProdQty.Text.ToString());
                    smsList.Add(smsUnit.Text.ToString());
                    smsList.Add(smsAgentName.Text.ToString());
                    smsList.Add(myResult + ": " + smsDate.ToString());

                arrayAdapter = new ArrayAdapter<string>(Application.Context, Android.Resource.Layout.SimpleListItem1, smsList);
                listView.Adapter = arrayAdapter;
                if (arrayAdapter.IsEmpty)
                {
                    listView.SetBackgroundResource(0);
                }
                else
                {
                    listView.SetBackgroundResource(Resource.Drawable.buttonBackground);
                }
            }
        }
        //[BroadcastReceiver(Exported = true, Permission = "//receiver/@android:android.permission.SEND_SMS")]
        [BroadcastReceiver]
        public class SMSDeliveredReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                switch ((int)ResultCode)
                {
                    case (int)Result.Ok:
                        Toast.MakeText(Application.Context, "SMS Delivered", ToastLength.Short).Show();
                        progress.Hide();
                        myResult = "Orders Sent";
                        break;
                    case (int)Result.Canceled:
                        Toast.MakeText(Application.Context, "SMS not delivered", ToastLength.Short).Show();
                        progress.Hide();
                        myResult = "Orders Send Faild";
                        break;
                }
            }
        }
    }
}