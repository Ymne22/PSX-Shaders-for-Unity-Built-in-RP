using UnityEngine;

public class ObjectPlayerFollow : MonoBehaviour
{
    [Tooltip("The tag of the player GameObject to follow.")]
    public string playerTag = "Player";

    public Vector3 offset = new Vector3(0, 0, 0);

    private Transform playerTransform;
    private Vector3 initialLocalRotation;

    void Start()
    {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerGameObject != null)
        {
            playerTransform = playerGameObject.transform;
        }
        else
        {
            Debug.LogError($"No GameObject found with tag '{playerTag}'. Please ensure your player has this tag.", this);
            enabled = false;
            return;
        }
        initialLocalRotation = transform.localEulerAngles;
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
        else
        {
            Debug.LogWarning("No ParticleSystem found on this GameObject. This script might not be needed here.", this);
        }
    }
    void LateUpdate()
    {
        if (playerTransform == null)
        {
            enabled = false;
            return;
        }
        transform.position = playerTransform.position + offset;
        transform.localEulerAngles = initialLocalRotation;
    }
}