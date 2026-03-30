using UnityEngine;
using UnityEngine.Events;

public class PaintballBehaviour : MonoBehaviour
{
    public LayerMask groundLayer = new();
    public UnityEvent<GameObject> onExplode = new();
    private float _hue;
    private Renderer _renderer;
    public void InitMaterial(float hue)
    {
        _hue = hue;
        _renderer = GetComponent<Renderer>();


        Color color = Color.HSVToRGB((hue + 15  )/ 360f, 1, 1);
        _renderer.material.SetColor("_BaseColor", color);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        //if (collision.gameObject.layer) return;
        if (!((groundLayer.value & (1 << collision.gameObject.layer)) != 0)) return;

        Destroy(gameObject);
        onExplode.Invoke(gameObject);
        return;
    }
}
