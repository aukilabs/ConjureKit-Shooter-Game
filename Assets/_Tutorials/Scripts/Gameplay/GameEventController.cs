using System;
using Auki.ConjureKit;
using Auki.ConjureKit.Vikja;
using ConjureKitShooter.Models;
using UnityEngine;

namespace ConjureKitShooter.Gameplay
{
    public class GameEventController
    {
        private IConjureKit _conjureKit;
        private Vikja _vikja;

        private uint _myEntityId;

        private const string NotifyGameState = "NOTIFY.GAME.STATE";
        private const string NotifySpawnerPos = "NOTIFY.SPAWNER.POS";

        public Action OnGameOver, OnGameStart;
        public Action<Pose> OnSpawnerMove;

        #region Public Methods
        public void Initialize(IConjureKit conjureKit, Vikja vikja)
        {
            _conjureKit = conjureKit;
            _vikja = vikja;
            _vikja.OnEntityAction += OnEntityAction;
            _conjureKit.OnParticipantEntityCreated += SetMyEntityId;
        }

        public void SendGameState(bool start)
        {
            _vikja.RequestAction(_myEntityId, NotifyGameState , start.ToJsonByteArray(), null, null);
        }

        public void SendSpawnerPos(Pose pose)
        {
            _vikja.RequestAction(_myEntityId, NotifySpawnerPos, new SPose(pose).ToJsonByteArray(), action =>
            {
                OnSpawnerMove?.Invoke(action.Data.FromJsonByteArray<SPose>().ToUnityPose());
            }, null);
        }
        #endregion

        #region Private Methods
        private void SetMyEntityId(Entity entity)
        {
            _myEntityId = entity.Id;
        }
        private void OnEntityAction(EntityAction obj)
        {
            switch (obj.Name)
            {
                case NotifyGameState:
                    var gameOn = obj.Data.FromJsonByteArray<bool>();
                    if (gameOn)
                        OnGameStart?.Invoke();
                    else
                        OnGameOver?.Invoke();
                    break;
                case NotifySpawnerPos:
                    OnSpawnerMove?.Invoke(obj.Data.FromJsonByteArray<SPose>().ToUnityPose());
                    break;
            }
        }
        #endregion
    }
}