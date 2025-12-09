using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    /*public WorldGraph worldGraph;
    public BattleSystem(WorldGraph worldGraph)
    {
        this.worldGraph = worldGraph;
    }*/
    public class BattleResult
    {
        public CountryStateManager Winner;
        public CountryStateManager Loser;
        public int AttackerLosses;
        public int DefenderLosses;
        public bool AttackerWon;
        public int Military;
    }

    public BattleResult DoBattle(CountryStateManager attacker, CountryStateManager defender)
    {
        /*if(worldGraph == null)
        {
            Debug.LogError("WorldGraph 未設置！");
            return null;
        }*/
        if (attacker == null || defender == null)
        {
            Debug.LogError($"❌ attacker 或 defender 為 null！ attacker={attacker}, defender={defender}");
            return null;
        }
        Country attackerCountry = attacker.resource.countries.Find(c => c.CountryName == attacker.CountryName);
        Country defenderCountry = defender.resource.countries.Find(c => c.CountryName == defender.CountryName);
        /*if (!worldGraph.CheckAttack(attacker, defender))//檢查能否攻擊
        {
            Debug.Log($"{attacker.CountryName} 無法攻擊 {defender.CountryName}：沒有直接或同盟通路。");
            return null;
        }*/
        
            // 士氣換算: 基礎為 1 + 民心值/100
            float attackerMorale = 1f + attacker.morale.MoraleValue / 100f;
            float defenderMorale = 1f + defender.morale.MoraleValue / 100f;

            // 戰鬥力公式: 兵力 * 士氣
            float attackerPower = attackerCountry.MilPower * attackerMorale;
            float defenderPower = defenderCountry.MilPower * defenderMorale;

            // 勝負判定: 攻方必須大於防守方才勝利
            bool attackerWon = attackerPower > defenderPower;

            CountryStateManager winner = attackerWon ? attacker : defender;
            CountryStateManager loser = attackerWon ? defender : attacker;

            // 傷亡計算: 基礎兵力一半 / 軍團戰鬥力 / 士氣
            int attackerLosses = Mathf.CeilToInt((attackerCountry.MilPower / 2f) / attackerMorale);
            int defenderLosses = Mathf.CeilToInt((defenderCountry.MilPower / 2f) / defenderMorale);

            // 扣除兵力 (避免負數)
            attackerCountry.MilPower = Mathf.Max(0, attackerCountry.MilPower - attackerLosses);
            defenderCountry.MilPower = Mathf.Max(0, defenderCountry.MilPower - defenderLosses);

            return new BattleResult
            {
                Winner = winner,
                Loser = loser,
                AttackerLosses = attackerLosses,
                DefenderLosses = defenderLosses,
                AttackerWon = attackerWon
            };
        
    }
}