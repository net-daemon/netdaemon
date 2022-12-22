namespace NetDaemon.HassModel.Entities;

public interface IEntityState
{
    string EntityId { get; }

    string? RawState { get; }

    JsonElement? AttributesJson { get; }

    DateTime? LastChanged { get; }

    DateTime? LastUpdated { get; }

    Context? Context { get; }
}

public interface IEntityState<TState, TAttributes> : IEntityState
    where TAttributes : class
{
    TState State { get; }

    TAttributes Attributes { get; }
}