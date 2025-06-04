using System;
using System.Reflection;
using System.Diagnostics;

namespace Icheon
{
    public enum LogType { ERROR_LOG, PACKET_LOG, SCAN_LOG };

    public class Logger : IDisposable
    {
        protected FileWriter ErrorLogFile, PacketLogFile, ScanLogFile;
        protected string ErrorLogName = "ErrorLog";
        protected string PacketLogName = "PacketLog";
        protected string ScanLogName = "ScanLog";

        public Logger()
        {
            string path = Assembly.GetEntryAssembly().GetName().Name;
            ErrorLogFile = new FileWriter(path, string.Format(ErrorLogName + "File.txt"), ErrorLogName);
            PacketLogFile = new FileWriter(path, string.Format(PacketLogName + "File.txt"), PacketLogName);
            ScanLogFile = new FileWriter(path, string.Format(ScanLogName + "File.txt"), ScanLogName);
        }
        public Logger(string name)
        {
            string path = Assembly.GetEntryAssembly().GetName().Name;
            ErrorLogFile = new FileWriter(path, string.Format("{0:00}_" + ErrorLogName + "File.txt", name), ErrorLogName);
            PacketLogFile = new FileWriter(path, string.Format("{0:00}_"+ PacketLogName + "File.txt", name), PacketLogName);
            ScanLogFile = new FileWriter(path, string.Format("{0:00}_"+ ScanLogName + "File.txt", name), ScanLogName);
        }

        public virtual void SetLog(LogType type, string msg)
        {
            Debug.WriteLine(string.Format("LogType : {0} => Msg : {1}", type, msg));
            switch (type)
            {
                case LogType.ERROR_LOG:
                    if (ErrorLogFile != null)
                    {
                        ErrorLogFile.WriteLine(msg);
                    }
                    break;
                case LogType.PACKET_LOG:
                    if (PacketLogFile != null)
                    {
                        PacketLogFile.WriteLine(msg);
                    }
                    break;
                case LogType.SCAN_LOG:
                    if (ScanLogFile != null)
                    {
                        ScanLogFile.WriteLine(msg);
                    }
                    break;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                if (ErrorLogFile != null)
                {
                    ErrorLogFile.Close();
                    ErrorLogFile.Dispose();
                    ErrorLogFile = null;
                }
                if (PacketLogFile != null)
                {
                    PacketLogFile.Close();
                    PacketLogFile.Dispose();
                    PacketLogFile = null;
                }
                if (ScanLogFile != null)
                {
                    ScanLogFile.Close();
                    ScanLogFile.Dispose();
                    ScanLogFile = null;
                }

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~Logger() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
