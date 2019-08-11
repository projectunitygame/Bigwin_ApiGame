using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models.Exceptions
{
    public class PlayerAlreadyInRoomException : Exception
    {
        public PlayerAlreadyInRoomException()
        {
        }
    }
}