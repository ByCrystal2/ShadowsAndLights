
using StateMachineSystem;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerUI))]
public class CharacterBehaviour : MonoBehaviour
{
    [HideInInspector] public PlayerUI playerUI;
    StateMachine stateMachine;
    [SerializeField] Transform collectableItemsContent;
    private void Awake()
    {
        stateMachine = new StateMachine();
        playerUI = GetComponent<PlayerUI>();
    }
    public void StatesInit()
    {
        IState idleState = new IdleState();
        IState walkState = new WalkState();
        IState runState = new RunState();
        IState attackState = new AttackState();
        IState dyingState = new DyingState();

        StateTransition idleToWalk = new StateTransition(idleState, walkState, () => true);
        StateTransition walkToIdle = new StateTransition(walkState, idleState, () => true);

        StateTransition idleToRun = new StateTransition(idleState, runState, () => true);
        StateTransition walkToRun = new StateTransition(walkState, runState, () => true);

        StateTransition runToIdle = new StateTransition(runState, idleState, () => true);
        StateTransition runToWalk = new StateTransition(runState, walkState, () => true);

        StateTransition walkToAttack = new StateTransition(walkState, attackState, () => true);
        StateTransition runToAttack = new StateTransition(runState, attackState, () => true);

        StateTransition anyToDying = new StateTransition(null, dyingState, () => true);

        stateMachine.SetStates(idleToWalk);
        stateMachine.SetStates(walkToIdle);
        stateMachine.SetStates(idleToRun);
        stateMachine.SetStates(walkToRun);
        stateMachine.SetStates(runToIdle);
        stateMachine.SetStates(runToWalk);
        stateMachine.SetStates(walkToAttack);
        stateMachine.SetStates(runToAttack);

        stateMachine.SetAnyStates(anyToDying);

        stateMachine.SetState(idleState);
    }
    public void CollectObject(ICollectable _collectableObj, GameObject _obj)
    {
        if (HandsFull()) return;
        if (_collectableObj.IsCollected)
        {
            Debug.Log("Obje zaten alinmis. Obje Name => " + _obj.name);
            return;
        }
        if (_collectableObj is ICollectHand)
        {
            _obj.transform.SetParent(collectableItemsContent);
            Transform objTrans = _obj.transform;
            objTrans.transform.localPosition = Vector3.zero;
            objTrans.localRotation = new Quaternion(objTrans.rotation.x,Vector3.forward.z, objTrans.rotation.z, objTrans.localRotation.w);
            playerUI.CloseInteractUIS();
            playerUI.ShowDropable();
            Debug.Log(_obj.name + " adli obje alindi.");
        }
        _collectableObj.IsCollected = true;
    }
    public bool HandsFull()
    {
        return collectableItemsContent.transform.childCount > 0 ? true : false;
    }
    float targetTouchTime = 2f, currentTouchTime = 0;
    private Vector2 lastTouchPosition;
    IInteractable currentHandObject;
    IRotateAnObject currentRotatingObject;
    bool isRotatableObjRotating = true;
    private void Update()
    {
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray,out hit, 100f))
        //    {
        //        GameObject hitObject = hit.collider.gameObject;
        //        if (hitObject.CompareTag("Interactable"))
        //        {
        //        Debug.Log("Interactable object name => " + hit.collider.gameObject.name);
        //            currentTouchTime += Time.deltaTime;
        //            IInteractable interact = hitObject.GetComponent<IInteractable>();
        //            if (interact != null) 
        //            {
        //                if (interact is IRotateAnObject rotateObj)
        //                {
        //                    if (!HandsFull() && !isRotatableObjRotating)
        //                    {
        //                        if (currentTouchTime >= targetTouchTime/2.5f)
        //                        {

        //                            MainUIManager.instance.LockPlayer();
        //                            currentRotatingObject = rotateObj;
        //                            isRotatableObjRotating = true;                                    
        //                        }                                
        //                    }
        //                }
        //                if (currentTouchTime >= targetTouchTime)
        //                {
        //                    if (!HandsFull())
        //                    {
        //                        isRotatableObjRotating = false;
        //                        interact.Interact(InteractType.Pickable);
        //                        currentHandObject = interact;
        //                        targetTouchTime = 2;
        //                        currentTouchTime = 0;
        //                        MainUIManager.instance.UnLockPlayer();
        //                    }
        //                    else
        //                    {

        //                    }
        //                    return;
        //                }
        //                if (currentTouchTime >= targetTouchTime/2)
        //                {
        //                    if (HandsFull())
        //                    {
        //                        isRotatableObjRotating = false;
        //                        interact.Interact(InteractType.Dropable);
        //                        currentHandObject = interact;
        //                        targetTouchTime = 2;
        //                        currentTouchTime = 0;
        //                        MainUIManager.instance.UnLockPlayer();
        //                    }
        //                }
        //            }
        //        }
        //        if (isRotatableObjRotating)
        //        {
        //            if (currentRotatingObject != null)
        //            {
        //                Debug.Log(currentRotatingObject + " adli obje'nin " + currentRotatingObject.RotatableObject + " adli rotate objesi donduruluyor...");
        //                Vector2 deltaPosition = touch.position - lastTouchPosition;
        //                float rotationAmount = deltaPosition.x * 0.5f; // Dönme hýzýný ayarlamak için çarpan kullanabilirsiniz.
        //                currentRotatingObject.RotatableObject.RotateY(rotationAmount);
        //                lastTouchPosition = touch.position;
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    if (isRotatableObjRotating)
        //    {
        //        isRotatableObjRotating = false;
        //        MainUIManager.instance.UnLockPlayer();
        //    }

        //    if (targetTouchTime != 2 || currentTouchTime != 0)
        //    {
        //        targetTouchTime = 2;
        //        currentTouchTime = 0;
        //    }
        //}
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            ResetTouchState();
        }
        if (isRotatableObjRotating && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            RotateObject(touch);
        }
    }
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (Vector3.Distance(transform.position, hitObject.transform.position) > 3f) return;
            if (hitObject.CompareTag("Interactable"))
            {
                Debug.Log("Interactable object name => " + hitObject.name);
                currentTouchTime += Time.deltaTime;
                IInteractable interact = hitObject.GetComponent<IInteractable>();
                if (interact != null)
                {
                    HandleInteractableObject(interact, touch);
                }
            }
        }
    }

    private void HandleInteractableObject(IInteractable interact, Touch touch)
    {
        if (interact is IRotateAnObject rotateObj)
        {
            HandleRotatableObject(rotateObj);
        }
        MainUIManager.instance.LockPlayer();
        if (currentTouchTime >= targetTouchTime)
        {
            if (!HandsFull())
            {
                PickUpObject(interact);
            }
        }
        else if (currentTouchTime >= targetTouchTime / 2)
        {
            if (HandsFull())
            {
                DropObject(interact);
            }
        }
    }

    private void HandleRotatableObject(IRotateAnObject rotateObj)
    {
        if (!HandsFull() && !isRotatableObjRotating)
        {
                MainUIManager.instance.LockPlayer();
            if (currentTouchTime >= targetTouchTime / 2.5f)
            {
                currentRotatingObject = rotateObj;
                isRotatableObjRotating = true;
                lastTouchPosition = Input.GetTouch(0).position;
            }
        }
    }

    private void PickUpObject(IInteractable interact)
    {
        isRotatableObjRotating = false;
        interact.Interact(InteractType.Pickable);
        currentHandObject = interact;
        ResetTouchState();
        MainUIManager.instance.UnLockPlayer();
    }

    private void DropObject(IInteractable interact)
    {
        isRotatableObjRotating = false;
        interact.Interact(InteractType.Dropable);
        currentHandObject = interact;
        ResetTouchState();
        MainUIManager.instance.UnLockPlayer();
    }

    private void RotateObject(Touch touch)
    {
        if (currentRotatingObject != null)
        {
            Debug.Log(currentRotatingObject + " adli obje'nin " + currentRotatingObject.RotatableObject + " adli rotate objesi donduruluyor...");
            Vector2 deltaPosition = touch.position - lastTouchPosition;
            float rotationAmount = deltaPosition.x * 0.5f; // Dönme hýzýný ayarlamak için çarpan kullanabilirsiniz.
            currentRotatingObject.RotatableObject.RotateX(rotationAmount);
            lastTouchPosition = touch.position;
        }
    }

    private void ResetTouchState()
    {
        if (isRotatableObjRotating)
        {
            isRotatableObjRotating = false;
        }

        if (targetTouchTime != 2f || currentTouchTime != 0f)
        {
            targetTouchTime = 2f;
            currentTouchTime = 0f;
        }
            MainUIManager.instance.UnLockPlayer();
    }
    public List<GameObject> triggerInteracts = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            triggerInteracts.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable") && !HandsFull())
            playerUI.CurrentInteract = null;
        triggerInteracts.Remove(other.gameObject);
    }
}