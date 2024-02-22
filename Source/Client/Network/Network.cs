﻿using System;
using Verse;

namespace GameClient
{
    //Main class that is used to handle the connection with the server

    public static class Network
    {
        //IP and Port that the connection will be bound to
        public static string ip = "";
        public static string port = "";

        //TCP listener that will handle the connection with the server
        public static Listener listener;

        //Useful booleans to check connection status with the server
        public static bool isConnectedToServer;
        public static bool isTryingToConnect;

        public static void StartConnection()
        {
            if (TryConnectToServer())
            {
                ClientValues.ManageDevOptions();
                DialogManager.PopWaitDialog();
                SiteManager.SetSiteDefs();

                Threader.GenerateThread(Threader.Mode.Listener);
                Threader.GenerateThread(Threader.Mode.Sender);
                Threader.GenerateThread(Threader.Mode.Health);
                Threader.GenerateThread(Threader.Mode.KASender);

                Log.Message($"[Rimworld Together] > Connected to server");
            }

            else
            {
                DialogManager.PopWaitDialog();

                RT_Dialog_Error d1 = new RT_Dialog_Error("The server did not respond in time");
                DialogManager.PushNewDialog(d1);

                ClearAllValues();
            }
        }

        private static bool TryConnectToServer()
        {
            if (isTryingToConnect || isConnectedToServer) return false;
            else
            {
                try
                {
                    isTryingToConnect = true;

                    isConnectedToServer = true;

                    listener = new Listener(new(ip, int.Parse(port)));

                    return true;
                }
                catch { return false; }
            }
        }

        public static void DisconnectFromServer()
        {
            listener.connection.Dispose();

            Action toDo = delegate
            {
                DialogManager.PushNewDialog(new RT_Dialog_Error("Connection to the server has been lost!", 
                    delegate { DisconnectionManager.DisconnectToMenu(); }));
            };

            ClearAllValues();
            Master.threadDispatcher.Enqueue(toDo);
            Log.Message($"[Rimworld Together] > Disconnected from server");
        }

        public static void ClearAllValues()
        {
            isTryingToConnect = false;
            isConnectedToServer = false;

            ClientValues.CleanValues();
            ServerValues.CleanValues();
            ChatManager.ClearChat();
        }
    }
}
