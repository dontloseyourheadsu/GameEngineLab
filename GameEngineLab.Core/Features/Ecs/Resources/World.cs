using System;
using System.Collections.Generic;
using GameEngineLab.Core.Features.Ecs.Components;
using GameEngineLab.Core.Features.Ecs.Entities;

namespace GameEngineLab.Core.Features.Ecs.Resources;

public sealed class World
{
    private readonly Dictionary<Type, Dictionary<int, object>> _componentStores = new();
    private readonly Dictionary<Type, object> _resources = new();
    private readonly HashSet<int> _aliveEntities = new();
    private int _nextEntityId = 1;

    public EntityId CreateEntity()
    {
        EntityId entityId = new(_nextEntityId++);
        _aliveEntities.Add(entityId.Value);
        return entityId;
    }

    public bool IsAlive(EntityId entityId) => _aliveEntities.Contains(entityId.Value);

    public void DestroyEntity(EntityId entityId)
    {
        if (!_aliveEntities.Remove(entityId.Value))
        {
            return;
        }

        foreach (Dictionary<int, object> store in _componentStores.Values)
        {
            store.Remove(entityId.Value);
        }
    }

    public void SetComponent<TComponent>(EntityId entityId, TComponent component)
        where TComponent : struct, IComponent
    {
        if (!IsAlive(entityId))
        {
            throw new InvalidOperationException($"Entity {entityId} is not alive.");
        }

        Dictionary<int, object> store = GetOrCreateStore<TComponent>();
        store[entityId.Value] = component;
    }

    public bool TryGetComponent<TComponent>(EntityId entityId, out TComponent component)
        where TComponent : struct, IComponent
    {
        component = default;

        if (!_componentStores.TryGetValue(typeof(TComponent), out Dictionary<int, object>? store))
        {
            return false;
        }

        if (!store.TryGetValue(entityId.Value, out object? boxedComponent) || boxedComponent is not TComponent typed)
        {
            return false;
        }

        component = typed;
        return true;
    }

    public bool HasComponent<TComponent>(EntityId entityId)
        where TComponent : struct, IComponent
    {
        return _componentStores.TryGetValue(typeof(TComponent), out Dictionary<int, object>? store)
               && store.ContainsKey(entityId.Value);
    }

    public bool RemoveComponent<TComponent>(EntityId entityId)
        where TComponent : struct, IComponent
    {
        return _componentStores.TryGetValue(typeof(TComponent), out Dictionary<int, object>? store)
               && store.Remove(entityId.Value);
    }

    public IEnumerable<EntityId> GetEntities()
    {
        foreach (int entity in _aliveEntities)
        {
            yield return new EntityId(entity);
        }
    }

    public IEnumerable<EntityId> GetEntitiesWith<TComponent>()
        where TComponent : struct, IComponent
    {
        if (!_componentStores.TryGetValue(typeof(TComponent), out Dictionary<int, object>? store))
        {
            yield break;
        }

        foreach (int entity in store.Keys)
        {
            if (_aliveEntities.Contains(entity))
            {
                yield return new EntityId(entity);
            }
        }
    }

    public IEnumerable<EntityId> GetEntitiesWith<TFirst, TSecond>()
        where TFirst : struct, IComponent
        where TSecond : struct, IComponent
    {
        foreach (EntityId entityId in GetEntitiesWith<TFirst>())
        {
            if (HasComponent<TSecond>(entityId))
            {
                yield return entityId;
            }
        }
    }

    public void SetResource<TResource>(TResource resource)
        where TResource : class
    {
        ArgumentNullException.ThrowIfNull(resource);
        _resources[typeof(TResource)] = resource;
    }

    public bool TryGetResource<TResource>(out TResource? resource)
        where TResource : class
    {
        resource = default;
        if (!_resources.TryGetValue(typeof(TResource), out object? boxedResource) || boxedResource is not TResource typed)
        {
            return false;
        }

        resource = typed;
        return true;
    }

    public TResource GetRequiredResource<TResource>()
        where TResource : class
    {
        if (!TryGetResource<TResource>(out TResource? resource) || resource is null)
        {
            throw new InvalidOperationException($"Resource {typeof(TResource).Name} is not registered.");
        }

        return resource;
    }

    private Dictionary<int, object> GetOrCreateStore<TComponent>()
        where TComponent : struct, IComponent
    {
        Type componentType = typeof(TComponent);
        if (_componentStores.TryGetValue(componentType, out Dictionary<int, object>? store))
        {
            return store;
        }

        store = new Dictionary<int, object>();
        _componentStores[componentType] = store;
        return store;
    }
}
