using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;


namespace Icheon
{
    // .NET Framework의 TcpClient를 기반으로 한 커스텀 TCP 클라이언트 클래스
    //  TCP 연결을 생성, 유지, 종료하는 기본 기능과, 데이터를 송수신하는 기능,
    // 그리고 관련 이벤트(연결, 연결 실패, 데이터 수신, 오류 등)를 정의하고 실행하는 기능을 제공
    public class TcpClientParent : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private bool _isRunning;
        private string _ip;
        private int _port;
        private CancellationTokenSource _cancellationTokenSource;
        private System.Timers.Timer connectionCheckTimer;

        //
        public event EventHandler OnConnected;
        public event EventHandler<string> OnConnectFail;
        public event EventHandler OnDisconnected;
        public event EventHandler<byte[]> OnDataReceived;
        public event EventHandler<string> OnTcpError;

        //
        public bool IsConnected => _client != null && _client.Connected;


        //
        public bool Connect(string ip, int port)
        {
            if (IsConnected)
                return true;

            //
            try
            {
                _ip = ip;
                _port = port;

                _client = new TcpClient(_ip, _port);
                _stream = _client.GetStream();
                _isRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;

                _receiveThread = new Thread(() => ReceiveData(token)) { IsBackground = true };
                _receiveThread.Start();

                OnConnected?.Invoke(this, EventArgs.Empty);
                StartConnectionCheck();

                //
                return true;
            }
            catch (Exception ex)
            {
                OnConnectFail?.Invoke(this, $"연결 실패: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            //
            StopConnectionCheck();
            Task.Run(() =>
            {
                try
                {
                    _isRunning = false;

                    // 수신 스레드 취소
                    _cancellationTokenSource?.Cancel();

                    // 스트림 및 클라이언트 닫기
                    _stream?.Close();
                    _client?.Close();

                    // 수신 스레드 종료 대기
                    _receiveThread?.Join();

                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnTcpError?.Invoke(this, $"Disconnect 중 오류 발생: {ex.Message}");
                }
            });
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 이벤트 핸들러 제거
                    foreach (Delegate d in OnDataReceived?.GetInvocationList() ?? Array.Empty<Delegate>())
                    {
                        OnDataReceived -= (EventHandler<byte[]>)d;
                    }

                    // TCP 리소스 해제
                    _client?.Dispose();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendData(string message)
        {
            if (!IsConnected)
            {
                OnTcpError?.Invoke(this, "연결되지 않음. 데이터를 보낼 수 없습니다.");
                return;
            }

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                OnTcpError?.Invoke(this, $"데이터 전송 오류: {ex.Message}");
            }
        }
        public void SendData(byte[] data, int count)
        {
            if (!IsConnected)
            {
                OnTcpError?.Invoke(this, "연결되지 않음. 데이터를 보낼 수 없습니다.");
                return;
            }

            try
            {
                _stream.Write(data, 0, count);
            }
            catch (Exception ex)
            {
                OnTcpError?.Invoke(this, $"데이터 전송 오류: {ex.Message}");
            }
        }

        private void ReceiveData(CancellationToken token)
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (_isRunning && !token.IsCancellationRequested)
                {
                    if (_client?.Connected != true)
                    {
                        //OnDisconnected?.Invoke(this, EventArgs.Empty);
                        break;
                    }

                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            //OnDisconnected?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                        
                        byte[] received = new byte[bytesRead];
                        Array.Copy(buffer, 0, received, 0, bytesRead);

                        OnReceiveData(received);
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                OnTcpError?.Invoke(this, $"데이터 수신 오류: {ex.Message}");                
            }
        }

        protected virtual void OnReceiveData(byte[] data)
        {
            OnDataReceived?.Invoke(this, data);
        }
        public void GetDelegateCount(string msg)
        {
            Console.WriteLine(msg + " 델리게이트 개수: " + OnDataReceived?.GetInvocationList().Count());
        }

        private void StartConnectionCheck()
        {
            connectionCheckTimer = new System.Timers.Timer(1000); // 1초 간격으로 상태 확인
            connectionCheckTimer.Elapsed += CheckConnectionStatus;
            connectionCheckTimer.AutoReset = true;
            connectionCheckTimer.Start();
        }

        private void StopConnectionCheck()
        {
            if (connectionCheckTimer != null)
            {
                connectionCheckTimer.Stop();
                connectionCheckTimer.Dispose();
                connectionCheckTimer = null;
            }
        }

        private void CheckConnectionStatus(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_client == null || _client.Client == null || !_client.Client.Connected)
                {
                    // 연결이 끊어진 경우
                    StopConnectionCheck();
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                    return;
                }

                // 소켓 상태 확인 (Poll 및 Available 사용)
                if (_client.Client.Poll(1000, SelectMode.SelectRead) && _client.Client.Available == 0)
                {
                    StopConnectionCheck();
                    _client.Close();
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                StopConnectionCheck();
                OnTcpError?.Invoke(this, ex.Message);
            }
        }

    }
}
