using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Basements
{
    class Basement : MonoBehaviour
    {
        Bounds interiorBounds;
        Collider[] localColliders;

        public static List<Basement> allBasements;

        [SerializeField]public GameObject BoundsObject;
        [SerializeField]public BoxCollider box;
        void Awake()
        {
            if (allBasements == null) allBasements = new List<Basement>();
            allBasements.Add(this);
            localColliders = gameObject.GetComponentsInChildren<Collider>();
            if (BoundsObject == null)
            {
                BoundsObject = transform.Find("Bounds").gameObject;
                box = BoundsObject.GetComponent<BoxCollider>();
            }
            BoundsObject.layer = 4; // Allows building without disabling zone detection, idk what this layer is actually for
            interiorBounds = box.bounds;
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
                if (item.gameObject.GetComponent<ItemDrop>() != null)
                {
                    BasementsMod.WriteLog(Localization.instance.Localize(item.gameObject.GetComponent<ItemDrop>().m_itemData.m_shared.m_name+" is preventing basement from being destroyed"), WarnLevel.All);
                }

                if (item.gameObject.GetComponent<Piece>() != null)
                {
                    BasementsMod.WriteLog(Localization.instance.Localize(item.gameObject.GetComponent<Piece>().m_name) + " is preventing basement from being destroyed", WarnLevel.All);
                }
                else
                {
                    BasementsMod.WriteLog(item.name + " is preventing basement from being destroyed", WarnLevel.All);
                }
                
            }
            return !ol.Any();            
        } 
    }
}