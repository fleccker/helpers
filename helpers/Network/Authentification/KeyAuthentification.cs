using helpers.Network.Features;
using helpers.Network.Interfaces;
using helpers.Network.Peers;
using helpers.Network.Authentification.Messages;
using helpers.Network.Requests;

using System;
using System.Timers;
using helpers.Network.Events;
using helpers.Network.Controllers;
using helpers.Network.Authentification.Storage;
using System.IO;

namespace helpers.Network.Authentification
{
    public class KeyAuthentification : NetworkFeatureBase, IAuthentification
    {
        private IAuthentificationStorage m_Storage;
        private IAuthentificationData m_Data;

        private AuthentificationStatus m_Status;
        private AuthentificationFailureReason m_FailureReason;

        private DateTime m_SentAt;
        private DateTime m_ReceivedAt;

        private Timer m_Timer;

        public IAuthentificationStorage Storage => m_Storage;
        public IAuthentificationData Data => m_Data;
        
        public AuthentificationStatus Status => m_Status;
        public AuthentificationFailureReason FailureReason => m_FailureReason;

        public DateTime SentAt => m_SentAt;
        public DateTime ReceivedAt => m_ReceivedAt;

        public double TimeRequired => (ReceivedAt - SentAt).TotalMilliseconds;

        public override void Install(INetworkEventCollection networkEventCollection)
        {
            this.ExecuteIfClient(client =>
            {
                m_Data = new AuthentificationData(client.Key);
            });

            this.ExecuteIfServerPeer(peer =>
            {
                m_Storage = peer.Controller.Authentification.Storage;
            });

            this.ExecuteIfServer(server =>
            {
                m_Storage = new KeyFileStorage($"{Directory.GetCurrentDirectory()}/key_list.txt");
                m_Storage.Reload();
            });
        }

        public override void OnEnabled()
        {
            if (Controller is INetworkPeer peer)
            {
                Start(peer);
            }
        }

        public void Start(INetworkPeer peer)
        {
            if (peer.Controller.RequiresAuthentification)
            {
                Log.Info($"Starting authentification challenge for peer: {peer.Id}");

                peer.ExecuteFeature<IRequestManager>(requests =>
                {
                    requests.Send(new AuthentificationRequestMessage(), HandleAuth);

                    m_SentAt = DateTime.Now;
                    m_Status = AuthentificationStatus.RequestSent;

                    RunTimer();

                    Log.Info("Sent authentification request.");
                });
            }
        }

        public void FailAuth(AuthentificationFailureReason failureReason)
        {
            m_FailureReason = failureReason;
            m_Status = AuthentificationStatus.Failed;

            Log.Error($"Authentification failed: {failureReason}");

            if (Controller is INetworkPeer peer)
            {
                peer.Disconnect(WatsonTcp.DisconnectReason.Removed);
            }
        }

        private void HandleAuth(IRequest request, AuthentificationRequestMessage authentificationRequestMessage)
        {
            m_ReceivedAt = request.ReceivedAt;

            Log.Info($"Received authentification request.");

            if (m_Data != null && m_Data.ClientKey != null)
            {
                request.RespondSuccess(m_Data);
                Log.Info($"Authentification key sent: {m_Data.ClientKey}");
                m_SentAt = DateTime.Now;
            }
            else
            {
                request.RespondFail();
                Log.Error("Authentification failed - missing client key.");
                m_SentAt = DateTime.Now;
            }
        }

        private void HandleAuth(IResponse response)
        {
            m_ReceivedAt = DateTime.Now;

            Log.Info($"Received authentification response.");

            if (!response.IsSuccess)
            {
                FailAuth(AuthentificationFailureReason.InvalidKey);
                return;
            }

            response.RunIf<IAuthentificationData>(data =>
            {
                if (m_Storage.IsValid(data))
                {
                    m_Status = AuthentificationStatus.Authentificated;
                    Log.Info($"Authentificated.");
                }
                else
                {
                    FailAuth(AuthentificationFailureReason.UnknownKey);
                }
            });
        }

        private void RunTimer()
        {
            m_Timer = new Timer(100);
            m_Timer.Elapsed += OnElapsed;
            m_Timer.Start();
        }

        private void StopTimer()
        {
            m_Timer.Elapsed -= OnElapsed;
            m_Timer.Dispose();
            m_Timer = null;
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (m_Status is AuthentificationStatus.Authentificated)
            {
                StopTimer();
                Log.Info("Stopping timer, authentificated.");
                return;
            }

            if ((DateTime.Now - m_SentAt).TotalMilliseconds >= 5000)
            {
                m_Status = AuthentificationStatus.Failed;
                FailAuth(AuthentificationFailureReason.TimedOut);
                Log.Error("Stopping timer, authentification failed.");
                StopTimer();
            }
        }
    }
}