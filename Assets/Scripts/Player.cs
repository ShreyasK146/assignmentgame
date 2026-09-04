using Unity.Cinemachine;
using UnityEngine;
public class Player : BaseMovement
{
    protected override void Start()
    {
        base.Start();
        virtualCamera = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        virtualCamera.Target.TrackingTarget = transform;
        GameEvents.Instance.onTileClicked += MoveToTarget;
    }
    private void OnDisable()
    {
        GameEvents.Instance.onTileClicked -= MoveToTarget;
    }
    protected override void OnMoveFinished()
    {
        //GameEvents.Instance.PlayerMoved(currentTile);
        GameManager.Instance.NotifyEnemyOfPlayerMove(currentTile); // signals player move complete to enemy
    }
}
