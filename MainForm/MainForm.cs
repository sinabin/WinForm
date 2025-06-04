using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Icheon.DTO;
using Newtonsoft.Json;

namespace Icheon
{
    public partial class MainForm : Form
    {
        protected DateTime _dtBegin;


        // 총량.
        protected List<string> _lsOrderCsv = new List<string>(); // 필요없을듯
        protected Dictionary<string, int> _dicOrder = new Dictionary<string, int>();
        protected Dictionary<string, ListViewItem> _dicWorkListView = new Dictionary<string, ListViewItem>();

        List<string> _lsStatusCsv = new List<string>(); // 필요없을듯
        protected Dictionary<string, string> _dicStatusStr = new Dictionary<string, string>();
        protected Dictionary<string, int> _dicStatus = new Dictionary<string, int>(); // 항목별 현재 생산량을 담고 있는 딕셔너리

        protected string _srNowKey = string.Empty; // 이건 비지니스 로직차원에서 data들에 대한 key값으로 사용되니깐 일단 냅두고
        protected int _iOrder = 0, _iStatus = 0; // 현재선택된 항목(in작업목록) 목표생산량 / 현재생산량
        protected Dictionary<string, int> _dicAmountToday = new Dictionary<string, int>();

        private long _nextSerial = 1; // 다음 시리얼번호
        private long _totalOrder = 0; // 주문된 총생산량
        private long _totalStatus = 0; // 현재 라인의 총생산량
        private int totalProducedCount = 0; // 전라인 통합생산량

        private CsvOutputManager _csvOutputManager; // csv 파일 내보내기
        private DB_Manager _dbManager; // DB Connection 및 Insert 처리 매니저 

        // 작업계획량 목록 텍스트 박스 처리 관련
        private TextBox editTextBox = null;
        private int editingSubItemIndex = -1;
        private ListViewItem editingItem = null;
        
        // 기존 _dicOrder 딕셔너리 외에 추가: 발주번호, 발주일시
        protected Dictionary<string, string> _dicOrderNumber = new Dictionary<string, string>();
        protected Dictionary<string, DateTime> _dicOrderDate = new Dictionary<string, DateTime>(); // 이런것도 나중에 vo로 통합처리해야함
        
        // 통합생산량(전라인) SyncTimer (통합생산량이 1만건 미만인 경우 2분마다 LoadAndInsertActiveOrderHistoryAsync 호출)
        private Timer totalPrdCountSyncTimer;
        
        // BaseURL & API Key
        private string _apiBaseUrl;
        private string _apiKey;

        public MainForm()
        {
            InitializeComponent();

            bool useLocal = bool.Parse(ConfigurationManager.AppSettings["UseLocalApi"]);
            _apiBaseUrl = ConfigurationManager.AppSettings[
                useLocal 
                    ? "DevelopmentApiBaseUrl" 
                    : "ProductionApiBaseUrl"
            ];
            _apiKey     = ConfigurationManager.AppSettings[
                useLocal 
                    ? "DevelopmentApiKey" 
                    : "ProductionApiKey"
            ];

            _Init();
        }

        protected void _Init()
        {
            this.Load += _OnLoad;
            this.Shown += _OnShown;
            this.KeyUp += _KeyUpEvent;
            this.Closing += _OnClosing;
            this.Text = @"URCode Client - " + RegionCodeToKr;

            _dtBegin = DateTime.Now;

            //btStart.Click += _OnStartClick; - 작업시작 버튼이 필요가 없음(이천시 기준)
            //btStart.Click += AssignWorkPlanAsync; - 작업시작 버튼이 필요가 없음(이천시 시준)
            btEnd.Click += _OnEndClick;

            btBlackConnect.Click += _OnBlackConnectClick;
            btBlackDisconnect.Click += _OnBlackDisconnectClick;
            btTransparentConnect.Click += _OnTransparentConnectClick;
            btTransparentDisconnect.Click += _OnTransparentDisconnectClick;

            // 프린터 연결, TCP 통신 등에 필요한 이벤트 핸들러 등록
            if (null == _pBlackDomino)
                _pBlackDomino = new Domino();
            //_pBlackDomino.GetDelegateCount(""); // 디버깅용 임시
            _pBlackDomino.OnConnected += (s, e) => _OnBlackDominoConnected();
            _pBlackDomino.OnConnectFail += (s, error) => _OnBlackDominoConnectFail(error);
            _pBlackDomino.OnDisconnected += (s, e) => _OnBlackDominoDisconnected();
            _pBlackDomino.OnDataReceived += (s, data) => _OnBlackDominoDataReceived(data);
            _pBlackDomino.OnTcpError += (s, error) => _OnBlackDominoTcpError(error);
            _pBlackDomino.OnDominoError += (s, error) => _OnBlackDominoError(error);

            if (null == _pTransparentDomino)
                _pTransparentDomino = new Domino();
            _pTransparentDomino.OnConnected += (s, e) => _OnTransparentDominoConnected();
            _pTransparentDomino.OnConnectFail += (s, error) => _OnTransparentDominoConnectFail(error);
            _pTransparentDomino.OnDisconnected += (s, e) => _OnTransparentDominoDisconnected();
            _pTransparentDomino.OnDataReceived += (s, data) => _OnTransparentDominoDataReceived(data);
            _pTransparentDomino.OnTcpError += (s, error) => _OnTransparentDominoTcpError(error);
            _pTransparentDomino.OnDominoError += (s, error) => _OnTransparentDominoError(error);
        }
        
        protected async Task _ReadOrderFromDB()
        {
            // 딕셔너리 초기화
            _dicOrder.Clear();
            _dicOrderNumber.Clear();
            _dicOrderDate.Clear();
            _totalOrder = 0;
            
            // 로컬 DB에서 발주내역 조회
            var orderData = await _dbManager.LoadOrderHistoryAsync();

            foreach (var kvp in orderData)
            {
                string key = kvp.Key;  // "order_number,usage,liter"
                var tuple = kvp.Value; // Item1: total_order, Item2: order_number, Item3: order_date

                _dicOrder.Add(key, tuple.Item1);
                _totalOrder += tuple.Item1;
                _dicOrderNumber.Add(key, tuple.Item2);
                _dicOrderDate.Add(key, tuple.Item3);
            }

            // 기존 로직: 총 주문량에 1.03 배 처리
            _totalOrder = (int)(_totalOrder * 1.03);
        }
        
        private void _UpdateGeneralInfo()
        {
            // 현재라인 총생산량 update
            if (LB_TotalStatus.InvokeRequired)
                LB_TotalStatus.Invoke(new Action(() => LB_TotalStatus.Text = string.Format("{0}", _totalStatus)));
            else LB_TotalStatus.Text = string.Format("{0}", _totalStatus);
            
        }
        
        protected void _InitWorkListView() 
        {
            // 중복 방지를 위해 _dicWorkListView Clear
            _dicWorkListView.Clear();

            foreach (KeyValuePair<string, int> kvp in _dicOrder)
            {
                ListViewItem item = new ListViewItem();
                string key = kvp.Key; // key 형식: "order_number,usage,liter"
                string[] parts = key.Split(',');
                if (parts.Length < 3)
                    continue; // 키 형식이 올바르지 않으면 건너뛰기

                string usage = parts[1]; // 용도
                string liter = parts[2]; // 리터
                
                if(usage.Equals("general", StringComparison.OrdinalIgnoreCase))
                {
                    usage = "일반";
                }
                else if(usage.Equals("food", StringComparison.OrdinalIgnoreCase))
                {
                    usage = "음식물";
                }
                else if(usage.Equals("recycle", StringComparison.OrdinalIgnoreCase))
                {
                    usage = "재사용";
                }

                // 인덱스 0: 체크박스용
                item.Text = "";
                // 인덱스 1: 작업번호 
                item.SubItems.Add("");
                // 인덱스 2: 지자체 – 예: "이천시"
                item.SubItems.Add("이천시");
                // 인덱스 3: 용도 – usage
                item.SubItems.Add(usage);
                // 인덱스 4: 리터 (예: "75L")
                item.SubItems.Add(liter + "L");

                // 인덱스 5: 현재 라인의 생산량
                int currentProduction = 0;
                if (!_dicStatus.TryGetValue(key, out currentProduction))
                    currentProduction = 0;
                item.SubItems.Add(currentProduction.ToString());

                // 인덱스 6: 발주량
                item.SubItems.Add(kvp.Value.ToString());

                // 인덱스 7: 발주번호 – _dicOrderNumber에서 해당 키의 값 사용
                string orderNum = "미할당";
                if (_dicOrderNumber.TryGetValue(key, out orderNum))
                {
                    // 그대로 사용
                }

                item.SubItems.Add(orderNum);

                // 인덱스 8: 발주일시 – _dicOrderDate에서 값 사용
                string orderDateStr = "";
                if (_dicOrderDate.TryGetValue(key, out DateTime orderDate))
                {
                    orderDateStr = orderDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }

                item.SubItems.Add(orderDateStr);

                lvWorkList.Items.Add(item);
                _dicWorkListView.Add(key, item);
            }

            lvWorkList.View = View.Details;
            lvWorkList.GridLines = true;
            lvWorkList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvWorkList.GetType().GetProperty("DoubleBuffered",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(lvWorkList, true);

            lvWorkList.Columns[0].Text = "선택";
            lvWorkList.Columns[0].Width = 40;
            lvWorkList.CheckBoxes = true;
            lvWorkList.ItemChecked -= _OnWorkListViewItemChecked;
            lvWorkList.ItemChecked += _OnWorkListViewItemChecked;

            // 필요에 따라 나머지 컬럼 폭 조정
            lvWorkList.Columns[2].Width = 80; // 지자체
            lvWorkList.Columns[3].Width = 80; // 용도
            lvWorkList.Columns[4].Width = 80; // 리터
            lvWorkList.Columns[5].Width = 180; // 현재라인의 생산량(항목별) 
            lvWorkList.Columns[6].Width = 180; // 발주량
            lvWorkList.Columns[7].Width = 100; // 발주번호
            lvWorkList.Columns[8].Width = 180; // 발주일시
        }



        // 작업목록 UI 갱신
        protected void _UpdateWorkListView(int currentProduction)
        {
            if (!_dicWorkListView.TryGetValue(_srNowKey, out ListViewItem lvi))
                return;
            
            // UI 스레드에서 안전하게 업데이트
            if (lvWorkList.InvokeRequired)
            {
                lvWorkList.Invoke(new Action(() =>
                {
                    // 컬럼 index 5: 현재 라인 생산량 업데이트
                    lvi.SubItems[5].Text = currentProduction.ToString();
                }));
            }
            else
            {
                lvi.SubItems[6].Text = currentProduction.ToString();
            }
        }


        protected void _InitOutputListView()
        {
            lvOutput.View = View.Details;
            lvOutput.GridLines = true;
            lvOutput.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvOutput.GetType().GetProperty("DoubleBuffered",
                BindingFlags.NonPublic |
                BindingFlags.Instance).SetValue(lvOutput, true);

            lvOutput.Columns[0].Width = 80;
            lvOutput.Columns[1].Width = 80;
            lvOutput.Columns[3].Width = 80;
            lvOutput.Columns[4].Width = 80;
            lvOutput.Columns[5].Width = 80;
        }

        protected void _AddOutputListView(string s)
        {
            string[] sr = s.Split(',');
            ListViewItem item = new ListViewItem();
            item.Text = sr[0]; // idx

            //item.SubItems.Add(sr[1]);
            DateTime now = DateTime.Now;
            item.SubItems.Add(string.Format("{0}/{1}/{2}", now.Year, now.Month.ToString("D2"),
                now.Day.ToString("D2"))); // 생산일.

            item.SubItems.Add(sr[2]); // printer
            item.SubItems.Add(sr[3]); // 지자체.
            item.SubItems.Add(sr[4]); // 용도.
            item.SubItems.Add(sr[5] + "L"); // liter
            item.SubItems.Add(sr[6]); // 제품코드

            //
            if (lvOutput.InvokeRequired)
                lvOutput.Invoke(new Action(() =>
                {
                    lvOutput.Items.Add(item);
                    lvOutput.EnsureVisible(lvOutput.Items.Count - 1);
                }));
            else
            {
                lvOutput.Items.Add(item);
                lvOutput.EnsureVisible(lvOutput.Items.Count - 1);
            }
            //lvOutput.Refresh();
        }

        protected async void _OnLoad(Object sender, EventArgs e)
        {
            try
            {
                this.Focus();
                // 타이머 초기화 및 시작
                InitializeSyncTimer();
                
                // dbManager Assign
                _dbManager = new DB_Manager("Server=127.0.0.1;Database=ur_printer_local;Uid=root;Pwd=root;");

                // 중앙서버의 발주량 및 전라인 총 생산량 확인 API 호출 후 결과를 DB에 INSERT
                await LoadAndInsertActiveOrderHistoryAsync();
                await _ReadOrderFromDB();
                
                // 다음시리얼 번호 초기화
                var OrderNumber = _dicOrder.Keys.First().Split(',')[0];
                _nextSerial = await _dbManager.GetNextSerialAsync(OrderNumber);
                
                // _totalStatus 초기화
                _totalStatus = 0;
                _dicStatus.Clear();
                
                // 임시방편
                var usageMap = new Dictionary<string, string>
                {
                    { "재사용", "recycle" },
                    { "음식물", "food"    },
                    { "일반",   "general" }
                };
                
                // 작업목록에 있는 발주번호 추출
                var orderKeys = _dicOrder.Keys;
                var orderNumbers = orderKeys
                    .Select(k => k.Split(',')[0])
                    .Distinct()
                    .ToList();
                
                Dictionary<string, int> prodStatus = await _dbManager.LoadProductionStatusByOrderNumbersAsync(orderNumbers);
                
                // Kor To Eng
                var convertedStatus = prodStatus
                    .Select(kvp =>
                    {
                        var parts = kvp.Key.Split(',');
                        if (parts.Length >= 2)
                        {
                            var kor = parts[1].Trim();
                            parts[1] = usageMap.TryGetValue(kor, out var eng) ? eng : kor;
                        }

                        var newKey = string.Join(",", parts);
                        return (Key: newKey, Value: kvp.Value);
                    })
                    .GroupBy(x => x.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(x => x.Value)
                    );

                _totalStatus = prodStatus.Values.Sum(); // 현재라인 총생산량

                // 각 항목별 현재 생산량을 _dicStatus에 업데이트 : _dicOrder의 키는 "order_number,usage,liter"
                foreach (string orderKey in _dicOrder.Keys)
                {
                    int currentProd = 0;
                    if (convertedStatus.TryGetValue(orderKey, out currentProd))
                    {
                        _dicStatus[orderKey] = currentProd;
                    }
                    else
                    {
                        _dicStatus[orderKey] = 0;
                    }
                }

                // UI에 생산량 정보 갱신
                _UpdateGeneralInfo();
                _InitOutputListView();
                _InitWorkListView(); // 여기서 _dicStatus의 값이 사용되어 현재 생산량이 표시됨
                
                _ConnectBlackDomino();
                _ConnectTransparentDomino();

                lbOutput.Text = string.Format("생산속도: 초당 {0}장", _iBlackPrintRpm);
                nud.Maximum = 999999999;
                nud.Enabled = false;
                nud.Value = _nextSerial;

                // DB에서 미완료 작업계획 불러와서 ListView 업데이트
                await LoadAndApplyIncompleteWorkPlansAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        protected void _OnShown(Object sender, EventArgs e)
        {
        }

        protected void _OnClosing(Object sender, EventArgs e)
        {
            // 작업 미완료가 있을 수 있으므로, 프로그램 닫기 전에 강제로 작업완료 처리
            try
            {
                // OnEndClick 로직을 호출
                _OnEndClick(null, null);
                
                // totalPrdCountSyncTimer 해제
                totalPrdCountSyncTimer?.Stop();
                totalPrdCountSyncTimer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while ending job: " + ex.Message);
            }

            // 기존 종료 처리
            _CloseDomino();
        }

        protected void _KeyUpEvent(Object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad7:
                    MessageBox.Show(Keys.NumPad7.ToString());
                    break;
            }
        }

        protected void _Cwl(object o)
        {
            Console.WriteLine(string.Format("{0} // {1}", o, DateTime.Now.ToString()));
        }
        

        // 작업완료 버튼 클릭 이벤트 핸들러
        protected async void _OnEndClick(Object sender, EventArgs e)
        {
            // 1) DB 업데이트 (is_done='Y') & UI 초기화
            // 체크된 항목이 없는 경우 확인
            if (lvWorkList.CheckedItems.Count == 0)
            {
                MessageBox.Show("체크된 작업 항목이 없습니다.");
                return;
            }

            // 체크된 항목에서 첫 번째 항목
            ListViewItem checkedItem = lvWorkList.CheckedItems[0];
            
            // 작업번호값 추출
            int idx;
            if (!int.TryParse(checkedItem.SubItems[1].Text, out idx))
            {
                MessageBox.Show("작업번호 값을 가져오는 데 실패했습니다.");
                return;
            }

            // 해당 작업 계획의 is_done을 'Y'로 업데이트
            await _dbManager.UpdateWorkPlanningDoneAsync(idx);

            // 1) ---------- 중앙서버의 작업완료 API 호출 ----------
            if (lvOutput.Items.Count == 0)
            {
                MessageBox.Show("중앙서버에 보낼 생산기록 Data가 없습니다.");
                return;
            }

            // UI 대신 DB work_planning 테이블에서 start/end 시리얼 조회하여 사용
            var serials = await _dbManager.GetWorkPlanningSerialsAsync(idx);
            string firstSerial = serials.st_serial;
            string lastSerial  = serials.end_serial;

            // items 배열 생성 (체크된 작업 항목만 대상으로 함)
            List<WorkPlanItemDTO> items = new List<WorkPlanItemDTO>();

            foreach (ListViewItem item in lvWorkList.CheckedItems)
            {
                string capacityText = item.SubItems[4].Text.Trim(); // 리터
                if (capacityText.EndsWith("L"))
                {
                    capacityText = capacityText.Substring(0, capacityText.Length - 1);
                }

                if (!int.TryParse(capacityText, out int capacity))
                {
                    MessageBox.Show("리터 값이 올바르지 않습니다.");
                    return;
                }

                // bag_type : 용도는 인덱스 3
                string bagType = item.SubItems[3].Text.Trim();

                items.Add(new WorkPlanItemDTO()
                {
                    bag_type = bagType,
                    capacity = capacity,
                    start_serial = firstSerial,
                    end_serial = lastSerial
                });
            }

            // 체크된 항목 중 첫 번째 항목에서 발주번호를 추출
            string orderNumberFromUI = "0";
            if (lvWorkList.CheckedItems.Count > 0)
            {
                orderNumberFromUI = lvWorkList.CheckedItems[0].SubItems[7].Text.Trim();
            }

            // 요청 객체 생성
            WorkPlanCompletionRequestDTO requestObj = new WorkPlanCompletionRequestDTO
            {
                production_date = DateTime.Now.ToString("yyyy-MM-dd"),
                printer_number = productionLine,
                order_id = orderNumberFromUI,
                items = items
            };

            // JSON 직렬화
            string jsonBody = JsonConvert.SerializeObject(requestObj);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("/api/production/bulk_create/", content);
                    if (response.IsSuccessStatusCode)
                    {
                        // 중앙서버의 발주량 및 전라인 총 생산량 확인 API 호출 후 결과를 DB에 INSERT 및 UI에서 통합생산량값을 update
                        await LoadAndInsertActiveOrderHistoryAsync();
                        MessageBox.Show("서버에 데이터가 정상적으로 전송되었습니다.");
                    }
                    else
                    {
                        MessageBox.Show("작업완료 API 호출 실패: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("작업완료 API 호출 중 오류 발생: " + ex.Message);
                }
            }

        }


        // 체크박스 - 체크 이벤트 핸들
        protected void _OnWorkListViewItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                // 이미 체크된 모든 항목 해제
                foreach (ListViewItem lvi in lvWorkList.CheckedItems)
                {
                    lvi.Checked = false;
                }

                // 전역 변수에 미리 값 저장 (order_id, usage, liter)
                // order_id는 인덱스 1, 용도는 인덱스 3, 리터는 인덱스 4 (마지막 문자 "L" 제거)
                ListViewItem item = lvWorkList.Items[e.Index];
                if (item != null)
                {
                    // 각 값을 추출
                    string orderId = item.SubItems[7].Text.Trim();
                    string usage = item.SubItems[3].Text.Trim();
                    string liter = item.SubItems[4].Text.Trim();

                    // liter 값이 "L"로 끝나면 제거
                    if (!string.IsNullOrEmpty(liter) && liter.EndsWith("L"))
                    {
                        liter = liter.Substring(0, liter.Length - 1);
                    }
                    
                    var usageMap = new Dictionary<string, string>
                    {
                        { "음식물", "food"    },
                        { "일반",   "general" },
                        { "재사용", "recycle" }
                    };

                    // 2) 원본 값 추출
                    string usageKor = item.SubItems[3].Text.Trim();

                    // 3) 매핑된 값 가져오고, 없으면 원본 유지
                    usage = usageMap.TryGetValue(usageKor, out var eng) ? eng : usageKor;

                    // _srNowKey를 "order_id,usage,liter" 형식으로 구성
                    _srNowKey = string.Format("{0},{1},{2}", orderId, usage, liter);

                    _dicStatus.TryGetValue(_srNowKey, out _iStatus);
                    _dicOrder.TryGetValue(_srNowKey, out _iOrder);
                    Console.WriteLine(string.Format("현재 생산량: {0} / 발주량: {1}", _iStatus, _iOrder));
                }
            }
        }
        
        /// <summary>
        /// 체크박스 상태가 변경된 후 호출
        /// 지금 체크된(e.Item) 항목만 남기고, 나머지는 해제한 뒤 작업계획량 할당을 바로 실행
        /// </summary>
        private async void _OnWorkListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // 1) 체크된 경우에만
            if (!e.Item.Checked)
                return;

            // 2) 다른 모든 아이템 해제 (지금 e.Item 만 남도록)
            foreach (ListViewItem lvi in lvWorkList.Items)
            {
                if (lvi != e.Item && lvi.Checked)
                    lvi.Checked = false;
            }

            // 3) _srNowKey, _iStatus, _iOrder 설정 (원래 _OnWorkListViewItemCheck 에 있던 로직)
            string orderId = e.Item.SubItems[7].Text.Trim();
            string usageKor = e.Item.SubItems[3].Text.Trim();
            string liter    = e.Item.SubItems[4].Text.TrimEnd('L');

            var usageMap = new Dictionary<string,string>
            {
                { "음식물","food" }, { "일반","general" }, { "재사용","recycle" }
            };
            string usageEng = usageMap.TryGetValue(usageKor, out var eng) ? eng : usageKor;

            _srNowKey = $"{orderId},{usageEng},{liter}";
            _dicStatus.TryGetValue(_srNowKey, out _iStatus);
            _dicOrder .TryGetValue(_srNowKey, out _iOrder);

            // 4) 프린터 전송 (기존 로직 그대로)
            _SendBlackPrintContent(_nextSerial, usageKor, liter);
            _SendTransparentPrintContent(_nextSerial, usageKor, liter);

            // 5) 작업계획량 바로 할당
            AssignWorkPlanAsync(this, EventArgs.Empty);
            
            // 6) UI에 반영 : 미완료 계획 다시 불러와 ListView 갱신
            await LoadAndApplyIncompleteWorkPlansAsync();
        }


        protected void _OnBlackConnectClick(Object sender, EventArgs e)
        {
            _ConnectBlackDomino();
        }

        //
        protected void _OnBlackDisconnectClick(Object sender, EventArgs e)
        {
            //_SendBlackPrintContent();
            if (null != _pBlackDomino)
                _pBlackDomino.Disconnect();
        }

        //
        protected void _OnTransparentConnectClick(Object sender, EventArgs e)
        {
            _ConnectTransparentDomino();
        }

        //
        protected void _OnTransparentDisconnectClick(Object sender, EventArgs e)
        {
            //_SendTransparentPrintContent();
            if (null != _pTransparentDomino)
                _pTransparentDomino.Disconnect();
        }

        // 작업계획량 할당 처리 : DB Insert
        private async void AssignWorkPlanAsync(object sender, EventArgs e)
        {
            if (!ValidateStartConditions())
                return;
            
            // 작업할당 버튼 클릭 시 ListView 비활성화
            // lvWorkList.Enabled = false;
            
            
            string regionCode = ConfigurationManager.AppSettings["RegionCode"]; // 예: "50"
            string prodLine = ConfigurationManager.AppSettings["ProductionLine"]; // 생산라인번호
            
            string firstSerial, lastSerial;
            
            if (lvOutput.Items.Count == 0)
            {
                // 생산 기록이 없을 때,
                firstSerial = regionCode + prodLine + _nextSerial.ToString("D9");
                lastSerial = regionCode + prodLine + _nextSerial.ToString("D9");
            }
            else
            {
                // lvOutput의 첫 항목과 마지막 항목의 Serial(서브아이템 인덱스 6)을 사용
                firstSerial = lvOutput.Items[0].SubItems[6].Text;
                lastSerial = lvOutput.Items[lvOutput.Items.Count - 1].SubItems[6].Text;
            }
            
                var usageMap = new Dictionary<string, string>
                {
                    { "음식물", "food" },
                    { "일반",   "general" },
                    { "재사용", "recycle" }
                };
                
            // 체크된 항목들을 순회하여 DTO 객체들을 생성
            List<WorkPlanAssignmentDTO> assignments = new List<WorkPlanAssignmentDTO>();

            foreach (ListViewItem item in lvWorkList.Items)
            {
                if (item.Checked)
                {
                    string orderIdStr = item.SubItems[7].Text;
                    if (!int.TryParse(orderIdStr, out int orderId))
                    {
                        MessageBox.Show("발주번호가 올바르지 않습니다.");
                        return;
                    }

                    // 용도 (인덱스 3)
                    string usageKor = item.SubItems[3].Text.Trim();
                    string usageEng = usageMap.TryGetValue(usageKor, out var eng) ? eng : usageKor;

                    // 리터 (인덱스 4, "75L"에서 L 제거)
                    string literText = item.SubItems[4].Text.Trim();
                    if (literText.EndsWith("L"))
                        literText = literText.Substring(0, literText.Length - 1);
                    
                    assignments.Add(new WorkPlanAssignmentDTO
                    {
                        ProductionLine = prodLine, // 생산라인(프린터번호)
                        Usage = usageEng, // 용도
                        CurrentPrintText = literText, // 리터 (문자열)
                        Order_id = orderId, // 추가된 주문번호
                        st_serial = firstSerial, // 시작 시리얼 (lvOutput의 첫 행 값)
                        end_serial = lastSerial // 끝 시리얼 (lvOutput의 마지막 행 값)
                    });
                }
            }

            if (assignments.Count == 0)
            {
                MessageBox.Show("체크된 항목이 없습니다.");
                return;
            }

            // DB에 각 DTO를 삽입 (중복 발생 시 예외 처리)
            try
            {
                foreach (var dto in assignments)
                {
                    await _dbManager.InsertWorkPlanningAsync(dto);
                }
            }
            catch (Exception ex)
            { 
                if (ex.Message == "DuplicateFound")
                {
                    return; // API도 호출하지 않아야 함
                }
                else
                {
                    MessageBox.Show("DB 삽입 중 오류 발생: " + ex.Message);
                    return; // API도 호출하지 않아야 함
                }
            }
        }
        
        // 미완료 작업계획 UI(작업목록) 반영
        private async Task LoadAndApplyIncompleteWorkPlansAsync()
        {
            var incompletePlans = await _dbManager.LoadIncompleteWorkPlansAsync(); // 미완료 작업계획 Load

            foreach (var record in incompletePlans)
            {
                int idx = Convert.ToInt32(record["idx"]); // 작업번호
                string prdLine = record["prd_line"].ToString(); // 필요시 사용
                string usage = record["usage"].ToString();
                string liter = record["liter"].ToString();
                string order_num = record["order_id"].ToString();

                // 한글 라벨을 영어 코드로 변환
                string usageCode;

                switch (usage) {
                    case "일반":
                        usageCode = "general";
                        break;
                    case "음식물":
                        usageCode = "food";
                        break;
                    case "재사용":
                        usageCode = "recycle";
                        break;
                    default:
                        usageCode = usage; // 이미 영어 코드라면 그대로
                        break;
                }
                
                // 키 생성: "용도,리터"
                string key = string.Format("{0},{1},{2}", order_num, usageCode, liter);
                

                if (_dicWorkListView.TryGetValue(key, out ListViewItem lvi))
                {
                    // 작업번호 컬럼은 인덱스 1
                    if (lvWorkList.InvokeRequired)
                    {
                        lvWorkList.Invoke(new Action(() => { lvi.SubItems[1].Text = idx.ToString(); }));
                    }
                    else
                    {
                        lvi.SubItems[1].Text = idx.ToString();
                    }

                    // 현재 생산량 _dicStatus에서 가져옴
                    int currentProduction = 0;
                    if (!_dicStatus.TryGetValue(key, out currentProduction))
                        currentProduction = 0;
                    
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void LB_RestOrder_Click(object sender, EventArgs e)
        {
        }

        private void LB_TotalStatus_Click(object sender, EventArgs e)
        {
        }

        // 작업계획량 할당(작업시작) 버튼 동작 조건 Check
        private bool ValidateStartConditions()
        {
            if (!_bBalckDominoConnect)
            {
                MessageBox.Show("프린터와 연결되지 않았습니다.");
                return false;
            }

            if (!_bBlackDominoUrcode)
            {
                MessageBox.Show("도미노 프린터와 통신 중입니다. 잠시 후에 다시 시작하세요.");
                return false;
            }
            
            if (_nextSerial < 0)
            {
                MessageBox.Show("시리얼번호가 정상적으로 로드되지 않았습니다.");
                return false;
            }

            if (totalProducedCount >= _totalOrder)
            {
                MessageBox.Show("남아있는 총생산량이 없습니다.");
                return false;
            }

            return true;
        }
        
        // 발주 조회 : 발주가 조회될 때, UI에서 통합생산량(전라인) 항목을 업데이트 처리
        private async Task LoadAndInsertActiveOrderHistoryAsync()
        {
            // productionLine 값을 appConfig에서 가져옵니다.
            string productionLine = ConfigurationManager.AppSettings["ProductionLine"];
            string requestUri = $"/api/orders/active/?printer_number={productionLine}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
                    HttpResponseMessage response = await client.GetAsync(requestUri);
                    
                    if (response.IsSuccessStatusCode) // 200 ok
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        OrderActiveResponseDTO orderResponse =
                            JsonConvert.DeserializeObject<OrderActiveResponseDTO>(jsonResponse);
                        if (orderResponse != null && orderResponse.order_items != null)
                        {
                            _totalOrder = 0; // _totalOrder 초기화
                            
                            // 각 order_item에 대해 order_history 테이블에 INSERT 및 get_produced_count 합산
                            long orderNumber = 0;
                            if (!long.TryParse(orderResponse.order_number, out orderNumber))
                            {
                                // 파싱 실패시 0으로 처리
                                orderNumber = 0;
                            }

                            // order_date은 API의 created_at 값 사용
                            DateTime orderDate = orderResponse.created_at;

                            totalProducedCount = 0;

                            foreach (var item in orderResponse.order_items)
                            {
                                // 각 item의 get_produced_count 누적
                                totalProducedCount += item.get_produced_count;

                                // DB에 삽입할 OrderHistory 레코드 생성
                                OrderHistoryRecordDTO record = new OrderHistoryRecordDTO
                                {
                                    order_number = orderNumber,
                                    order_date = orderDate,
                                    usage = item.bag_type.type,
                                    liter = item.bag_type.capacity.ToString(),
                                    order_amount = item.quantity
                                };

                                _totalOrder += record.order_amount;
                                // DB에 INSERT
                                await _dbManager.InsertOrderHistoryAsync(record);
                            }
                            
                            _totalOrder = (int)(_totalOrder * 1.03);
                            
                            // INSERT 이후, UI에 즉시 반영
                            RefreshWorkList();
                            
                            // UI의 label7(통합생산량-전라인) 업데이트
                            if (label7.InvokeRequired)
                            {
                                label7.Invoke(new Action(() => { label7.Text = totalProducedCount.ToString(); }));
                            }
                            else
                            {
                                label7.Text = totalProducedCount.ToString();
                            }
                            
                            // UI의 LB_RestOrder(통합생산 잔여량-전라인) 업데이트
                            long restOrder = _totalOrder - totalProducedCount;
                            if (LB_RestOrder.InvokeRequired)
                            {
                                LB_RestOrder.Invoke(new Action(() => { LB_RestOrder.Text = restOrder.ToString(); }));
                            }
                            else
                            {
                                LB_RestOrder.Text = restOrder.ToString();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("GET /api/orders/active 호출 실패: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GET /api/orders/active 호출 중 오류 발생: " + ex.Message);
            }
        }
        
        // 타이머 초기화 메서드
        private void InitializeSyncTimer()
        {
            if (totalPrdCountSyncTimer == null)
            {
                totalPrdCountSyncTimer = new Timer
                {
                    Interval = 1 * 60 * 1000  // 1분
                };
                
                // 2분마다 totalPrdCountSyncTimer.Tick 이벤트 핸들처리
                totalPrdCountSyncTimer.Tick += async (s, e) =>
                {
                    long remaining = _totalOrder - totalProducedCount;

                    // 남은 발주량이 1만 미만일 때만 API를 호출하여 동기화
                    if (remaining < 10000)
                    {
                        await LoadAndInsertActiveOrderHistoryAsync();

                        // 동기화 뒤 조건 재계산
                        remaining = _totalOrder - totalProducedCount;

                        // 발주량이 1만 이상인 경우 타이머 중지
                        if (remaining >= 10000)
                            totalPrdCountSyncTimer.Stop();
                    }
                };
            }

            totalPrdCountSyncTimer.Start();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private async Task RefreshWorkList()
        {
            // 기존 항목들 Clear
            lvWorkList.Items.Clear();
            _dicWorkListView.Clear();

            // re-load order data
            // then init
            _InitWorkListView();
        }


        private void label7_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    } // public partial class MainForm : Form
} // namespace Icheon