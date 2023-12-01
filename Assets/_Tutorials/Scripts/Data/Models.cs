using System;
using Auki.ConjureKit;
using ConjureKitShooter.UI;
using UnityEngine;

namespace ConjureKitShooter.Models
{
    #region Serializable Pose, Vector and Quaternion
    [Serializable]
    public struct SPose
    {
        public SVector3 pos;
        public SQuaternion rot;

        public SPose(Pose pose)
        {
            pos = new SVector3(pose.position);
            rot = new SQuaternion(pose.rotation);
        }

        public Pose ToUnityPose()
        {
            return new Pose(pos.ToVector3(), rot.ToQuaternion());
        }
    }

    [Serializable]
    public struct SVector3
    {
        public float px;
        public float py;
        public float pz;
        
        public SVector3(Vector3 pos)
        {
            px = pos.x;
            py = pos.y;
            pz = pos.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(px, py, pz);
        }
    }
    
    [Serializable]
    public struct SQuaternion
    {
        public float rx;
        public float ry;
        public float rz;
        public float rw;
        
        public SQuaternion(Quaternion rot)
        {
            rx = rot.x;
            ry = rot.y;
            rz = rot.z;
            rw = rot.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(rx, ry, rz, rw);
        }
    }
    #endregion
    
    [Serializable]
    public class ShootData
    {
        public SVector3 StartPos;
        public SVector3 EndPos;
    }
    
    [Serializable]
    public class HitData
    {
        public uint EntityId;
        public SVector3 Pos;
    }
    
    [Serializable]
    public class HostileData
    {
        public float Speed;
        public SVector3 TargetPos;
        public long TimeStamp;
        public HostileType Type;
    }

    [Serializable]
    public class SpawnData
    {
        public Vector3 startPos;
        public Vector3 targetPos;
        public Entity linkedEntity;
        public float speed;
        public long timestamp;
        public HostileType type;
    }

    [Serializable]
    public class ScoreData
    {
        public string name;
        public int score;
    }

    public class ParticipantComponent
    {
        public LineRenderer LineRenderer;
        public ParticipantNameUi NameUi;
        public int Score;

        public ParticipantComponent(LineRenderer lineRenderer, ParticipantNameUi nameUi, int score = 0)
        {
            LineRenderer = lineRenderer;
            NameUi = nameUi;
            Score = score;
        }
    }

    public enum GameState
    {
        Intro,
        PlaceSpawner,
        WaitToStart,
        GameOn
    }

    public enum HostileType
    {
        Ghost,
        Pumpkin
    }
}