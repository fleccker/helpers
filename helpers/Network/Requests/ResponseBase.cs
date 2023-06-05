using helpers.Network.Extensions.Data;

using System;
using System.IO;

namespace helpers.Network.Requests
{
    public class ResponseBase : IResponse
    {
        private bool m_IsSuccess;
        private string m_RequestId;
        private object m_Response;

        private DateTime m_SentAt;
        private DateTime m_ReceivedAt;

        public bool IsSuccess => m_IsSuccess;
        public string RequestId => m_RequestId;
        public object Response => m_Response;

        public DateTime SentAt => m_SentAt;
        public DateTime ReceivedAt => m_ReceivedAt;

        public ResponseBase() { }
        public ResponseBase(bool success, string id, object response)
        {
            m_IsSuccess = success;
            m_RequestId = id;
            m_Response = response;
        }

        public virtual void Read(BinaryReader reader)
        {
            m_IsSuccess = reader.ReadBoolean();
            m_RequestId = reader.ReadString();
            m_Response = reader.ReadObject();
            m_SentAt = reader.ReadDateTime();
            m_ReceivedAt = DateTime.Now;
        }

        public virtual void Write(BinaryWriter writer)
        {
            m_SentAt = DateTime.Now;

            writer.Write(m_IsSuccess);
            writer.Write(m_RequestId);
            writer.WriteObject(m_Response);
            writer.Write(m_SentAt);
        }
    }
}