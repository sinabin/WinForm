//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//! @file   MainClass.cs
//! @brief  
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;


namespace Icheon
{
    static class MainClass
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // System.Windows.Forms.Application : 크게 두 가지 역할을 수행하는데, 하나는 윈도우 응용 프로그램을 시작하고 종료시키는 메소드를 제공하는 것이고,
            // 또 다른 하나는 윈도우 메시지를 처리하는 것. [이것이 C#이다(7.2) P.669]
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 응용 프로그램을 종료시키는 메소드는 Application.Exit()입니다. Exit() 메소드가 호출된다고 해서 응용 프로그램이 바로 종료되는 것은 아닙니다. 이 메소드가 하는 일은
            // 응용 프로그램이 갖고 있는 모든 윈도우를 닫은 뒤 Run() 메소드가 반환되도록 하는 것입니다. 따라서 Run() 메소드 뒤에 자원을 정리하는 코드를 넣어 두면 우아하게 
            // 응용 프로그램을 종료시킬 수 있습니다. [이것이 C#이다(7.2) P.669]
            MainForm mf = new MainForm();

            // 응용 프로그램을 시작. 종료시키는 메소드는 Application.Exit() [이것이 C#이다(7.2) P.669]
            //
            // 인수로 전달된 폼을 메인 윈도우로 하여 이 윈도우가 종료될 때까지 프로그램을 계속 실행한다. Run 메서드는 메인 메시지 루프 역할을 하며 폼으로 전달되는 메시지를 받아 처리한다.
            // API의 메시지 루프, MFC의 CWinThread::Run 함수와 유사한 역할을 한다. [닷넷 프로그래밍 정복 P.561]
            Application.Run(mf);
        }
    }// static class MainClass
}// namespace Icheon