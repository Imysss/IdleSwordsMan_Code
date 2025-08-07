// using System;
// using UnityEngine;
// using Unity.Notifications.Android;
// using UnityEngine.Android;
//
// public class LocalPushManager : MonoBehaviour
// {
//     private const string ChannelId = "my_channel_id";
//
//     public void Init()
//     {
// #if UNITY_ANDROID && !UNITY_EDITOR
//         if (AndroidBuildVersion() >= 33)
//         {
//             if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
//             {
//                 Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//             }
//         }
// #endif
//
//         var channel = new AndroidNotificationChannel
//         {
//             Id = ChannelId,
//             Name = "기본 채널",
//             Importance = Importance.Default,
//             Description = "기본 알림 채널"
//         };
//
//         AndroidNotificationCenter.RegisterNotificationChannel(channel);
//     }
//
//     private int AndroidBuildVersion()
//     {
// #if UNITY_ANDROID && !UNITY_EDITOR
//         using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
//         {
//             return version.GetStatic<int>("SDK_INT");
//         }
// #else
//         return 0;
// #endif
//     }
//
//     public void SendNotification(string title, string message, DateTime fireTime)
//     {
//         var notification = new AndroidNotification
//         {
//             Title = title,
//             Text = message,
//             FireTime = fireTime,
//             SmallIcon = "icon_0",
//             LargeIcon = "icon_0",
//             ShowInForeground = false
//         };
//
//         AndroidNotificationCenter.SendNotification(notification, ChannelId);
//     }
//
//     public void CancelAllNotifications()
//     {
//         AndroidNotificationCenter.CancelAllNotifications();
//     }
// }