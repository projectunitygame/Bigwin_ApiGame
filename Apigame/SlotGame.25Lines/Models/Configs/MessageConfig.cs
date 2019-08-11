using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models.Configs
{
    public enum GameMessage
    {
        OtherDevices = -21,
        Default = -99

    }
    public static class MessageFactory
    {
        public static string GetMessage(GameMessage msg)
        {
            switch (msg)
            {
                case GameMessage.OtherDevices:
                    return "Tài khoản của bạn đang được chơi trên thiết bị khác";
                default:
                    return "Hệ thống đang bận, vui lòng thử lại sau";
            }
        }
    }
}