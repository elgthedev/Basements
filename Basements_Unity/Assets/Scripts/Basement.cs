using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Basements
{
    class Basement : MonoBehaviour
    {
        Bounds interiorBounds;
        Collider[] localColliders;

        public static List<Basement> allBasements;

        GameObject b;

        void Awake()
        {
            if (allBasements == null) allBasements = new List<Basement>();
            allBasements.Add(this);

            localColliders = gameObject.GetComponentsInChildren<Collider>();

            b = transform.Find("Interior/Bounds").gameObject;
            b.layer = 4; // Allows building without disabling zone detection, idk what this layer is actually for
            interiorBounds = b.GetComponent<BoxCollider>().bounds;
        }

        void OnDestroy()
        {
            allBasements.Remove(this);
        }

        public bool CanBeRemoved()
        {
            var ol = Physics.OverlapBox(interiorBounds.center, interiorBounds.extents).Where(x => !localColliders.Contains(x));
            foreach (var item in ol)
            {
                Debug.LogError(item.name + " is preventing basement from being destroyed");
            }
            return !ol.Any();            
        } 
    }
}