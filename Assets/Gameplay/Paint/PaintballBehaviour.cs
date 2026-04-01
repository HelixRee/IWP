using UnityEngine;
using UnityEngine.Events;

public class PaintballBehaviour : MonoBehaviour
{
    public LayerMask groundLayer = new();
    public UnityEvent<GameObject> onExplode = new();
    private float _hue;
    private Renderer _renderer;
    private Color _storedColor;
    public void InitMaterial(float hue)
    {
        _hue = hue;
        _renderer = GetComponent<Renderer>();


        Color color = Color.HSVToRGB((hue + 15  )/ 360f, 1, 1);
        _renderer.material.SetColor("_BaseColor", color);

        _storedColor = color;
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1);
        foreach (Collider collider in colliders) 
        {
            if (!(collider.TryGetComponent(out Paintable paintable))) continue;

            PaintManager.instance.Paint(paintable, transform.position, 0.6f, 1f, 1f, _storedColor);
            
        }

        Destroy(gameObject);
        onExplode.Invoke(gameObject);
        return;
    }
}
