using GamePortal.API.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace GamePortal.API.Models.AnotherLogic
{
    public class GameLockQuery
    {
        private StringBuilder _query;
        private bool _start = false;
        private bool _end = false;

        public GameLockQuery()
        {
            _query = new StringBuilder();
        }

        public void StartQuery()
        {
            _start = true;
            _query.AppendLine("begin transaction");
            _query.AppendLine("begin try");
        }

        public void EndQuery()
        {
            _end = true;
            _query.AppendLine("commit transaction");
            _query.AppendLine("end try");
            _query.AppendLine("begin catch");
            _query.AppendLine("if @@trancount > 0 begin rollback transaction end;");
            _query.AppendLine("throw 50000, 'sql exception', 1");
            _query.AppendLine("end catch");
        }

        public void LockGame(long accountId, int gameId, int status)
        {
            if (!_start || _end)
                throw new Exception("Query not started or already finished yet");

            _query.AppendLine($"update game.Lock set Disabled = {status} where AccountId = {accountId} and GameId = {gameId}");
            _query.AppendLine($"if @@rowcount = 0 insert into game.Lock values ({accountId}, {gameId}, {status})");
        }

        public void Execute()
        {
            GameDAO.ExecuteLockCommand(ToString());
        }

        public override string ToString()
        {
            if (!_start && !_end)
                throw new Exception("Query not started or already finished yet");

            return _query.ToString();
        }
    }
}