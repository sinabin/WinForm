//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//! @file   MainForm.Urcode.cs
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

//
namespace Icheon
{
    public partial class MainForm : Form
    {
        // 기존 _SendUrcode
        protected void _SendUrcode()
        {
            //string bmp = _ConvertBMPToHex(@"D:\11.Icheon(250329)\bin\x64\Debug\Urcode01.bmp");
            //_Img2Byte(@"D:\11.Icheon(250329)\bin\x64\Debug\Urcode01.bmp");
            //string bmp = MainForm.ConvertBMPToHex(@"D:\11.Icheon(250331)\bin\x64\Debug\Urcode01.bmp");
            //현장PC 절대경로 //string bmp = MainForm.ConvertBMPToHex(@"D:\11.Icheon(250331)\bin\x64\Debug\img_bar_3039-250331_58x9.bmp");
            string bmp = MainForm.ConvertBMPToHex("./img_bar_3039-250331_58x9.bmp");

            //
            string esc = Convert.ToString((char)0x1b);
            string eot = Convert.ToString((char)0x04);
            string ur = @"UR";
            string s = esc + "OL" +
                "2UR" +
                //"424D76000000000000003E000000280000003600000007000000010001000000000038000000222E0000222E0000020000000000000000000000FFFFFF0000000000000000007FFFFFFFFFFFF8005B56D56B55AD68005FFFFFFFFFFFF8005FD6AB6ADAAAA8007FFFFFFFFFFFF8000000000000000000" +
                bmp +
                eot;
            _pBlackDomino.SendData(s);
        }
        
        public static string ConvertBMPToHex(string fileName)
        {
            StringBuilder result = new StringBuilder();
            using (Bitmap originalBmp = new Bitmap(fileName))
            {
                // BMP를 1비트 흑백 이미지로 변환
                using (Bitmap bmp = originalBmp.Clone(new Rectangle(0, 0, originalBmp.Width, originalBmp.Height), PixelFormat.Format1bppIndexed))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        bmp.Save(memStream, ImageFormat.Bmp);
                        memStream.Position = 0;

                        byte[] byteArray = memStream.ToArray();
                        foreach (byte byteData in byteArray)
                        {
                            result.Append(byteData.ToString("X2"));
                        }
                    }
                }
            }
            return result.ToString();
        }

        // 매일 API를 통해 Urcode BMP 이미지를 다운로드하거나, 이미 존재하면 로컬 파일 경로를 반환
        private async Task<string> DownloadDailyUrcodeAsync()
        {
            try
            {
                // 1) 저장 폴더 및 파일명 준비
                string baseDir =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "ur_code_printer");
                Directory.CreateDirectory(baseDir);

                string fileName = $"Urcode_{DateTime.Now:yyyyMMdd}.bmp";
                string filePath = Path.Combine(baseDir, fileName);

                if (File.Exists(filePath))
                    return filePath;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);

                    // 2) JSON 메타정보 요청
                    HttpResponseMessage apiResponse = await client.GetAsync("api/ur/daily/create");
                    apiResponse.EnsureSuccessStatusCode();

                    string jsonString = await apiResponse.Content.ReadAsStringAsync();

                    var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    if (json == null ||
                        !json.TryGetValue("status", out var statusObj) ||
                        statusObj?.ToString().Equals("success", StringComparison.OrdinalIgnoreCase) != true)
                    {
                        throw new Exception($"API 상태 오류: {jsonString}");
                    }

                    if (!json.TryGetValue("image_url", out var urlObj) ||
                        string.IsNullOrWhiteSpace(urlObj?.ToString()))
                    {
                        throw new Exception("응답에 image_url이 없습니다.");
                    }

                    string imageUrl = urlObj.ToString();

                    // 5) 실제 BMP 바이트 다운로드
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // 6) 파일로 저장 (동기)
                    File.WriteAllBytes(filePath, imageBytes);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UR 코드 다운로드 오류: {ex.Message}");
                return null;
            }
        }
        
        // Urcode 이미지를 매일 다운로드 및 BMP를 HEX 문자열로 변환하여 프린터로 전송
        protected async Task _SendUrcodeForEachDay()
        {
            // 이미지 다운로드
            string filePath = await DownloadDailyUrcodeAsync();
            
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("Urcode 파일을 찾을 수 없습니다.: " + filePath);
                return;
            }

            string bmpHex = ConvertBMPToHex(filePath);
            if (string.IsNullOrEmpty(bmpHex))
            {
                Console.WriteLine("BMP 변환에 실패했습니다.");
                return;
            }

            string esc = ((char)0x1b).ToString();
            string eot = ((char)0x04).ToString();
            string payload = esc + "OL" + "2UR" + bmpHex + eot;

            // 실제 프린터 전송
            _pBlackDomino.SendData(payload);
        }

    }// public partial class MainForm : Form
}// namespace Icheon
