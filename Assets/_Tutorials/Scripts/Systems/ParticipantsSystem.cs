using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.ECS;
using ConjureKitShooter.Models;
using UnityEngine;

public class ParticipantsSystem: SystemBase
{
    private byte[] _emptyData = new byte[1];

    private uint _scoreComponentTypeId;
    private uint _shootFxComponentTypeId;

    public event Action<uint, ScoreData> OnParticipantScores;
    public event Action<uint, ShootData> InvokeShootFx;
    
    private const string ScoreComponent = "SCORE.COMPONENT";
    private const string ShootFxComponent = "SHOOT.FX.COMPONENT";

    public ParticipantsSystem(Session session) : base(session)
    {

    }

    /// <summary>
    /// Method to generate the components type Id with type of uint
    /// </summary>
    public void GetComponentsTypeId()
    {
        _session.GetComponentTypeId(ScoreComponent, id => _scoreComponentTypeId = id,
            error => Debug.LogError(error.TagString()));
        _session.GetComponentTypeId(ShootFxComponent, id => _shootFxComponentTypeId = id,
            error => Debug.LogError(error.TagString()));
    }

    public override string[] GetComponentTypeNames()
    {
        return new[] {ScoreComponent, ShootFxComponent};
    }

    public override void Update(IReadOnlyList<(EntityComponent component, bool localChange)> updated)
    {
        foreach (var c in updated)
        {
            if (c.component.ComponentTypeId == _scoreComponentTypeId)
            {
                var data = c.component.Data.FromJsonByteArray<ScoreData>();
                OnParticipantScores?.Invoke(c.component.EntityId, data);
            }
            
            if (c.component.ComponentTypeId == _shootFxComponentTypeId)
            {
                if (c.localChange) return;

                var data = c.component.Data.FromJsonByteArray<ShootData>();
                if (data == null) return;
                InvokeShootFx?.Invoke(c.component.EntityId, data);
            }
        }
    }

    public override void Delete(IReadOnlyList<(EntityComponent component, bool localChange)> deleted)
    {

    }

    public void GetAllScoresComponent(Action<List<EntityComponent>> onComplete)
    {
        _session.GetComponents(_scoreComponentTypeId, result =>
        {
            onComplete?.Invoke(result);
        }, error =>
        {
            Debug.LogError(error);
            onComplete?.Invoke(null);
        });
    }

    public void AddParticipantComponent(uint entityId, string name)
    {
        var data = new ScoreData()
        {
            name = name
        }.ToJsonByteArray();
        var ec = _session.GetEntityComponent(entityId, _scoreComponentTypeId);
        if (ec != null)
        {
            _session.UpdateComponent(_scoreComponentTypeId, entityId, data);
            return;
        }
        _session.AddComponent(_scoreComponentTypeId, entityId, data, null, Debug.LogError);
        _session.AddComponent(_shootFxComponentTypeId, entityId, _emptyData, null, Debug.LogError);
    }
    
    public void UpdateParticipantScoreComponent(uint entityId, int score)
    {
        var prevData = _session.GetEntityComponent(entityId, _scoreComponentTypeId);
        var data = prevData.Data.FromJsonByteArray<ScoreData>();
        data.score = score;
        _session.UpdateComponent(_scoreComponentTypeId, entityId, data.ToJsonByteArray());
    }
    
    public void SyncShootFx(uint entityId, Vector3 pos, Vector3 hit)
    {
        var data = new ShootData()
        {
            StartPos = new SVector3(pos),
            EndPos = new SVector3(hit)
        }.ToJsonByteArray();
        _session.UpdateComponent(_shootFxComponentTypeId, entityId, data);
    }
}