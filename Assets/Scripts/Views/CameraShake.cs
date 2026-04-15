using System.Collections;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private Vector3 originalPosition;

    private void Awake()
    {
        instance = this;
        originalPosition = transform.localPosition;
    }

    public static void Shake(float magnitude, float duration)
    {
        // Dotween shake
        instance?.transform.DOShakePosition(duration, magnitude, vibrato: 30, randomness: 90);
        
        // Coroutine shake
        // instance?.StartCoroutine(instance.DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
