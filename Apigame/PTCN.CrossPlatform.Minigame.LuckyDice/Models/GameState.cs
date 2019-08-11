namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models
{
    /// <summary>
    /// Determine which game state is
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// 55s betting 
        /// </summary>
        Betting = 0,

        /// <summary>
        /// 20s finish session + can cua + tra thuong
        /// </summary>
        ShowResult = 1,

        /// <summary>
        /// 3s chuan bi cho session moi
        /// </summary>
        PrepareNewRound = 2,

        /// <summary>
        /// 5s chot phien
        /// </summary>
        EndBetting = 3
    }

    public enum BetSide
    {
        Tai = 0,
        Xiu = 1
    }

    public class Config
    {
        public static int[] TimeConfig =  { 50, 20, 1, 0 };
    }
}