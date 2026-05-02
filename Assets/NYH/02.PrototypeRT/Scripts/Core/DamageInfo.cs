using UnityEngine;

namespace PrototypeRT
{
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly GameObject Source;
        public readonly Team SourceTeam;

        // 데미지 출처를 함께 넘겨두면 나중에 장비, 직업, 유전자, 상태이상 보정이 들어와도 호출부를 크게 바꾸지 않아도 된다.
        public DamageInfo(int amount, GameObject source, Team sourceTeam)
        {
            Amount = amount;
            Source = source;
            SourceTeam = sourceTeam;
        }
    }
}
