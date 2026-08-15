using UnityEngine;

public class HorrorLightFlicker : MonoBehaviour
{
    public Light pointLight;
    public float normalIntensity = 0.5f;

    void Start()
    {
        if (pointLight == null)
            pointLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            pointLight.intensity = normalIntensity;

            yield return new WaitForSeconds(Random.Range(2f, 6f));

            int flashes = Random.Range(2, 5);

            for (int i = 0; i < flashes; i++)
            {
                pointLight.intensity = Random.Range(0f, 0.15f);
                yield return new WaitForSeconds(0.03f);

                pointLight.intensity = normalIntensity;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}