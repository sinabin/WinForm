using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Icheon
{
    public static class Constants
    {
        public const double PI = 3.14159;
        public const long GIGABYTE = 1073741824;
        public const long MEGABYTE = 1048576;
        public const long KILOBYTE = 1024;
        public static UInt16[] Bit16 = { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
        public static byte[] Bit8 = { 0, 1, 2, 4, 8, 16, 32, 64, 128 };
    }


    //
    public static class FileTools
    {
        public static string GetFileDirectory(string path)
        {
            //string BasicINIPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TG");
            string BasicINIPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), path);
            DirectoryInfo dir = new DirectoryInfo(BasicINIPath);

            if (dir.Exists == false)
            {
                dir.Create();
            }
            return BasicINIPath;
        }
        /// <summary>
        /// 파일명을 포함하는 파일 수를 확인합니다. 예) Input : abc.txt, 검색되는 파일명 : abc*.txt... 
        /// </summary>
        /// <param name="path">파일을 검색할 폴더 위치</param>
        /// <param name="fileName">검색할 파일 예) abc.txt</param>
        /// <returns>검색된 파일 수</returns>
        public static int DuplicateFileCount(string path, string fileName)
        {
            string strName = Path.GetFileNameWithoutExtension(fileName);
            string strExt = Path.GetExtension(fileName);

            string[] Files = Directory.GetFiles(path, strName + "*" + strExt);

            int count = 0;
            foreach (string str in Files)
            {
                count++;
            }
            return count;
        }
        /// <summary>
        /// 디렉토리를 생성합니다.
        /// </summary>
        /// <param name="path">디렉토리명이 포함된 경로</param>
        public static void CreateDirectory(string path)
        {
            DirectoryInfo LogDir = new DirectoryInfo(path);

            if (LogDir.Exists == false)
            {
                LogDir.Create();
            }
        }
        /// <summary>
        /// 파일을 나눕니다.
        /// </summary>
        /// <param name="file">나눌 파일 명 예) abc.txt</param>
        /// <param name="FileSize">나눌 크기</param>
        /// <returns>나누어진 파일명 예abc(2).txt </returns>
        public static string DividLogFile(FileInfo file, int FileSize)
        {
            if (file.Exists)
            {
                if (file.Length > Constants.MEGABYTE * FileSize) //파일 크기가 커지면 분할 저장을 위한 파일명 생성
                {
                    return FileTools.GetAvailablePathname(file.DirectoryName, file.Name);
                }
            }
            return file.Name;
        }

        /// <summary>
        /// 사용 가능한 파일 명을 생성합니다.
        /// </summary>
        /// <param name="folderPath">폴더 경로</param>
        /// <param name="filename">파일 명</param>
        /// <returns>사용 가능한 파일명</returns>
        public static string GetAvailablePathname(string folderPath, string filename)
        {
            int invalidChar = 0;
            do
            {
                invalidChar = filename.IndexOfAny(Path.GetInvalidFileNameChars());

                if (invalidChar != -1)
                    filename = filename.Remove(invalidChar, 1);
            }
            while (invalidChar != -1);

            string fullPath = Path.Combine(folderPath, filename);
            string filenameWithoutExtention = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            string reName = filename;

            while (File.Exists(fullPath))
            {
                Regex rg = new Regex(@".*\((?<Num>\d*)\)");
                Match mt = rg.Match(fullPath);

                if (mt.Success)
                {
                    string numberOfCopy = mt.Groups["Num"].Value;
                    int nextNumberOfCopy = int.Parse(numberOfCopy) + 1;
                    int posStart = filenameWithoutExtention.LastIndexOf("(" + numberOfCopy + ")");
                    filenameWithoutExtention = filenameWithoutExtention.Remove(posStart);

                    reName = string.Format("{0}({1}){2}", filenameWithoutExtention, nextNumberOfCopy, extension);
                }
                else
                {
                    reName = filenameWithoutExtention + " (1)" + extension;
                }
                fullPath = folderPath + reName;
            }
            return reName;
        }
    }
}
