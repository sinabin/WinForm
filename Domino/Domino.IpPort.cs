using System;


//
namespace Icheon
{
    public partial class Domino : TcpClientParent
    {
        // @"192.168.0.1";// 공유기 어드민.
        // @"192.168.0.2";// PC

        //public const string BLACK_IP = @"192.168.0.101";// 흑백 도미노.
        public const string BLACK_IP = @"127.0.0.1";// 흑백 도미노 - test용
        public const int BLACK_PORT = 7000;// 도미노 프린터 디폴트.
        
        
        //public const string TRANSPARENT_IP = @"192.168.0.102";// 투명 도미노.
        public const string TRANSPARENT_IP = @"127.0.0.1";// 투명 도미노 - test용
        public const int TRANSPARENT_PORT = 7001;// 도미노 프린터 디폴트.
        
        
        //public const string ESC = "ox1b";
        //public const string EOT = "ox04";


    }// public partial class Domino : TcpClientParent
}// namespace Icheon