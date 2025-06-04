//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//! @file   MainForm.Transparent.cs
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Threading;


//
namespace Icheon
{
    public partial class MainForm : Form
    {
        // call _OnLoad()
        protected void _ConnectTransparentDomino()
        {
            lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 중");

            // 투명 도미노 연결 스레드.
            Thread tdominothread = new Thread(() =>
            {
                if (null != _pTransparentDomino)
                    _pTransparentDomino.Connect(Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT);
            });
            tdominothread.Start();
        }

        //
        protected void _SendTransparentPrintContent()
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);
            string s = esc + "S" +// command
                "001" +// lableName
                " " +
                eot;

            //
            _srTransparentDominoInitContent = s;
            if (_pTransparentDomino != null && _pTransparentDomino.IsConnected)
                _pTransparentDomino.SendData(s);
        }

        //
        protected void _SendTransparentPrintContent(long nextSerial, string use, string liter)
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);

            // SerialCommand
            string srInitSerial = nextSerial.ToString("D9");
            string serialcommand = esc + "j" +// command
                "1" +// serial number id
                "N" +// batch linked
                "09" +// numeric field width
                "000000000" +// first number limit
                "999999999" + // last number limit
                "000000001" +// numeric step size
                "Y" +// print leading zeros
                "N" +// prefix/suffix select
                "0" +// number of alpha prefix/suffix characters
                srInitSerial +// start value. size of numeric + alpha field width
                "00000" +// number of times to repeat each number
                "N" +// increment alpha or numeric
                " ";

            //
            DateTime now = DateTime.Now;
            string today = (now.Year % 2000).ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");

            //
            string srUse = string.Empty;
            switch (use)
            {
                case "일반":
                    srUse = "02";
                    break;

                case "음식물":
                    srUse = "03";
                    break;

                case "재사용":
                    srUse = "04";
                    break;
            }

            //
            string req = esc + "S" +// command
                "001" +// lableName
                //today + " " +// Date + " "
                today +// Date + " "
                "1" +// printerline
                " UR " +
                //"36" + // printerline + 지역번호(이천시)
                //srUse + // 용도(일반)
                //serialcommand +

                //
                //esc + "x2" + "01" + " " +// 이천시.
                //esc + "x2" + srUse + " " +// 용도(일반)
                esc + "x2" + "01" +// 이천시.
                esc + "x2" + srUse +// 용도(일반)
                liter + "L" +// liter

                //
                eot;

            //
            _srTransparentDominoInitContent = today + " 136" + srUse + srInitSerial + " 이천시 " + use + " " + liter + "L";
            if (_pTransparentDomino != null && _pTransparentDomino.IsConnected)
                _pTransparentDomino.SendData(req);
        }

        protected void _OnTransparentDominoConnected()
        {
            _bTransparentDominoConnect = true;
            if (lbTransparentDomino.InvokeRequired)
                lbTransparentDomino.Invoke(new Action(() => lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 완료")));
            else
                lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 완료");
            _Cwl("_OnTransparentDominoConnected");

            // PrintAckFlag
            _SendPrintAckFlag();
        }

        //
        protected void _OnTransparentDominoConnectFail(string error)
        {
            _bTransparentDominoConnect = false;
            if (lbTransparentDomino.InvokeRequired)
                lbTransparentDomino.Invoke(new Action(() => lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 실패")));
            else
                lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 실패");
            _Cwl("_OnTransparentDominoConnectFail : " + error);
        }

        //
        protected void _OnTransparentDominoDisconnected()
        {
            // 투명 프린터 연결 종료 시, 플래그 초기화
            _bTransparentDominoConnect = false;
            _bTransparentDominoPrintReady = false;
            _bTransparentDominoPrintAckFlag = false;
            _bTransparentDominoContent = false;
            if (lbTransparentDomino.InvokeRequired)
                lbTransparentDomino.Invoke(new Action(() => lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 종료")));
            else
                lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"연결 종료");
            _Cwl("_OnTransparentDominoDisconnected");
        }

        //
        protected void _OnTransparentDominoDataReceived(byte[] ba)
        {
            _Cwl(string.Format(@"_OnTransparentDominoDataReceived : " + ba[0].ToString()));
            if (6 == ba[0] || ba[0] == 54)// PrintAckFlag Success (54는 테스트용 임시)
            {
                if (!_bTransparentDominoPrintAckFlag)
                    _bTransparentDominoPrintAckFlag = true;
                else if (!_bTransparentDominoContent)
                {
                    _bTransparentDominoContent = true;
                    _SendPrintReady();
                }
                else if (!_bTransparentDominoPrintReady)
                {
                    _bTransparentDominoPrintReady = true;
                    _Cwl("Transparent Printer Status: PrintReady");
                    // PrintReady 상태가 되었으므로 투명 프린터 UI도 "준비 완료"로 변경
                    if (lbTransparentDomino.InvokeRequired) // 현재 스레드와 컨트롤이 생성된 스레드가 다르면 InvokeRequired가 true를 반환 : 다른 스레드에서 컨트롤에 접근할 때 발생할 수 있는 문제를 예방하기 위해 사용
                    {
                        lbTransparentDomino.Invoke(new Action(() =>
                            lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"준비 완료")
                        ));
                    }
                    else
                    {
                        lbTransparentDomino.Text = string.Format(TransparentDominoFormat, Domino.TRANSPARENT_IP, Domino.TRANSPARENT_PORT, @"준비 완료");
                    }
                }
            }
            else if (0x21 == ba[0])// Error
            {
                int error = 0;
            }
            else if (0x5A == ba[0])
            {
                if (!_bTransparentDominoPrintReady)
                    return;
            }
        }

        //
        protected void _OnTransparentDominoTcpError(string error)
        {
            _Cwl(@"_OnTransparentDominoTcpError : " + error);
        }

        //
        protected void _OnTransparentDominoError(string error)
        {
            _Cwl(@"_OnTransparentDominoError : " + error);
        }


    }// public partial class MainForm : Form
}// namespace Icheon
