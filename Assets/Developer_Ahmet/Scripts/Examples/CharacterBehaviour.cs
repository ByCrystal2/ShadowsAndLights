using NUnit.Framework.Constraints;
using StateMachineSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerUI),typeof(InventoryHandler))]
public class CharacterBehaviour : MonoBehaviour
{
    [HideInInspector] public PlayerUI playerUI;
    StateMachine stateMachine;
    [SerializeField] Transform collectableItemsContent;
    
    [SerializeField] private CarryState carryState;
    InventoryHandler inventory;
    private void Awake()
    {
        carryState = CarryState.PlayerFree;
        stateMachine = new StateMachine();
        playerUI = GetComponent<PlayerUI>();    
        inventory = GetComponent<InventoryHandler>();
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
        
        if (_collectableObj is ICollectHand && _collectableObj.IsCollected)
        {
            Debug.Log("Obje zaten alinmis. Obje Name => " + _obj.name);
            return;
        }
        if (_collectableObj is ICollectHand)
        {
            if (HandsFull()) return;
            _obj.transform.SetParent(collectableItemsContent);
            Transform objTrans = _obj.transform;
            objTrans.localPosition = Vector3.zero;
            //objTrans.localRotation = new Quaternion(objTrans.rotation.x,Vector3.forward.z, objTrans.rotation.z, objTrans.localRotation.w);
            playerUI.CloseInteractUIS();
            playerUI.ShowDropable();
            Debug.Log(_obj.name + " adli obje alindi.");
        }
        else if (_collectableObj is ICollectInventory inventoryObj)
        {
            inventory.AddInventory(inventoryObj);
            _obj.gameObject.SetActive(false);
        }
        _collectableObj.IsCollected = true;
    }
    public bool HandsFull()
    {
        return collectableItemsContent.transform.childCount > 0 ? true : false;
    }
    float targetTouchTime = 1.5f, currentTouchTime = 0;
    private Vector2 lastTouchPosition;
    IInteractable lastInteractObject;
    IInteractable currentInteractObject;
    IRotateAnObject currentRotatingObject;
    bool isRotatableObjRotating = true;
    float waitSecondProcess = 2f;
    private void Update()
    {
        if (waitSecondProcess > 0)
        {
            waitSecondProcess -= Time.deltaTime;
            //Debug.Log($"Second process will run in {waitSecondProcess} second.");
        }
        if (Input.touchCount > 0 && waitSecondProcess <= 0)
        {
            if(carryState != CarryState.PlayerRotate && carryState != CarryState.PlayerMove)
            {
                HandleTouchInput();
                CollectInventoryTouchInput();
            }
        }
        else
        {
            ResetTouchState();
        }
        RotateProcessControl();
    }
    public void RotateProcessControl()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && HandsFull() && isRotatableObjRotating)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LightPuzzleHandler.LayerMaskHelper.CarryLayer, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (currentInteractObject != null)
                {
                    if (currentInteractObject is ICollectHand handler)
                    {
                        if (hitObject.GetInstanceID() == handler.HandObject.GetInstanceID())
                        {
                            currentRotatingObject = hitObject.GetComponent<IRotateAnObject>();
                            Debug.Log("currentRotatingObject = hitObject.GetComponent<IRotateAnObject>();");
                        }
                    }
                }
            }
        }
        else if (Input.touchCount <= 0 && HandsFull())
        {
            currentRotatingObject = null;
            currentRotateTime = 0;
            SetStateForce(carryState = CarryState.PlayerCarry);
            Debug.Log("currentRotatingObject = null;");
            return;
        }
        currentRotateTime += Time.deltaTime;
        if (currentRotatingObject is ICollectHand hand)
        {
            ICollectable collectable = (ICollectable)hand;
            if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase != TouchPhase.Stationary) && collectable.barHandler.gameObject.activeSelf)
            collectable.barHandler.gameObject.SetActive(false);
        }
        if (isRotatableObjRotating && HandsFull() && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && currentRotateTime >= targetTouchTime / 5f)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (currentInteractObject is ICollectHand handler)
                {
                    SetStateForce(CarryState.PlayerRotate);
                    RotateObject(touch);
                    
                }
            }
        }
    }
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hitNew, 100, LightPuzzleHandler.LayerMaskHelper.CarryLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hitNew.collider.gameObject;
            if (currentInteractObject is ICollectHand handler)
            {
                if (handler.HandObject.GetInstanceID() != hitObject.GetInstanceID())
                {
                    Debug.Log($"Hit object: {hitObject.name} on layer {hitObject.layer}");
                    Debug.Log("handler.HandObject: " + handler.HandObject.name + " / hit: " + hitObject.name);
                    Debug.Log("Zaten bir obje tasiyorken, farkli bir objeye dokunuldu.");
                    return;
                }
            }
            if (Vector3.Distance(transform.position, hitObject.transform.position) > 3f) return;
            Debug.Log("Interactable object name => " + hitObject.name);

            currentTouchTime += Time.deltaTime;
            IInteractable interact = hitObject.GetComponent<IInteractable>();
            //if (interact is ICollectable collectable && collectable.IsCollected) return;
            if (interact != null)
            {
                if (HandsFull())
                {
                    SetStateForce(CarryState.PlayerCarryEnd);
                }
                else
                {
                    Vector3 direction = hitNew.collider.transform.position - transform.position;
                    direction.y = 0; 
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);
                    SetStateForce(CarryState.PlayerCarryBegin);
                }
                lastInteractObject = interact;
                HandleInteractableObject(interact, touch);
            }
        }
        else
        {
            SetStateForce(carryState == CarryState.PlayerMove ? CarryState.PlayerMove : CarryState.PlayerFree);
        }
    }

    private void HandleInteractableObject(IInteractable interact, Touch touch)
    {
        if (interact is IRotateAnObject rotateObj)
        {
            HandleRotatableObject(rotateObj);
        }
        MainUIManager.instance.LockPlayer();
        if (touch.phase == TouchPhase.Moved || touch.phase != TouchPhase.Stationary) return;
        if (!HandsFull() && currentTouchTime >= targetTouchTime)
        {
            PickUpObject(interact);
            waitSecondProcess = 1f;
            Handheld.Vibrate();
            Debug.Log("Telefon obje alindigi icin titredi.");
        }
        else if (HandsFull() && currentTouchTime >= targetTouchTime / 2)
        {
            DropObject(interact);
            waitSecondProcess = 1f;
            Handheld.Vibrate();
            Debug.Log("Telefon obje birakildigi icin titredi.");
        }        
        CollectControl(interact);
        
    }
    private void CollectInventoryTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hitNew, 100, LightPuzzleHandler.LayerMaskHelper.CarryLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hitNew.collider.gameObject;
            //if (currentInteractObject is ICollectInventory handler)
            //{
            //    if (handler..GetInstanceID() != hitObject.GetInstanceID())
            //    {
            //        Debug.Log($"Hit object: {hitObject.name} on layer {hitObject.layer}");
            //        Debug.Log("handler.HandObject: " + handler.HandObject.name + " / hit: " + hitObject.name);
            //        Debug.Log("Zaten bir obje tasiyorken, farkli bir objeye dokunuldu.");
            //        return;
            //    }
            //}
            if (Vector3.Distance(transform.position, hitObject.transform.position) > 3f) return;
            Debug.Log("Interactable object name => " + hitObject.name);

            currentTouchTime += Time.deltaTime;
            IInteractable interact = hitObject.GetComponent<IInteractable>();
            if (interact is ICollectable collectable && collectable.IsCollected) return;
            if (interact != null)
            {
                ICollectInventory inventoryInteract = interact as ICollectInventory;
                if (inventoryInteract != null && HaveItemInInventory(inventoryInteract))
                {
                    SetStateForce(CarryState.PlayerCarryEnd);
                }
                else
                {
                    Vector3 direction = hitNew.collider.transform.position - transform.position;
                    direction.y = 0;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);
                    SetStateForce(CarryState.PlayerCarryBegin);
                }
                lastInteractObject = interact;
                InventoryInteractableObject(inventoryInteract, touch);
            }
        }
        else
        {
            SetStateForce(carryState == CarryState.PlayerMove ? CarryState.PlayerMove : CarryState.PlayerFree);
        }
    }
    private void InventoryInteractableObject(ICollectInventory inventoryInteract, Touch touch)
    {
        MainUIManager.instance.LockPlayer();
        if (touch.phase == TouchPhase.Moved) return;
        if (!HaveItemInInventory(inventoryInteract) && currentTouchTime >= targetTouchTime)
        {
            PickUpObject(inventoryInteract as IInteractable);
            waitSecondProcess = 1f;
            Handheld.Vibrate();
            Debug.Log("Telefon obje alindigi icin titredi.");
        }
        //else if (HaveItemInInventory((ICollectInventory)interact) && currentTouchTime >= targetTouchTime / 2)
        //{
        //    DropObject(interact);
        //    waitSecondProcess = 1f;
        //    Handheld.Vibrate();
        //    Debug.Log("Telefon obje birakildigi icin titredi.");
        //}
        //CollectControl(interact);

    }
    public void CollectControl(IInteractable interact)
    {
        if (interact is ICollectable collect)
        {
            if (!collect.barHandler.gameObject.activeSelf)
                collect.barHandler.gameObject.SetActive(true);

            float percentage = 0;
            int barValue = Mathf.RoundToInt(percentage * 100);

            Color bgColor = Color.white;
            Color fillerColor = Color.green;
            float speed;

            if (!HandsFull() || (interact is ICollectInventory s && !HaveItemInInventory(s)))
            {
                speed = 5;
                percentage = Mathf.Clamp01(currentTouchTime / targetTouchTime);
                if (currentTouchTime >= targetTouchTime / 2)
                    fillerColor = Color.green;
                else if ( currentTouchTime >= targetTouchTime / 4)
                    fillerColor = Color.blue;
                else
                    fillerColor = Color.cyan;
            }
            else 
            {
                speed = 20;
                percentage = Mathf.Clamp01(currentTouchTime / (targetTouchTime / 2));
                if (currentTouchTime >= (targetTouchTime / 3))
                    fillerColor = Color.red;
                else if (currentTouchTime >= (targetTouchTime / 4))
                    fillerColor = Color.yellow;
                else
                    fillerColor = Color.grey;
            }

            // Set the bar smoothly
            collect.barHandler.SetBar(bgColor, fillerColor, percentage, speed);
        }
    }
    float currentRotateTime = 0f;
    private void HandleRotatableObject(IRotateAnObject rotateObj)
    {
        if (HandsFull() && !isRotatableObjRotating)
        {
            currentRotatingObject = rotateObj;
            isRotatableObjRotating = true;
            lastTouchPosition = Input.GetTouch(0).position;
        }
    }
    bool HaveItemInInventory(ICollectInventory _item)
    {
        if (_item == null) return true;
        return inventory.IsItemInInventory(_item);
    }
    private void PickUpObject(IInteractable interact)
    {
        SetStateForce(CarryState.PlayerCarry);
        interact.Interact(InteractType.Pickable);
        currentInteractObject = interact;
        if (currentInteractObject is ICollectHand handler)
        {
            isRotatableObjRotating = false;
            Renderer[] childRenderers = handler.HandObject.GetComponentsInChildren<Renderer>();
            foreach (var item in childRenderers)
                item.gameObject.layer = LayerMask.NameToLayer("PlayerCarry");
        }
        ResetTouchState();
        MainUIManager.instance.UnLockPlayer();
    }

    private void DropObject(IInteractable interact)
    {
        SetStateForce(carryState == CarryState.PlayerMove ? CarryState.PlayerMove : CarryState.PlayerFree);
        isRotatableObjRotating = false;
        interact.Interact(InteractType.Dropable);
        if (currentInteractObject is ICollectHand handler)
        {
            Renderer[] childRenderers = handler.HandObject.GetComponentsInChildren<Renderer>();
            foreach (var item in childRenderers)
                item.gameObject.layer = LayerMask.NameToLayer("PlayerIgnore");
        }
        currentInteractObject = null;
        ResetTouchState();
        MainUIManager.instance.UnLockPlayer();
    }

    private void RotateObject(Touch touch)
    {
        if (currentRotatingObject != null)
        {
            //MainUIManager.instance.LockPlayer();
            Debug.Log(currentRotatingObject + " adli obje'nin " + currentRotatingObject.RotatableObject + " adli rotate objesi donduruluyor...");
            Vector2 deltaPosition = touch.position - lastTouchPosition;

            // Convert touch delta to rotation angles
            float rotationX = deltaPosition.y;
            float rotationY = -deltaPosition.x;

            currentRotatingObject.RotatableObject.RotateWithRotateAngel(new Vector3(rotationX, rotationY, 0));
            lastTouchPosition = touch.position;
        }
    }

    private void ResetTouchState()
    {
        SetStateForce(carryState == CarryState.PlayerMove ? CarryState.PlayerMove : CarryState.PlayerFree);
        if (lastInteractObject is ICollectable collectHand)
        {
            if (collectHand.barHandler.gameObject.activeSelf)
            {
                collectHand.barHandler.gameObject.SetActive(false);
                collectHand.barHandler.ResetBar();
            }            
        }
        //if (isRotatableObjRotating)
        //{
        //    isRotatableObjRotating = false;
        //}

        if (targetTouchTime != 1.5f || currentTouchTime != 0f)
        {
            targetTouchTime = 1.5f;
            currentTouchTime = 0f;
        }
            MainUIManager.instance.UnLockPlayer();
        //waitSecondProcess = 0.5f;
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

    public void SetState(bool _free)
    {
        if (_free && carryState != CarryState.PlayerMove)
        {
            Debug.Log("State degistirme basarisiz, Player nesne tasiyorken freeye gecilemez.");
            return;
        }
        if (!_free && carryState != CarryState.PlayerFree)
        {
            Debug.Log("State degistirme basarisiz, Player nesne tasiyorken freeye gecilemez.");
            return;
        }

        if (_free)
        {
            CancelInvoke(nameof(InvokeSetPlayerFree));
            CancelInvoke(nameof(InvokeSetPlayerMove));
            Invoke(nameof(InvokeSetPlayerFree), 0.2f);
        }
        else
        {
            CancelInvoke(nameof(InvokeSetPlayerFree));
            CancelInvoke(nameof(InvokeSetPlayerMove));
            Invoke(nameof(InvokeSetPlayerMove), 0.2f);
        }
    }

    public void SetStateForce(CarryState _newState)
    {
        carryState = _newState;
    }

    void InvokeSetPlayerFree()
    {
        if (carryState == CarryState.PlayerMove)
            carryState = CarryState.PlayerFree;
    }

    void InvokeSetPlayerMove()
    {
        if (carryState == CarryState.PlayerFree)
            carryState = CarryState.PlayerMove;
    }

    public enum CarryState
    {
        PlayerFree,
        PlayerMove,
        PlayerCarryBegin,
        PlayerCarry,
        PlayerCarryEnd,
        PlayerRotate,
    }
}