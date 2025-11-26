using UnityEngine;

public class InventoryBounds : MonoBehaviour
{
    [SerializeField] private InventoryManager _inventoryManager;
    private void OnTriggerExit(Collider other)
    {
        //if (other.TryGetComponent(out LitterBehaviour litterScript))
        //{
        //    _inventoryManager.RemoveLitterObject(litterScript);
        //}
        _inventoryManager.RemoveLitterSimObject(other.gameObject);
        //Debug.Log(other.name);  
    }
}
