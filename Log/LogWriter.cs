using System.Net;

namespace Icheon
{
    public class LogWriter
    {
        protected Logger logger;
        public virtual void SetLogger(Logger log)
        {
            logger = log;
        }

        public void WriteErrorLog(string where, string what, string message)
        {
            if(logger != null) logger.SetLog(LogType.ERROR_LOG, string.Format("{0} Exception, 에러 요소 : {1}, 에러 메세지 : {2}",
                where, what, message));
        }
        public void WriteErrorLog(string message)
        {
            if (logger != null) logger.SetLog(LogType.ERROR_LOG, message);
        }
        public void WriteScanLog(string message)
        {
            if (logger != null) logger.SetLog(LogType.SCAN_LOG, message);
        }
        public void WritePacketLog(string what, IPEndPoint ip, string from, string to, string packet)
        {
            if (logger != null) logger.SetLog(LogType.PACKET_LOG, string.Format("[{0}] {1} ({2}->{3}) : {4}", ip, what, from, to, packet));
        }
        public void WritePacketLog(string message)
        {
            if (logger != null) logger.SetLog(LogType.PACKET_LOG, message);
        }
    }
}
