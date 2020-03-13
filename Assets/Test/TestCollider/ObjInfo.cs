using System;
using System.Collections;
using System.Collections.Generic;
using Sword;
using UnityEngine;
using XLua;

public class ObjInfo : MonoBehaviour
{
    private Vector3 originPos;
    private Vector3 lastPos;
    private Vector3 colliderPos;
    private Vector3 currentPos;
    private Collider collider;
    private Color originColor;
    private bool isInCollider = false;

    private bool limitX = false;
    private bool limitY = false;
    private bool limitZ = false;

    private float boardUnit = 0.5f;

    private GameObject signCollider;

    // Start is called before the first frame update
    void Start()
    {
        originPos = transform.position;
        lastPos = transform.position;
        currentPos = transform.position;

        originColor = GetComponent<MeshRenderer>().material.color;

        limitY = true;

        InitCollider();
    }
    
    public void InitCollider()
    {
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.drag = 0;
        rigidbody.angularDrag = 0;
        rigidbody.useGravity = false;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
//        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll | RigidbodyConstraints.FreezePositionY;

        signCollider = GameObject.CreatePrimitive(PrimitiveType.Plane);
        signCollider.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
        signCollider.GetComponent<MeshRenderer>().material.color = Color.green;
        signCollider.transform.position = new Vector3(currentPos.x, 0, currentPos.z);
        MeshCollider col = signCollider.GetComponent<MeshCollider>();
        DestroyImmediate(col);
    }

    public void MoveOffset(Vector2 offset)
    {
        float x = Mathf.Floor(offset.x * 0.01f / boardUnit) * boardUnit;
        float y = Mathf.Floor(offset.y * 0.01f / boardUnit) * boardUnit;
        
        Vector3 offset3 = new Vector3(x, 0, y);
        SetCurrentPosition(lastPos + offset3);
    }

    public void ApplayOffset()
    {
        lastPos = transform.position;
    }

    public void Select()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
    }

    public void UnSelect()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = originColor;
    }

    public void SetCurrentPosition(Vector3 position)
    {
        if (!position.Equals(currentPos))
        {
            Vector3 offset = position - currentPos;
            bool hit1 = Physics.Raycast(transform.position, Vector3.right, offset.x, 1 << LayerMask.NameToLayer("Collider"));
            if (hit1 )
            {
                return;
            }
            
            transform.position = position;
            currentPos = transform.position;
            if (signCollider)
            {
                signCollider.transform.position = new Vector3(currentPos.x, 0, currentPos.z);
            }
        }
    }

    #region 碰撞触发器

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTrigger Enter -------- {name}");
        signCollider.GetComponent<MeshRenderer>().material.color = Color.red;
        colliderPos = transform.position;
//        SetCurrentPosition(colliderPos);
        isInCollider = true;
    }

    private void OnTriggerStay(Collider other)
    {
//        SetCurrentPosition(colliderPos);
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        isInCollider = false;
        Debug.Log($"OnTrigger Exit -------- {name}");  
        signCollider.GetComponent<MeshRenderer>().material.color = Color.green;
//        RemoveLimitBorder(other.gameObject.GetComponent<RoomBorder>());
    }
    #endregion
    
}
