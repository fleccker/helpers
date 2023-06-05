using helpers.Network.Extensions.Data;
using helpers.Network.Features;

using System;
using System.IO;

namespace helpers.Network.Requests
{
    public class RequestBase : IRequest
    {
        private string m_Id;
        private object m_Request;

        private DateTime m_SentAt;
        private DateTime m_ReceivedAt;

        private INetworkObject m_Acceptor;

        public string Id => m_Id;
        public object Request => m_Request;

        public bool IsDeclined => m_Acceptor is null;

        public INetworkObject AcceptedBy => m_Acceptor;

        public DateTime SentAt => m_SentAt;
        public DateTime ReceivedAt => m_ReceivedAt;

        public RequestBase() { }
        public RequestBase(string id, object request)
        {
            m_Id = id;
            m_Request = request;
        }

        public void Accept(INetworkObject networkObject)
        {
            if (m_Acceptor != null) 
                return;

            m_Acceptor = networkObject;
        }

        public virtual void Read(BinaryReader reader)
        {
            m_ReceivedAt = DateTime.Now;

            m_Id = reader.ReadString();
            m_Request = reader.ReadObject();
            m_SentAt = reader.ReadDateTime();
        }

        public virtual void Write(BinaryWriter writer)
        {
            m_SentAt = DateTime.Now;

            writer.Write(m_Id);
            writer.WriteObject(m_Request);
            writer.Write(m_SentAt);
        }

        public void RespondFail(object response = null)
        {
            if (IsDeclined) return;

            m_Acceptor.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.Send(response, this, false);
            });
        }

        public void RespondSuccess(object response)
        {
            if (IsDeclined) return;

            m_Acceptor.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.Send(response, this, true);
            });
        }

        public void Decline()
        {
            m_Acceptor = null;
        }
    }
}