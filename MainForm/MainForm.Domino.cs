//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//! @file   MainForm.Domino.cs
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Icheon
{
    public partial class MainForm : Form
    {
        protected Domino _pBlackDomino = null, _pTransparentDomino = null;
        protected const string BlackDominoFormat = @"흑백 도미노 : {0}:{1}({2})", TransparentDominoFormat = @"투명 도미노 : {0}:{1}({2})";
        protected bool _bBalckDominoConnect = false, _bTransparentDominoConnect = false;

        protected bool _bBlackDominoPrintAckFlag = false;
        protected bool _bBlackDominoUrcode = false;
        protected bool _bBlackDominoContent = false;
        protected bool _bBlackDominoPrintReady = false;

        protected bool _bTransparentDominoPrintAckFlag = false;
        protected bool _bTransparentDominoContent = false;
        protected bool _bTransparentDominoPrintReady = false;

        protected int _iBlackDominoInitSerial = 0, _iTransparentDominoInitSerial = 0;
        protected string _srBlackDominoInitContent = string.Empty, _srTransparentDominoInitContent = string.Empty;
        
        protected string _srBlackDominoCsvFormat = string.Empty;
        protected int _iBlackPrint = 0;
        protected List<string> _lsBlackPrint = new List<string>();

        protected int _iBlackPrintRpm = 0;
        protected DateTime _dtBlackPrintTime;
        
        //ProductionLine(생산라인번호) 및 RegionCode(지역코드)
        string productionLine = ConfigurationManager.AppSettings["ProductionLine"];
        string regionCode = ConfigurationManager.AppSettings["RegionCode"];
        string RegionCodeToKr = ConfigurationManager.AppSettings["RegionCodeToKr"];
        

        // call _OnLoad()
        protected void _ConnectBlackDomino()
        {
            lbBlackDomino.BackColor = Color.Black;
            lbBlackDomino.ForeColor = Color.White;
            lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 중");

            // 흑백 도미노 연결 스레드.
            Thread bdominothread = new Thread(() =>
            {
                if (null != _pBlackDomino)
                    _pBlackDomino.Connect(Domino.BLACK_IP, Domino.BLACK_PORT);
            });
            bdominothread.Start();
        }

        //
        protected void _CloseDomino()
        {
            if (null != _pBlackDomino && _pBlackDomino.IsConnected)
                _pBlackDomino.Disconnect();
            if (null != _pTransparentDomino && _pTransparentDomino.IsConnected)
                _pTransparentDomino.Disconnect();
        }

        //
        protected void _SendPrintAckFlag()
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);
            string s = esc + "I1Z" + eot;

            //
            if (null != _pBlackDomino && _pBlackDomino.IsConnected)
                _pBlackDomino.SendData(s);
            if (null != _pTransparentDomino && _pTransparentDomino.IsConnected)
                _pTransparentDomino.SendData(s);
        }

        // PrintContent 메소드가 쓰이는 곳이 단 한군데도 없는데?
        protected void 
            PrintContent()
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);
            string s = esc + "S" +// command
                "001" +// lableName
                " " +
                eot;

            //
            _srBlackDominoInitContent = s;
            if (_pBlackDomino != null && _pBlackDomino.IsConnected)
                _pBlackDomino.SendData(s);
        }

#if false

        //
        protected void _SendBlackPrintContent(int initserial, string use, string liter)
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);

            // SerialCommand
            ++initserial;// 시작 시리얼.
            string srInitSerial = "AA" + initserial.ToString("D6");
            string serialcommand = esc + "j" +// command
                "1" +// serial number id
                "N" +// batch linked
                "06" +// numeric field width
                "000000" +// first number limit
                "999999" + // last number limit
                "000001" +// numeric step size
                "Y" +// print leading zeros
                "P" +// prefix/suffix select
                "2" +// number of alpha prefix/suffix characters
                "AA" +// first alpha limit
                "ZZ" +// second alpha limit 
                srInitSerial +// start value. size of numeric + alpha field width
                "00000" +// number of times to repeat each number
                "A";// increment alpha or numeric

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
                "1" + "36" + // printerline + 지역번호(이천시)
                srUse + // 용도(일반)
                serialcommand +

                // barcode
                esc + "q8" +// command
                serialcommand +
                esc + "q0" +

                // ur text
                " " +
                //" UR " +

                // urcode.bmp
                esc + "x2UR" + " " +

                //
                //esc + "x2" + "01" + " " +// 이천시.
                esc + "x2" + "01" +// 이천시.

                //esc + "x2" + srUse + " " +// 용도(일반)
                esc + "x2" + srUse +// 용도(일반)
                liter + "L" +// liter

                eot;

            _srBlackDominoInitContent = today +
                " 136" + srUse + srInitSerial +
                " UR urcode.bmp 이천시 " +
                use + " " + liter + "L";

            //
            _srBlackDominoCsvFormat = "{0}," + today + "," +
                "1,이천시," + use + "," + liter + ",136" + srUse + "{1}";

            //
            if (null != _pBlackDomino && _pBlackDomino.IsConnected)
                _pBlackDomino.SendData(req);
        }


#else

        protected void _SendBlackPrintContent(long nextSerial, string use, string liter)
        {
            try
            {
                // ESC, EOT 등의 특수문자 변환
                string esc = Convert.ToString((char)0x1b);
                string eot = Convert.ToString((char)0x04);

                // SerialCommand (9자리)
                string srInitSerial = nextSerial.ToString("D9");
                string serialcommand = esc + "j" + // command
                                       "1" + // serial number id
                                       "N" + // batch linked
                                       "09" + // numeric field width
                                       "000000000" + // first number limit
                                       "999999999" + // last number limit
                                       "000000001" + // numeric step size
                                       "Y" + // print leading zeros
                                       "N" + // prefix/suffix select
                                       "0" + // number of alpha prefix/suffix characters
                                       srInitSerial + // start value (size of numeric + alpha field width)
                                       "00000" + // number of times to repeat each number
                                       "N" + // increment alpha or numeric
                                       " ";

                DateTime now = DateTime.Now;
                string today = (now.Year % 2000).ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");

                // 용도 값에 따른 코드 할당
                string srUse = ""; //진짜 string.empty는 고의적인건지?.....
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
                    case "폐기":
                        srUse = "05";
                        break;
                    default:
                        throw new ArgumentException("올바르지 않은 용도 값: " + use);
                }

                // req 문자열 구성 (문자열 결합 부분이 많으므로 필요한 경우 한 줄씩 나누어서 디버깅하기 좋게 구성)
                string req = esc + "S" + // command S
                             "001" + // 라벨명 (3자리)
                             today + // Date
                             regionCode + // regionCode: 클래스 또는 전역변수로 정의되어 있어야 함.
                             productionLine + // productionLine: 클래스 또는 전역변수로 정의되어 있어야 함.
                             serialcommand + // serial command 부분
                             esc + "q8" + // barcode 시작 command
                             regionCode +
                             productionLine +
                             serialcommand +
                             esc + "q0" + // barcode 끝 command
                             " UR " + // UR 텍스트 지정
                             esc + "x2UR" + " " + // 이미지 파일 urcode.bmp 관련 커맨드
                             esc + "x2" + "01" + // 예: 이천시 (고정 문자열 혹은 변수에 따라 달라질 수 있음)
                             esc + "x2" + srUse + // 용도 (일반, 음식물, 재사용 등)
                             liter + "L" + // liter 값 및 접미사 L
                             eot; // 전송 종료

                // 출력 초기화 문자열 (_srBlackDominoInitContent) 구성
                _srBlackDominoInitContent = today +
                                            srInitSerial +
                                            " UR urcode.bmp 이천시 " +
                                            use + " " +
                                            liter + "L";

                // CSV 포맷 문자열 구성
                _srBlackDominoCsvFormat = "{0}," + today + "," +
                                          "1,이천시," + use + "," + liter + "," + regionCode + productionLine + "{1}";

                // _pBlackDomino 객체가 null이 아니면서 연결 상태인 경우 데이터 전송
                if (_pBlackDomino != null && _pBlackDomino.IsConnected)
                {
                    _pBlackDomino.SendData(req);
                }
                else
                {
                    // 프린터 연결이 되어 있지 않을 경우 예외 처리
                    throw new InvalidOperationException("Black Domino 프린터가 연결되어 있지 않습니다.");
                }
            }
            catch (ArgumentException argEx)
            {
                System.Diagnostics.Debug.WriteLine("Parameter Error in _SendBlackPrintContent: " + argEx.ToString());
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                System.Diagnostics.Debug.WriteLine("Operation Error in _SendBlackPrintContent: " + invOpEx.ToString());
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unexpected Error in _SendBlackPrintContent: " + ex.ToString());
                throw;
            }
        }

#endif

        //
        protected void _SendPrintReady()
        {
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);
            string s = esc + "P1" +// command
                "001" +// lableName
                eot;

            //
            if (null != _pBlackDomino && _pBlackDomino.IsConnected)
                _pBlackDomino.SendData(s);
            if (null != _pTransparentDomino && _pTransparentDomino.IsConnected)
                _pTransparentDomino.SendData(s);
        }

        //
        protected void _OnBlackDominoConnected()
        {
            _bBalckDominoConnect = true;
            if (lbBlackDomino.InvokeRequired)
                lbBlackDomino.Invoke(new Action(() => lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 완료")));
            else
                lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 완료");
            _Cwl("_OnBlackDominoConnected");

            // PrintAckFlag
            _SendPrintAckFlag();
        }

        //
        protected void _OnBlackDominoConnectFail(string error)
        {
            _bBalckDominoConnect = false;
            if (lbBlackDomino.InvokeRequired)
                lbBlackDomino.Invoke(new Action(() => lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 실패")));
            else
                lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 실패");
            _Cwl("_OnBlackDominoConnectFail : " + error);
        }

        //
        protected void _OnBlackDominoDisconnected()
        {
            // 검정 프린터 연결 종료 시, 플래그 초기화
            _bBalckDominoConnect = false;
            _bBlackDominoPrintReady = false; 
            _bBlackDominoPrintAckFlag = false;
            _bBlackDominoUrcode = false;
            if (lbBlackDomino.InvokeRequired)
                lbBlackDomino.Invoke(new Action(() => lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 종료")));
            else
                lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"연결 종료");
            _Cwl("_OnBlackDominoDisconnected");
        }

        protected void _OnBlackDominoDataReceived(byte[] ba)
        {
            if (1 == ba.Length)
                _Cwl(@"_OnBlackDominoDataReceived : " + ba[0].ToString());
            else
            {
                for (int y = 0; y < ba.Length; ++y)
                    _Cwl(string.Format(@"_OnBlackDominoDataReceived() ba[{0}] : {1}", y, ba[y].ToString()));
            }

            //
            if (6 == ba[0] || ba[0] == 54)// PrintAckFlag Success (54는 테스트용 임시)
            {
                if (!_bBlackDominoPrintAckFlag)
                {
                    _bBlackDominoPrintAckFlag = true;
                    _SendUrcodeForEachDay();
                }
                else if (!_bBlackDominoUrcode)
                {
                    _bBlackDominoUrcode = true;
                    _bBlackDominoContent = true;
                    _SendPrintReady();
                }
                else if (!_bBlackDominoPrintReady)
                {
                    _bBlackDominoPrintReady = true;
                    _dtBlackPrintTime = DateTime.UtcNow;
                    // PrintReady 상태가 되었으므로 UI에 "준비 완료" 표시
                    if (lbBlackDomino.InvokeRequired) // 현재 스레드와 컨트롤이 생성된 스레드가 다르면 InvokeRequired가 true를 반환 : 다른 스레드에서 컨트롤에 접근할 때 발생할 수 있는 문제를 예방하기 위해 사용
                        lbBlackDomino.Invoke(new Action(() => lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"준비 완료")));
                    else
                        lbBlackDomino.Text = string.Format(BlackDominoFormat, Domino.BLACK_IP, Domino.BLACK_PORT, @"준비 완료");
                }
            }
            else if (0x21 == ba[0])// Error
            {
                int error = 0;
            }
            else if (0x5A == ba[0]) //  대문자 Z의 코드 값
            {
                if (!_bBlackDominoPrintReady)// || _iOrder <= 0)
                    return;

                ++_iBlackPrintRpm;
                long elpased = DateTime.UtcNow.Ticks - _dtBlackPrintTime.Ticks;
                TimeSpan span = new TimeSpan(elpased);
                _Cwl("mise " + span.TotalMilliseconds);
                if (span.TotalMilliseconds >= 1000)
                {
                    if (lbOutput.InvokeRequired)
                        lbOutput.Invoke(new Action(() => lbOutput.Text = string.Format("생산속도: 초당 {0}장", _iBlackPrintRpm)));
                    else
                        lbOutput.Text = string.Format("생산속도: 초당 {0}장", _iBlackPrintRpm);
                    _Cwl("_iBlackPrintRpm : " + _iBlackPrintRpm);

                    _dtBlackPrintTime = DateTime.UtcNow;
                    _iBlackPrintRpm = 0;
                }

                DateTime now = DateTime.Now;
                string today = (now.Year % 2000).ToString() + now.Month.ToString("D2") + now.Day.ToString("D2");

                ++_nextSerial;
                nud.Invoke(new Action(() => nud.Value = _nextSerial));
                _dicStatus[_srNowKey] = ++_iStatus;
                ++_totalStatus;
                _UpdateGeneralInfo();

                if (_iStatus == _iOrder)
                {
                    string filename = string.Format(@".\{0}L,{1}({2}).csv", _srNowKey, _iOrder, today);// 음식물,1L,70000(250331).csv
                    FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    foreach (string ss in _lsBlackPrint)
                        sw.WriteLine(ss);
                    _iBlackPrint = 0;
                    sw.Close();
                    fs.Close();
                }

                if (!_dicStatus.TryGetValue(_srNowKey, out int initserial))
                {
                }
                string s = string.Format(_srBlackDominoCsvFormat, _iStatus, (_nextSerial-1).ToString("D9"));
                _lsBlackPrint.Add(s);
                _AddOutputListView(s);
                _UpdateWorkListView(_iStatus);
                
                // DB Insert 및 update 를 비동기적으로 수행
                Task.Run(async () =>
                {
                    // _srNowKey의 값을 분리하여 orderId, usage, literStr 변수에 할당합니다.
                    string[] keyParts = _srNowKey.Split(',');
                    string orderId = keyParts.Length > 0 ? keyParts[0] : "";
                    string usage = keyParts.Length > 1 ? keyParts[1] : "";
                    string literStr = keyParts.Length > 2 ? keyParts[2] : "";
                    
                    int parsedOrderId = 0;
                    if(!int.TryParse(orderId, out parsedOrderId))
                    {
                        parsedOrderId = 0;
                    }
                    
                    // 신규 CSV 문자열 구성
                    // 원하는 CSV 형식: "order_number,prd_date,prd_line,order_place,usage,liter,serial"
                    string prd_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");                  // 생산일시
                    string prd_line = ConfigurationManager.AppSettings["ProductionLine"];             // 생산라인(프린터 번호)
                    string[] sParts = s.Split(',');
                    string serialValue = "";
                    if (sParts.Length >= 7)
                    {
                        serialValue = sParts[6]; // 원래 s의 마지막 필드 (serial) 값 추출
                    }
                    else
                    {
                        // 필드 개수가 부족할 경우 기본값 처리
                        serialValue = (_nextSerial - 1).ToString("D9");
                    }
                    
                    string sModified = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                        orderId, prd_date, prd_line, RegionCodeToKr, usage, literStr, serialValue);

                    // 기존 InsertOutputDataAsync(s) 수행
                    await _dbManager.InsertOutputDataAsync(sModified);
                    
                    // 현재 생산된 마지막 시리얼 번호 Update
                    string currentSerial = regionCode + productionLine + (_nextSerial - 1).ToString("D9");
                    await _dbManager.UpdateWorkPlanningEndSerialAsync(currentSerial, parsedOrderId, usage, literStr);
                    
                });
                
                Console.WriteLine("s: " + s);
                
                if (totalProducedCount >= _totalOrder) // 전라인 통합생산량 >= 발주량
                {
                    _SendBlackPrintContent(0, "폐기", "");
                    _SendTransparentPrintContent(0, "폐기", "");
                    Console.WriteLine("폐기 명령이 전송되었습니다.");
                    return;
                }
            }
        }

        //
        protected void _OnBlackDominoTcpError(string error)
        {
            _Cwl(@"_OnBlackDominoTcpError : " + error);
        }

        //
        protected void _OnBlackDominoError(string error)
        {
            _Cwl(@"_OnBlackDominoTcpError : " + error);
        }


    }// public partial class MainForm : Form
}// namespace Icheon
