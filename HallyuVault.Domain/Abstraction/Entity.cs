namespace HallyuVault.Domain.Abstraction
{
    // override equals, implement IEqualtable interface if needed when comparaing entities
    public abstract class Entity : IDomainEvent
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public int Id { get; init; }

        protected Entity(int id)
        {
            Id = id;
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents;
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
