using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PaintParticles : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float hardness = 0.5f;
    [SerializeField] private float strength = 0.5f;
    [SerializeField] private Color paintColor = Color.red;

    private ParticleSystem main;
    private List<ParticleCollisionEvent> collisionEvents;
    private float rateOverDistance;


    // Start is called before the first frame update
    void Awake()
    {
        main = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();

        if (!isActive)
            main.Pause(true);
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
            main.Play();
        else
            main.Pause(true);

        this.isActive = isActive;
    }


    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = main.GetCollisionEvents(other, collisionEvents);
        //Debug.Log("Hit");
        Paintable paintable = other.GetComponent<Paintable>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            if (paintable)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float paintAmount = Mathf.Pow(main.main.startSize.constant, 3) * 13f;
                float puddleRadius = Mathf.Sqrt(paintAmount);
                puddleRadius = Mathf.Min(puddleRadius, 3);
                PaintManager.instance.Paint(paintable, pos, puddleRadius, hardness, strength, paintColor);


            }
            i++;
        }
    }
}
