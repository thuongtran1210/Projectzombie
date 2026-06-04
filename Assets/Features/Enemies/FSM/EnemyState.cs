namespace ProjectZombie.Features.Enemies
{
    public abstract class EnemyState
    {
        protected Enemy _enemy;
        protected EnemyStateMachine _stateMachine;

        protected EnemyState(Enemy enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}
