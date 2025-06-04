//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//! @file   MainForm.txt.cs
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using System.Text;


//
namespace Icheon
{
    public partial class MainForm : Form
    {
        public enum FileType
        {
            BIN,
            TXT,
            CSV,
            DATATABLE
        }


        // 로컬에 디렉토리가 있는지 확인.
        public static bool DirExists(string path)
        {
            return Directory.Exists(path);
        }

        //
        public static string[] DirFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        //
        public static void DirCreate(string path)
        {
            Directory.CreateDirectory(path);
        }

        // 로컬에 파일이 있는지 확인.
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        // 로컬에 파일이 있는지 확인.
        public static bool FileExists(string path, string ext)
        {
            return File.Exists(path + ext);
        }

        //
        public static string GetLastWritedFilePath(string path)
        {
            string[] files = Directory.GetFiles(path);
            if (null == files || 0 == files.Length)
                return null;

            // 마지막 수정일의 파일?
            string filepath = string.Empty;
            DateTime l = DateTime.MinValue;// System.DateTime의 최소값을 나타냅니다. 이 필드는 읽기 전용입니다.
            foreach (string s in files)
            {
                DateTime r = File.GetLastWriteTime(s);
                //if (l.CompareTo(r) >= 0)// 이 인스턴스의 값을 지정된 System.DateTime 값과 비교하고 이 인스턴스가 지정된 System.DateTime 값보다 이전(-)인지, 같은지(0) 또는 이후(+)
                if (l >= r)// 지정된 System.DateTime이 다른 지정된 System.DateTime과 같거나 나중인 날짜와 시간을 나타내는지를 결정합니다.
                    continue;
                l = r;
                filepath = s;
            }

            //
            return filepath;
        }

        // 파일 쓰기.
        public static bool WriteFile(string path, FileType ft, string txt)
        {
            try
            {
                // FileStream은 저장 장치와 데이터를 주고받도록 구현(디스크 파일에 데이터를 기록합니다.) [뇌를 자극하는 C# 4.0 프로그래밍 P.519]
                FileStream fs = new FileStream(path,
                    // FileMode.Open,// 파일 열기.
                    FileMode.Create,// 새 파일을 만드는데 사용한다. 만약 같은 이름의 파일이 존재하면 덮어쓴다.
                                    //FileMode.OpenOrCreate,// 같은 이름의 파일이 존재하면 열고 없으면 새로 생성한다.
                                    //FileMode.Append,// 파일을 열고 데이터를 추가하는데 사용한다. 반드시 FileAccess.Write 값과 함께 사용해야 한다. 
                                    //FileMode.Truncate,// 존재하는 파일의 내용을 무시하고 빈 파일로 연다.
                    FileAccess.Write);

                //
                if (ft == FileType.BIN)
                {
                    // FileStream 클래스는 파일 처리를 위한 모든 것을 갖고 있지만, 사용하기에 여간 불편한 것이 아닙니다. 특히 데이터를 저장할 때 반드시 byte 형식 또는
                    // byte의 배열 형식으로 변환해야 한다(BitConverter class)는 문제가 있었습니다. 이것은 파일로부터 데이터를 읽을 때도 마찬가지였습니다. 
                    // .NET 프레임워크는 FileStream의 이런 불편함을 해소하기 위해 도우미 클래스들을 제공하고 있습니다. BinaryWriter는 스트림에 이진 데이터(Binary Data)를
                    // 기록하기 위한 목적으로 만들어진 클래스입니다. 이 클래스는 어디까지나 파일 처리의 도우미 역할을 할 뿐이기 때문에 이들 클래스들을 이용하려면 Stream으로부터
                    // 파생된 클래스의 인스턴스가 있어야 합니다. BinaryWriter의 생성자를 호출하면서 FileStream의 인스턴스를 매개 변수로 넘기고 있습니다. 이제 BinaryWriter의
                    // 객체는 FileStream의 인스턴스가 생성한 스트림에 대해 이진 데이터 기록을 수행할 겁니다. [뇌를 자극하는 C# 4.0 프로그래밍 P.527]
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(txt);// BinaryWriter는 Write() 메소드를 C#이 제공하는 모든 기본 데이터 형식에 대해 오버로딩하고 있기 때문에 byte나 byte 배열로 변환하지 않는다.
                    bw.Close();// 쓰기를 마쳤으면 Close()를 호출하여 내부 스트림을 닫습니다. [뇌를 자극하는 C# 4.0 프로그래밍 P.528]
                }
                else if (ft == FileType.TXT || ft == FileType.CSV)
                {
                    // 텍스트 파일은 구조는 간단하지만 활용도는 높은 파일 형식입니다. ASCII 인코딩에서는 각 바이트가 문자 하나를 나타내기 때문에 바이트 오더의 문제에서도 
                    // 벗어날 수 있고, 이로 인해 플랫폼에 관계없이 생성하고 읽을 수 있습니다. 뿐만 아니라 프로그램이 생성한 파일의 내용을 편집기로 열면 사람이 바로 읽을 수도
                    // 있습니다. .NET 프레임워크는 텍스트 파일을 쓸 수 있도록 StreamWriter를 제공합니다. [뇌를 자극하는 C# 4.0 프로그래밍 P.530]
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(txt);
                    sw.Close();
                }

                //
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        // 파일 읽기.
        public static string ReadFile(string path, FileType ft)
        {
            if (!FileExists(path))
            {
                Console.WriteLine(path + " doesn't exist!!!");
                return null;
            }

            //
            string s = string.Empty;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (FileType.BIN == ft)
                {
                    // BinaryReader 클래스는 스트림으로부터 이진 데이터를 읽어 들이기 위한. [뇌를 자극하는 C# 4.0 프로그래밍 P.527]
                    BinaryReader br = new BinaryReader(fs);
                    if (null != br)
                    {
                        s = br.ReadString();
                        br.Close();
                    }
                }
                else if (FileType.TXT == ft ||

                    // 엑셀에서 csv 파일로 저장하면 한글 윈도우에서 기본 인코딩 방식은 euc-kr. 따라서 C#에서 그 csv 파일을 읽으면 한글이 깨진다.
                    // UTF로 변경하는 방법은 3가지(엑셀의 웹 옵션, 구글스프레트시트, 메모장)가 있으나, 메모장에서 파일을 열고 인코딩 방식을 UTF8로 변경 후 저장한다.
                    // 마지막 빈 행이 있을 수 있으니, 메모장에서 확인 후 삭제. [https://ssems-teacher.tistory.com/32]
                    //
                    // UTF8로 인코딩 된 csv 파일은 파일 더블클릭으로 엑셀에서 열면 한글이 깨진다. 데이터 -> 텍스트/CSV로 읽는다.
                    // [https://blogpack.tistory.com/613], [https://blog.naver.com/lsw3210/222015854342]
                    //
                    // csv 숫자0표시하도록 [https://pstree.tistory.com/62]
                    FileType.CSV == ft)
                {
                    // .NET 프레임워크는 텍스트 파일을 읽을 수 있도록 StreamReader 제공합니다. [뇌를 자극하는 C# 4.0 프로그래밍 P.530]
                    StreamReader sr = new StreamReader(fs);
                    //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                    if (null != sr)
                    {
                        s = sr.ReadToEnd();
                        sr.Close();
                    }
                }

                //
                fs.Close();
                return s;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return s;
            }
        }

    }// public partial class MainForm : Form
}// namespace Icheon
