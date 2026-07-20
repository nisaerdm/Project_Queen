using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private CheckpointManager checkpointManager;

    public void Initialize(CheckpointManager manager)
    {
        checkpointManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Car"))
        {
            checkpointManager.PlayerThroughCheckpoint(this, other.transform.root);
        }
    }
}