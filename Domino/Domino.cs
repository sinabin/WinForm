using System;
using System.Threading.Tasks;


//
namespace Icheon
{
    // TcpClientParent 클래스를 상속받아 Domino 프린터와의 통신을 구현하는 클래스
    public partial class Domino : TcpClientParent
    {
        private const char CR = (char)0x0D;  // Carriage Return
        private const char LF = (char)0x0A;  // Line Feed
        private int errorCode  = -1;

        private string LastSendText = "";
        public string CurrentPrintText = "";
        public int PrintCount = 0;

        public event EventHandler<byte[]> OnDominoDataReceived;
        public event EventHandler<string> OnDominoError;
        public event EventHandler OnDominoPrintReady;
        public event EventHandler OnDominoPrintStart;
        public event EventHandler OnDominoPrintEnd;


        public Domino() : base() { }

        public async Task<bool> PrintStart(string projectName, string textName)
        {
            LastSendText = "";
            CurrentPrintText = "";
            PrintCount = 0;

            if (await SetMsgAsync("2", "1") != 0) { return false; }
            if (await SetMsgAsync("3", "1") != 0) { return false ; }
            if (await SetMsgAsync("26", "1") != 0) { return false ; }
            if (await LoadProjectAsync(projectName) != 0) { return false ; }
            if (await MarkAsync("START") != 0) { return false ; }
            if (await SetTextAsync(textName, "") != 0) { return false ; }

            return true;
        }

        public async Task<bool> PrintStop()
        {
            if (await MarkAsync("START") != 0) { return false; }
            return true;
        }


        public async Task<int> LoadProjectAsync(string projectName)
        {
            if (IsConnected)
            {
                errorCode = -1;
                SendData("LOADPROJECT store:/" + projectName + CR + LF);
                await GetErrorCodeAsync();

                return errorCode;
            }

            return -1;
        }
        public async Task<int> MarkAsync(string action)
        {
            if (IsConnected)
            {
                errorCode = -1;
                SendData("MARK" + " " + action + CR + LF);
                await GetErrorCodeAsync();

                return errorCode;
            }

            return -1;
        }

        public async Task<int> SetMsgAsync(string MsgID, string Mode)
        {
            if (IsConnected)
            {
                errorCode = -1;
                SendData("SETMSG" + " " + MsgID + " " + Mode + CR + LF);
                await GetErrorCodeAsync();

                return errorCode;
            }

            return -1;
        }

        public async Task<int> SetTextAsync(string textName, string textValue)
        {
            if (IsConnected)
            {
                LastSendText = textValue;

                errorCode = -1;
                SendData("SETTEXT \"" + textName + "\" \"" + textValue + "\"" + CR + LF);
                await GetErrorCodeAsync();  

                return errorCode;
            }

            return -1;
        }

        private async Task GetErrorCodeAsync()
        {
            for (int i = 0; i < 500; i++)
            {
                if (errorCode > -1)
                {
                    return;
                }

                await Task.Delay(10);  
            }

            OnDominoError?.Invoke(this, $"Error {errorCode}: No Response");
        }

        protected override void OnReceiveData(byte[] data)
        {
            base.OnReceiveData(data);
            //OnDominoDataReceived?.Invoke(this, data);
            ProcessReceivedData(data);
        }

        private void ProcessReceivedData(byte[] data)
        {
            bool isOK = data[0] == 0x4F && data[1] == 0x4B;
            bool isError = data[0] == 0x45 && data[1] == 0x52;
            bool isMSG = data[0] == 0x4D && data[1] == 0x53 && data[2] == 0x47;


            if (isOK)
            {
                errorCode = 0;
            }
            else if (isError)
            {
                errorCode = (int)data[6];
                OnDominoError?.Invoke(this, $"Error {errorCode}: {Domino.GetErrorMessage(errorCode)}");
            }
            else if (isMSG) 
            {
                if (data[4] == 0x32 && data[5] == 0x36 && data[7] == 0x31)
                {
                    CurrentPrintText = LastSendText;
                    OnDominoPrintReady?.Invoke(this, EventArgs.Empty);
                }
                else if (data[4] == 0x32 && data[6] == 0x31)
                {
                    OnDominoPrintStart?.Invoke(this, EventArgs.Empty);
                }
                else if (data[4] == 0x33 && data[6] == 0x31)
                {
                    PrintCount++;
                    OnDominoPrintEnd?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
