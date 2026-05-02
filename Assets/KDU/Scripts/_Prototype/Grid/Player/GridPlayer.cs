using UnityEngine;

/// <summary>
/// 격자 플레이어 메인 브레인. 4개 서브 컴포넌트를 조정하고 IDamageable을 위임.
/// </summary>
[RequireComponent(typeof(GridPlayerInput))]
[RequireComponent(typeof(GridPlayerMovement))]
[RequireComponent(typeof(GridPlayerCombat))]
[RequireComponent(typeof(GridPlayerHealth))]
public class GridPlayer : EntityBase, IDamageable
{
    [SerializeField] private GameConfig config;

    public GameConfig Config => config;

    public GridPlayerInput Input { get; private set; }
    public GridPlayerMovement Movement { get; private set; }
    public GridPlayerCombat Combat { get; private set; }
    public GridPlayerHealth Health { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Input = GetComponent<GridPlayerInput>();
        Movement = GetComponent<GridPlayerMovement>();
        Combat = GetComponent<GridPlayerCombat>();
        Health = GetComponent<GridPlayerHealth>();

        Movement.Init(this);
        Combat.Init(this);
        Health.Init(this);
        Input.Init(this);
    }

    private void Start()
    {
        // 씬에 배치된 위치를 격자에 스냅
        var startPos = GridSystem.Instance.WorldToGrid(transform.position);
        Initialize(startPos);

        VisionSystem.Instance.UpdateVision(GridPosition);
        TimeSystem.Instance.StartTimer();
    }

    public override void OnTurnUpdate()
    {
        // 플레이어는 입력으로 구동되며 TurnManager 엔티티 리스트에 등록되지 않음
    }

    // IDamageable 위임
    public int CurrentHp => Health.CurrentHp;
    public int MaxHp => Health.MaxHp;
    public bool IsDead => Health.IsDead;
    public void TakeDamage(int amount) => Health.TakeDamage(amount);
    public void Heal(int amount) => Health.Heal(amount);
}
