using System;
using System.Collections;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.ECS;
using ConjureKitShooter.Models;
using UnityEngine;

public class HostilesSystem : SystemBase
{
    private const string HostileComponent = "HOSTILE.COMPONENT";
    private const string HitFxComponent = "HIT.FX.COMPONENT";

    private uint _hostileComponentTypeId;
    private uint _hitFxComponentTypeId;
    
    private readonly byte[] _emptyByte = Array.Empty<byte>();
    
    public event Action<SpawnData> InvokeSpawnHostile;
    public event Action<HitData> InvokeHitFx;
    public event Action<uint> InvokeDestroyHostile;

    public HostilesSystem(Session session) : base(session)
    {

    }

    public override string[] GetComponentTypeNames()
    {
        return new[] {HostileComponent, HitFxComponent};
    }

    public override void Update(IReadOnlyList<(EntityComponent component, bool localChange)> updated)
    {
        foreach (var c in updated)
        {
            if (c.component.ComponentTypeId == _hostileComponentTypeId)
            {
                var entity = _session.GetEntity(c.component.EntityId);
                var payload = c.component.Data.FromJsonByteArray<HostileData>();
                var spawnData = new SpawnData()
                {
                    startPos = _session.GetEntityPose(entity.Id).position,
                    targetPos = payload.TargetPos.ToVector3(),
                    linkedEntity = entity,
                    speed = payload.Speed,
                    timestamp = payload.TimeStamp,
                    type = payload.Type
                };
                InvokeSpawnHostile?.Invoke(spawnData);
                continue;
            }
            
            if (c.component.ComponentTypeId == _hitFxComponentTypeId)
            {
                if (c.component.Data == SharedValues.EmptyByte) return;
                
                var data = c.component.Data.FromJsonByteArray<HitData>();

                if (data == null)
                    data = new HitData(){ EntityId = c.component.EntityId };
                
                InvokeHitFx?.Invoke(data);
            }
        }
    }

    public override void Delete(IReadOnlyList<(EntityComponent component, bool localChange)> deleted)
    {
        foreach (var c in deleted)
        {
            if (c.component.ComponentTypeId == _hostileComponentTypeId)
            {
                var entityId = c.component.EntityId;
                InvokeDestroyHostile?.Invoke(entityId);
            }
        }
    }
    
    public void GetComponentsTypeId()
    {
        _session.GetComponentTypeId(HostileComponent, u => _hostileComponentTypeId = u, error =>
        {
            Debug.LogError(error.TagString());
        });
        _session.GetComponentTypeId(HitFxComponent, u => _hitFxComponentTypeId = u, error =>
        {
            Debug.LogError(error.TagString());
        });
    }
    
    public void AddHostile(Entity entity, float speed, uint targetEntityId)
    {
        //defining and initializing payload values
        var targetPos = _session.GetEntityPose(targetEntityId).position;
        var types = Enum.GetValues(typeof(HostileType));
        var payload = new HostileData()
        {
            Speed = speed,
            TargetPos = new SVector3(targetPos),
            TimeStamp = DateTime.UtcNow.Ticks,
            Type = (HostileType)types.GetValue(UnityEngine.Random.Range(0, types.Length))
        }.ToJsonByteArray();

        //add the related component to each Hostile entity, along with the payload
        _session.AddComponent(_hostileComponentTypeId, entity.Id, payload, null,
            error => Debug.LogError(error.TagString()));
        _session.AddComponent(_hitFxComponentTypeId, entity.Id, _emptyByte, null,
            error => Debug.LogError(error.TagString()));
    }

    public void DeleteHostile(uint entityId)
    {
        _session.DeleteComponent(_hostileComponentTypeId, entityId, null,
            error => Debug.LogError(error.TagString()));
    }
    
    public void SyncHitFx(uint entityId, Vector3 hitPos)
    {
        var hostileEntity = _session.GetEntity(entityId);
        if (hostileEntity == null) return;

        var data = new HitData()
        {
            EntityId = entityId,
            Pos = new SVector3(hitPos),
        };
        var jsonData = data.ToJsonByteArray();
        _session.UpdateComponent(_hitFxComponentTypeId, entityId, jsonData);
    }
}