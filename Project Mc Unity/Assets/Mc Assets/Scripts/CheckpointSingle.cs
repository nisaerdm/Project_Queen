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
        // KONTROL: Çarpan obje "Player" (Sen) veya "Car" (AI Bot) ise kabul et
        if (other.CompareTag("Player") || other.CompareTag("Car"))
        {
            // Arabanın tekerleği bile çarpsa, .root ile her zaman kasanın kök objesini yolla
            checkpointManager.PlayerThroughCheckpoint(this, other.transform.root);
        }
    }
}