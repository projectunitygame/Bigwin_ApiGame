using Intecom.Software.RDTech.SlotMachine.DataAccess.DAO;
using Intecom.Software.RDTech.SlotMachine.DataAccess.Factory;
using Studio.WebGame.SupperNova.Controllers;
using System.Web.Http;
using MiniGame.SuperNovaServer.Models;
using Utilities.Session;

namespace Studio.WebGame.SupperNova.App_Start.WebApi
{
    public class MiniSlot2Controller : ApiController
    {
        private ISlotMachineDAO _slotDAO = AbstractDAOFactory.Instance().CreateSlotMachineDAO();

        [HttpOptions, HttpGet]
        public IHttpActionResult GetAccountHistory(MoneyType moneyType = MoneyType.Gold)
        {
            long accountId = AccountSession.AccountID;
            if (accountId < 1)
                return null;
            var res = _slotDAO.GetTransactionHistory(accountId, moneyType);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [HttpOptions, HttpGet]
        public IHttpActionResult GetJackpotHistory(MoneyType moneyType = MoneyType.Gold)
        {
            var res = _slotDAO.GetJackpotHistory(moneyType);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }

        [HttpOptions, HttpGet]
        public IHttpActionResult GetHonorHistory(MoneyType moneyType = MoneyType.Gold)
        {
            var res = _slotDAO.GetHonorHistory(moneyType);
            if (res == null)
            {
                return NotFound();
            }
            //res.ForEach(h => h.Username = h.Username.Substring(0, h.Username.Length - 3) + "***");
            return Ok(res);
        }
    }
}