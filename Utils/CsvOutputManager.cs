using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace Icheon
{
    /// <summary>
    /// CSV 데이터를 비동기적으로 파일에 기록하는 헬퍼 클래스
    /// 지정된 간격마다 버퍼에 쌓인 데이터를 파일에 플러시
    /// </summary>
    public class CsvOutputManager
    {
        private readonly ConcurrentQueue<string> _outputQueue = new ConcurrentQueue<string>();
        private readonly Timer _flushTimer;
        private readonly string _fileNameFormat;

        /// <summary>
        /// CsvOutputManager 생성자.
        /// </summary>
        /// <param name="flushIntervalInMs">플러시 주기 (밀리초)</param>
        /// <param name="fileNameFormat">
        /// CSV 파일 이름 포맷. {0} 자리에 날짜가 들어감 기본값: @".\Outputing({0}).csv"
        /// </param>
        public CsvOutputManager(double flushIntervalInMs, string fileNameFormat = @".\Outputing({0}).csv")
        {
            _fileNameFormat = fileNameFormat;
            _flushTimer = new Timer(flushIntervalInMs);
            _flushTimer.Elapsed += (sender, e) => FlushOutputQueue();
            _flushTimer.AutoReset = true;
        }

        /// <summary>
        /// 플러시 타이머를 시작합니다.
        /// </summary>
        public void Start()
        {
            _flushTimer.Start();
        }

        /// <summary>
        /// 플러시 타이머를 정지
        /// </summary>
        public void Stop()
        {
            _flushTimer.Stop();
        }

        /// <summary>
        /// CSV 내용을 버퍼에 추가
        /// </summary>
        /// <param name="line">CSV 형식의 한 줄 데이터</param>
        public void Enqueue(string line)
        {
            _outputQueue.Enqueue(line);
        }

        /// <summary>
        /// 버퍼에 쌓인 데이터를 파일에 기록
        /// </summary>
        public void FlushOutputQueue()
        {
            try
            {
                DateTime now = DateTime.Now;
                string today = (now.Year % 2000).ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");
                string filePath = string.Format(_fileNameFormat, today);

                List<string> lines = new List<string>();
                while (_outputQueue.TryDequeue(out string line))
                {
                    lines.Add(line);
                }
                if (lines.Count > 0)
                {
                    File.AppendAllLines(filePath, lines, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // 필요에 따라 별도의 로깅 처리
                Console.WriteLine("FlushOutputQueue error: " + ex.Message);
            }
        }
    }
}
