using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Icheon
{
    public class FileWriter : IDisposable
    {
        private ReaderWriterLockSlim WriteLock = new ReaderWriterLockSlim();
        private StreamWriter m_File;
        private string CurrentFileName;
        private DateTime LogLastReceiveTime;
        /// <summary>
        /// 로그파일 최대크기(MB)
        /// </summary>
        private int MaxFileSize = 50;
        public string FileName = "Log_File.txt";
        private string FolderName = "Log";
        private string RootFolderName = "INO";
        private string FolderPath = "";

        private bool IsClosed = false;
        /// <summary>
        /// 기본 폴더(Log)에 기본 파일 이름(KLFC_Debug_file.txt)으로 저장합니다.
        /// 최종 기록되는 파일 이름은 날짜와 기본 파일 이름(KLFC_Debug_file.txt)으로 구성됩니다.
        /// </summary>
        public FileWriter()
        {
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 기본 폴더(Log)에 기본 파일 이름(KLFC_Debug_file.txt)으로 저장합니다.
        /// 최종 기록되는 파일 이름은 날짜와 기본 파일 이름(KLFC_Debug_file.txt)으로 구성됩니다.
        /// 파일 최대크기를 지정하며 최대크기를 넘을 경우 파일을 나누어 저장하며, 파일이름(번호)(예. KLFC_Debug_file(2).txt)로 구성됩니다.
        /// </summary>
        /// <param name="maxFileSize">파일최대크기</param>
        public FileWriter(int maxFileSize)
        {
            MaxFileSize = maxFileSize;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 기본 폴더(Log)에 입력된 파일 이름으로 기록합니다.
        /// 최종 기록되는 파일 이름은 날짜와 입력된 파일 이름으로 구성됩니다.
        /// </summary>
        /// <param name="InFileName">파일 이름</param>
        public FileWriter(string InFileName)
        {
            FileName = InFileName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 기본 폴더(Log)에 입력된 파일 이름으로 기록합니다.
        /// 최종 기록되는 파일 이름은 날짜와 입력된 파일 이름으로 구성됩니다.
        /// 로그파일 최대크기를 지정하며 최대크기를 넘을 경우 파일을 나누어 저장하며, 파일이름(번호)(예. KLFC_Debug_file(2).txt)로 구성됩니다.
        /// </summary>
        /// <param name="InFileName">파일이름</param>
        /// <param name="maxFileSize">파일최대크기</param>
        public FileWriter(string InFileName, int maxFileSize)
        {
            MaxFileSize = maxFileSize;
            FileName = InFileName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 입력된 폴더에 입력된 파일 이름으로 기록합니다.
        /// 최종 기록되는 파일 이름은 날짜와 입력된 파일 이름으로 구성됩니다.
        /// </summary>
        /// <param name="InFileName">파일 이름</param>
        /// <param name="InFolderName">폴더 이름</param>
        public FileWriter(string InFileName, string InFolderName)
        {
            FileName = InFileName;
            FolderName = InFolderName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 입력된 폴더에 입력된 파일 이름으로 기록합니다.
        /// 최종 기록되는 파일 이름은 날짜와 입력된 파일 이름으로 구성됩니다.
        /// </summary>
        /// <param name="InFileName">파일 이름</param>
        /// <param name="InFolderName">폴더 이름</param>
        public FileWriter(string InRootFolderName, string InFileName, string InFolderName)
        {
            FileName = InFileName;
            FolderName = InFolderName;
            RootFolderName = InRootFolderName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        /// <summary>
        /// 입력된 폴더에 입력된 파일 이름으로 기록합니다.
        /// 최종 기록되는 파일 이름은 날짜와 입력된 파일 이름으로 구성됩니다.
        /// 로그파일 최대크기를 지정하며 최대크기를 넘을 경우 파일을 나누어 저장하며, 파일이름(번호)(예. KLFC_Debug_file(2).txt)로 구성됩니다.
        /// </summary>
        /// <param name="InFileName">파일이름</param>
        /// <param name="InFolderName">폴더이름</param>
        /// <param name="maxFileSize">파일최대크기</param>
        public FileWriter(string InFileName, string InFolderName, int maxFileSize)
        {
            MaxFileSize = maxFileSize;
            FileName = InFileName;
            FolderName = InFolderName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), RootFolderName, FolderName);
            LogOn();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            if (!IsClosed)
            {
                LogOff();
            }
            // free native resources
            WriteLock.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region 로그용
        /// <summary>
        /// 로그 기록 시작
        /// </summary>
        private void LogOn()
        {
            WriteLock.EnterWriteLock();
            try
            {
                //로그 기록용 시간
                LogLastReceiveTime = System.DateTime.Now;

                //년월일별 폴더 위치
                //string subFolderPath = Path.Combine(FolderPath, LogLastReceiveTime.ToString("yyyyMMdd"));
                string subFolderPath = FolderPath;

                //일별 폴더 위치
                //subFolderPath = Path.Combine(subFolderPath, LogLastReceiveTime.ToString("dd"));

                //폴더가 없을 시 생성
                FileTools.CreateDirectory(subFolderPath);

                //일별 로깅 파일 생성
                CurrentFileName = LogLastReceiveTime.ToString("yyMMdd") + "_" + FileName;

                #region 최신 파일을 열기
                //일별 폴더 내 파일 목록
                DirectoryInfo dirInfo = new DirectoryInfo(subFolderPath);
                //조건 검색된 파일 목록 얻기
                FileInfo[] files = dirInfo.GetFiles(Path.GetFileNameWithoutExtension(CurrentFileName) + "*" + Path.GetExtension(CurrentFileName));
                //기본(초기) 파일명

                //기본 파일 정보 얻기
                FileInfo Lastfile = new FileInfo(Path.Combine(subFolderPath, CurrentFileName));
                //최신 파일 찾기
                foreach (FileInfo file in files)
                {
                    if (Lastfile.LastWriteTime < file.LastWriteTime)
                    {
                        Lastfile = file;
                    }
                }
                #endregion
                //파일 크기 체크
                CurrentFileName = FileTools.DividLogFile(Lastfile, MaxFileSize);

                //로그 기록 객체 존재여부 확인

                if (m_File != null)
                {
                    m_File.Close();
                    m_File.Dispose();
                }
                m_File = new StreamWriter(Path.Combine(subFolderPath, CurrentFileName), true, System.Text.Encoding.UTF8);
                m_File.AutoFlush = true;
                m_File.WriteLine(string.Format("============================ {0} 프로그램 실행 ============================", DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")));
            }
            catch (Exception e)
            {
                MessageBox.Show("로그 파일을 기록할 수 없습니다.\n 이유 : " + e.Message);
            }
            finally
            {
                WriteLock.ExitWriteLock();
            }
        }

        private void LogOff()
        {
            WriteLock.EnterWriteLock();
            try
            {
                if (m_File != null)
                {
                    m_File.WriteLine(string.Format("============================ {0} 프로그램 종료 ============================", DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")));
                    m_File.Flush();
                    m_File.Close();
                    m_File.Dispose();
                    m_File = null;
                }
                IsClosed = true;
            }
            finally
            {
                WriteLock.ExitWriteLock();
            }
        }

        public void WriteLine(string log)
        {
            WriteLock.EnterWriteLock();
            try
            {
                DateTime ReceivedTime = System.DateTime.Now;
                //월별 폴더 위치
                //string subFolderPath = Path.Combine(FolderPath, ReceivedTime.ToString("yyyy-MM-dd"));
                string subFolderPath = FolderPath;

                //일별 폴더 위치
                //subFolderPath = Path.Combine(subFolderPath, ReceivedTime.ToString("dd"));

                //폴더가 없을 시 생성
                FileTools.CreateDirectory(subFolderPath);

                //일별 로깅 파일 생성
                string fileName = ReceivedTime.ToString("yyMMdd") + "_" + FileName;

                FileInfo file1;
                //날짜 변경 시
                if (LogLastReceiveTime.Day != ReceivedTime.Day)
                {
                    file1 = new FileInfo(Path.Combine(subFolderPath, fileName));
                    CurrentFileName = fileName;
                }
                else
                {
                    file1 = new FileInfo(Path.Combine(subFolderPath, CurrentFileName));
                }

                if (file1.Exists) //기존 파일이 존재할 때
                {
                    if (file1.Length > Constants.MEGABYTE * MaxFileSize)
                    {
                        //파일 크기 체크
                        CurrentFileName = FileTools.GetAvailablePathname(file1.DirectoryName, file1.Name);
                        //로그 기록 객체 존재여부 확인
                        if (m_File != null)
                        {
                            m_File.Close();
                            m_File.Dispose();
                        }
                        m_File = new StreamWriter(Path.Combine(subFolderPath, CurrentFileName), true, System.Text.Encoding.UTF8);
                        m_File.AutoFlush = true;
                    }
                }
                else //파일이 존재하지 않을 때
                {
                    CurrentFileName = FileTools.GetAvailablePathname(subFolderPath, fileName);

                    //로그 기록 객체 존재여부 확인
                    if (m_File != null)
                    {
                        m_File.Close();
                        m_File.Dispose();
                    }
                    m_File = new StreamWriter(Path.Combine(subFolderPath, CurrentFileName), true, System.Text.Encoding.UTF8);
                    m_File.AutoFlush = true;
                }
                if (m_File != null)
                {
                    //m_File.WriteLine(string.Format("{0} : {1}", DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"), log));
                    m_File.WriteLine(log);
                }
                //File.Flush();
                //마지막 로그 기록 시간 갱신
                LogLastReceiveTime = ReceivedTime;
            }
            finally
            {
                WriteLock.ExitWriteLock();
            }
        }

        public void Close()
        {
            LogOff();
        }
        #endregion 로그용 끝
    }
}
