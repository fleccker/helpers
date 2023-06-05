using helpers.Network.Controllers;
using helpers.Network.Events;
using helpers.Network.Features;

using System;
using System.Net.Sockets;
using System.Threading;

using WatsonTcp;

namespace helpers.Network.Reconnection
{
    public class Reconnector : NetworkFeatureBase, IReconnector
    {
        private volatile int m_MaxAttempts = 10;
        private volatile int m_CurAttempts;

        private volatile float m_Delay;

        private volatile bool m_IsAllowed;
        private volatile bool m_Cancel;

        private volatile ReconnectionState m_State;

        private DateTime m_LastTime;
        private DateTime m_NextTry;

        private Thread m_ReconnectThread;

        public bool IsAllowed => m_IsAllowed;
        public bool IsReconnecting => m_State is ReconnectionState.Reconnecting;

        public int MaxAttempts { get => m_MaxAttempts; set => m_MaxAttempts = value; }
        public int CurrentAttempts => m_CurAttempts;

        public float CurrentDelay => m_Delay;

        public DateTime LastTry => m_LastTime;
        public DateTime NextTry => m_NextTry;

        public ReconnectionState State => m_State;

        public bool ShouldReconnect(DisconnectReason reason) => reason != DisconnectReason.AuthFailure;

        public override void Uninstall(INetworkEventCollection networkEventCollection)
        {
            m_Cancel = true;
            m_ReconnectThread = null;
        }

        public void Start()
        {
            Log.Info($"Starting the reconnection sequence ..");

            m_ReconnectThread = new Thread(Advance);
            m_ReconnectThread.Start();
        }

        public void Stop()
        {
            m_IsAllowed = false;
            m_Delay = 1500f;
            m_CurAttempts = 0;
            m_State = ReconnectionState.Connected;
            m_Cancel = true;
            m_ReconnectThread = null;
        }

        private void Advance()
        {
            Log.Info("Reconnection thread started.");

            m_IsAllowed = true;
            m_Cancel = false;
            m_NextTry = DateTime.Now;
            m_LastTime = DateTime.Now;
            m_State = ReconnectionState.Reconnecting;
            m_CurAttempts = 0;
            m_Delay = 1500f;

            while (!m_Cancel)
            {
                if (!m_IsAllowed) continue;
                if (m_State is ReconnectionState.Cooldown)
                {
                    if ((DateTime.Now - m_LastTime).TotalMilliseconds >= m_Delay)
                    {
                        m_State = ReconnectionState.Reconnecting;
                        Log.Info("Cooldown expired, attempting reconnection.");
                        continue;
                    }
                }
                else if (m_State is ReconnectionState.CooldownFailure)
                {
                    if (!(DateTime.Now >= NextTry))
                    {
                        continue;
                    }

                    m_Delay += 1000f;

                    if (m_Delay >= 60000f)
                    {
                        Stop();
                        m_State = ReconnectionState.CooldownFailure;
                        Log.Warn("The delay reached a minute - disabling reconnection.");
                        continue;
                    }

                    m_State = ReconnectionState.Reconnecting;
                    Log.Info("Cooldown expired, attempting reconnection.");
                    continue;
                }
                else if (m_State is ReconnectionState.Reconnecting)
                {
                    if ((DateTime.Now - m_LastTime).TotalMilliseconds >= 2500)
                    {
                        m_CurAttempts++;

                        if (m_CurAttempts >= m_MaxAttempts)
                        {
                            m_State = ReconnectionState.CooldownFailure;
                            m_NextTry = DateTime.Now + TimeSpan.FromSeconds(60);
                            m_LastTime = DateTime.Now;
                            m_CurAttempts = 0;
                            Log.Warn("Reached the reconnection attempt limit! Waiting 60 seconds before retrying ..");
                            continue;
                        }

                        m_LastTime = DateTime.Now;
                        m_State = ReconnectionState.Reconnecting;

                        Log.Info("Reconnecting ..");

                        try
                        {
                            (Controller as INetworkController).Connect();
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }
}