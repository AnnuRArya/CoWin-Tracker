using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using System.Threading.Tasks;
using Co_Win_Tracker.DAL;
using Co_Win_Tracker.Adapter;
using System.Linq;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.App;
using System;
using OS = Android.OS;
using static Android.Provider.Contacts;

namespace Co_Win_Tracker
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ProgressBar ProgressBar;
        private LinearLayout LayoutMask;
        private Button _btnRefresh;
        private Button _btnRefreshweek;
        private ListView _vaccineListView;
        private AlarmManager alarmManager;
        private PendingIntent pendingIntent;
        private List<Session> v;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            LayoutMask = FindViewById<LinearLayout>(Resource.Id.layoutMaskMain);
            ProgressBar = FindViewById<ProgressBar>(Resource.Id.pbSpinnerMain);
            LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Invisible;
            _btnRefreshweek = FindViewById<Button>(Resource.Id.buttonrefreshweek);
            _btnRefresh = FindViewById<Button>(Resource.Id.buttonrefreshtoday);
            _btnRefresh.Click += _btnRefresh_Click;
            _btnRefreshweek.Click += _btnRefreshweek_Click;
            _vaccineListView = FindViewById<ListView>(Resource.Id.vaccinelist);
            alarmManager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent alarmIntent = new Intent(this, typeof(SocketServerReceiver));
            pendingIntent = PendingIntent.GetBroadcast(this, 0, alarmIntent, 0);
            alarmManager.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, 10000, 10000, pendingIntent);
            SocketBackgroundService.SocketAlive += SocketBackgroundService_SocketAlive;
        }

        protected override void OnResume()
        {
            base.OnResume();
            RefreshNextDay();
        }
        private void SocketBackgroundService_SocketAlive(object sender, System.EventArgs e)
        {
            RefreshNextDayNotify();
          
        }

        private void _btnRefreshweek_Click(object sender, System.EventArgs e)
        {
            try
            {
                LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Visible;
                Refresh();
            }
            catch (System.Exception ex)
            {
            }
        }

        private void RefreshNotfy()
        {
            Task.Run(() =>
            {
                try
                {
                    List<Session> vai = new List<Session>();
                    var result = VaccineManager.GetSlotsByDistrict();
                    RunOnUiThread(() =>
                    {
                        if (result?.Object != null)
                        {
                            var vf = result?.Object?.centers.ToList();
                            //var jj=v.Where(x=>x.sessions.Where(t=>t.available_capacity>0).ToList())
                            foreach (var n in vf)
                            {
                                n.sessions.ForEach(x => x.name = n.name);
                                vai.AddRange(n.sessions);
                            }
                            v = vai.Where(x => x.available_capacity > 0).ToList();
                            if (v?.Count > 0)
                            {

                                var launchIntent = new Intent(this, typeof(MainActivity));
                                MakeNotifySoundAndVibrate("CO-WIN", "Vaccine available", launchIntent);
                            }
                            else
                            {
                                //Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            //Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                        }
                    });
                }
                catch (System.Exception c)
                {
                }
            });
        }

        private void Refresh()
        {
            Task.Run(() =>
            {
                try
                {
                    List<Session> vai = new List<Session>();
                    var result = VaccineManager.GetSlotsByDistrict();
                    RunOnUiThread(() =>
                    {
                        LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Invisible;
                        if (result?.Object != null)
                        {
                            var vf = result?.Object?.centers.ToList();
                            //var jj=v.Where(x=>x.sessions.Where(t=>t.available_capacity>0).ToList())
                            foreach (var n in vf)
                            {
                                n.sessions.ForEach(x => x.name = n.name);
                                vai.AddRange(n.sessions);
                            }
                            v = vai.Where(x => x.available_capacity > 0).ToList();
                            if (v?.Count > 0)
                            {

                                _vaccineListView.Adapter = new VaccineAdapter(this, v);
                            }
                            else
                            {
                                Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                        }
                    });
                }
                catch (System.Exception c)
                {
                }
            });
        }

        private void _btnRefresh_Click(object sender, System.EventArgs e)
        {
            try
            {
                LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Visible;
                RefreshNextDay();
            }
            catch (System.Exception ex)
            {
            }
        }

        private void RefreshNextDay()
        {
            Task.Run(() =>
            {
                try
                {
                    var result = VaccineManager.GetSlots();
                    RunOnUiThread(() =>
                    {
                        LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Invisible;
                        if (result?.Object != null)
                        {
                            if (result?.Object.sessions?.Count > 0)
                            {
                                v = result?.Object.sessions.Where(x => x.available_capacity > 0)?.ToList();
                                if (v?.Count > 0)
                                {
                                    _vaccineListView.Adapter = new VaccineAdapter(this, v);
                                }
                                else
                                {
                                    LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Visible;
                                    Task.Run(() => {
                                        Refresh();
                                    });
                                    Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                        }
                    });
                }
                catch (System.Exception c)
                {
                }
            });
        }
        private void RefreshNextDayNotify()
        {
            Task.Run(() =>
            {
                try
                {
                    var result = VaccineManager.GetSlots();
                    RunOnUiThread(() =>
                    {
                        if (result?.Object != null)
                        {
                            if (result?.Object.sessions?.Count > 0)
                            {
                                v = result?.Object.sessions.Where(x => x.available_capacity > 0 && x.min_age_limit == 18)?.ToList();
                                if (v?.Count > 0)
                                {
                                    var launchIntent = new Intent(this, typeof(MainActivity));
                                    MakeNotifySoundAndVibrate("CO-WIN", "Vaccine available", launchIntent);
                                }
                                else
                                {
                                   // LayoutMask.Visibility = ProgressBar.Visibility = ViewStates.Visible;
                                    Task.Run(() =>
                                    {
                                        RefreshNotfy();
                                    });
                                }
                            }
                        }
                        else
                        {
                            //Toast.MakeText(this, "No Slots available", ToastLength.Short).Show();
                        }
                    });
                }
                catch (System.Exception c)
                {
                }
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void MakeNotifySoundAndVibrate(string title, string message, Intent launchIntent)
        {
            try
            {
                //#if Release
                var unMuteIntent = new Intent(this, typeof(AlarmReceiver));
                //unMuteIntent.SetAction(this.GetString(Resource.String.UnMuteSound));
                var unMutePending = PendingIntent.GetBroadcast(this, 0, unMuteIntent, PendingIntentFlags.CancelCurrent);
                var muteIntent = new Intent(this, typeof(AlarmReceiver));
                // muteIntent.SetAction(this.GetString(Resource.String.MuteSound));
                var mutepending = PendingIntent.GetBroadcast(this, 0, muteIntent, PendingIntentFlags.CancelCurrent);
                NotificationManager notificationManager = (NotificationManager)this.GetSystemService(Context.NotificationService);
                PendingIntent pendingIntent = null;
                PendingIntent mutependingIntent = null;
                NotificationCompat.Action action = null;
                Intent intent = new Intent(this, launchIntent.Class);
                String CHANNEL_ID = "COWIN";
                String Mute_Channel = "Mute";
                if (OS.Build.VERSION.SdkInt >= OS.BuildVersionCodes.O)
                {
                    // The id of the channel. 
                    //if (IsSoundEnabled)
                    //{
                    Java.Lang.ICharSequence name = new Java.Lang.String(this.GetString(Resource.String.app_name));// The user-visible name of the channel.
                    NotificationChannel mChannel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.High);
                    notificationManager.CreateNotificationChannel(mChannel);
                    mChannel.EnableLights(true);
                    Intent actionintent = new Intent(global::Android.Provider.Settings.ActionChannelNotificationSettings);
                    actionintent.PutExtra(global::Android.Provider.Settings.ExtraAppPackage, Application.Context.PackageName);
                    actionintent.PutExtra(global::Android.Provider.Settings.ExtraChannelId, mChannel.Id);
                    pendingIntent = PendingIntent.GetActivity(this, 0, actionintent, 0);
                    //}
                    //else
                    //{

                    //    Java.Lang.ICharSequence mutename = new Java.Lang.String(this.GetString(Resource.String.app_name));// The user-visible name of the channel.
                    //    NotificationChannel muteChannel = new NotificationChannel(Mute_Channel, mutename, NotificationImportance.High);
                    //    muteChannel.SetSound(null, null);
                    //    muteChannel.EnableLights(true);
                    //    notificationManager.CreateNotificationChannel(muteChannel);
                    //    Intent actionmuteintent = new Intent(global::Android.Provider.Settings.ActionChannelNotificationSettings);
                    //    actionmuteintent.PutExtra(global::Android.Provider.Settings.ExtraAppPackage, Application.Context.PackageName);
                    //    actionmuteintent.PutExtra(global::Android.Provider.Settings.ExtraChannelId, muteChannel.Id);
                    //    pendingIntent = PendingIntent.GetActivity(this, 0, actionmuteintent, 0);
                    //}
                }
                //else
                //{
                //    Intent actionintent = new Intent(global::Android.Provider.Settings.ActionApplicationDetailsSettings, Uri.FromParts("package", Application.Context.PackageName, null));
                //    actionintent.AddFlags(ActivityFlags.FromBackground);
                //    pendingIntent = PendingIntent.GetActivity(this, 0, actionintent, 0);
                //}
                // if (NotificationManagerCompat.From(this).AreNotificationsEnabled())
                //{
                //    action = new NotificationCompat.Action.Builder(Resource.Drawable.ic_ack_green, this.GetString(Resource.String.DisableNotifications), pendingIntent).Build();
                //}
                //else
                //{
                //    action = new NotificationCompat.Action.Builder(Resource.Drawable.ic_grey_tick, this.GetString(Resource.String.EnableNotifications), pendingIntent).Build();
                //}
                PendingIntent resultPendingIntent = PendingIntent.GetActivity(this, 0, launchIntent, PendingIntentFlags.UpdateCurrent);
                //if (IsSoundEnabled)
                //{
                NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
               .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
              .SetAutoCancel(true)
              .SetContentIntent(resultPendingIntent)
              .SetContentTitle(title)
              .SetContentText(message)
              .SetBadgeIconType(NotificationCompat.BadgeIconSmall)
                .SetVisibility(NotificationCompat.VisibilityPrivate)
                .SetPriority(NotificationCompat.PriorityMax)
                .SetChannelId(CHANNEL_ID)
                .SetCategory(NotificationCompat.CategoryMessage);
                if (OS.Build.VERSION.SdkInt >= OS.BuildVersionCodes.O)
                {
                    builder.SetChannelId(CHANNEL_ID);
                }
                builder.SetDefaults(NotificationCompat.DefaultSound | NotificationCompat.DefaultVibrate);
                //builder.AddAction(action);
                notificationManager.Notify(0, builder.Build());
                OS.PowerManager pm = (OS.PowerManager)this
                    .GetSystemService(Context.PowerService);
                bool isScreenOn = pm.IsInteractive; // check if screen is on
                if (!isScreenOn)
                {
                    OS.PowerManager.WakeLock wl = pm.NewWakeLock(OS.WakeLockFlags.ScreenDim | OS.WakeLockFlags.AcquireCausesWakeup, Application.Context.PackageName);
                    wl.Acquire(3000); //set your time in milliseconds
                }
                //#endif
            }
            catch (Exception ex)
            { }
        }
    }
}