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
            objTrans.localPosition = new Vector3();
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
    float targetTouchTime = 1.5f, currentTouchTime = 0;
    private Vector2 lastTouchPosition;
    IInteractable lastInteractObject;
    IInteractable currentHandObject;
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
            HandleTouchInput();
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
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.CompareTag("Interactable"))
                {
                    if (currentHandObject != null)
                    {
                        if (currentHandObject is ICollectHand handler)
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
        }
        else if (Input.touchCount <= 0 && HandsFull())
        {
            currentRotatingObject = null;
            Debug.Log("currentRotatingObject = null;");
        }
        if (isRotatableObjRotating && HandsFull() && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (currentHandObject is ICollectHand handler)
                {
                    //Debug.Log("for Rotate hitObject name => " + hitObject.name);
                    if (hitObject.GetInstanceID() != handler.HandObject.GetInstanceID())
                    {
                        RotateObject(touch);
                        Debug.Log("RotateObject(touch);");
                    }
                }
            }
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
                    lastInteractObject = interact;
                    HandleInteractableObject(interact, touch);
                }
            }
            else
            {
                ResetTouchState();
                
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
        if (touch.phase == TouchPhase.Moved) return;
        if (currentTouchTime >= targetTouchTime)
        {
            if (!HandsFull())
            {
                PickUpObject(interact);
                waitSecondProcess = 1f;
            }
        }
        else if (currentTouchTime >= targetTouchTime / 2)
        {
            if (HandsFull())
            {
                DropObject(interact);
                waitSecondProcess = 1f;
            }
        }        
            CollectHandControl(interact);
        
    }
    public void CollectHandControl(IInteractable interact)
    {
        if (interact is ICollectHand collectHand)
        {
            if (!collectHand.barHandler.gameObject.activeSelf)
                collectHand.barHandler.gameObject.SetActive(true);

            float percentage = 0;
            int barValue = Mathf.RoundToInt(percentage * 100);

            Color bgColor = Color.white;
            Color fillerColor = Color.green;
            float speed;
            if (!HandsFull())
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
                if (currentTouchTime >= (targetTouchTime / 2))
                    fillerColor = Color.red;
                else if (currentTouchTime >= (targetTouchTime / 4))
                    fillerColor = Color.yellow;
                else
                    fillerColor = Color.grey;
            }

            // Set the bar smoothly
            collectHand.barHandler.SetBar(bgColor, fillerColor, percentage, speed);
        }
    }
    private void HandleRotatableObject(IRotateAnObject rotateObj)
    {
        if (HandsFull() && !isRotatableObjRotating)
        {
                MainUIManager.instance.LockPlayer();
            //if (currentTouchTime >= targetTouchTime / 2.5f)
            //{
                currentRotatingObject = rotateObj;
                isRotatableObjRotating = true;
                lastTouchPosition = Input.GetTouch(0).position;
            //}
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
            //float rotationAmount = deltaPosition.x * 0.5f; // Dönme hýzýný ayarlamak için çarpan kullanabilirsiniz.
            currentRotatingObject.RotatableObject.RotateWithRotateAngel(deltaPosition);
            lastTouchPosition = touch.position;
        }
    }

    private void ResetTouchState()
    {        
        if (lastInteractObject is ICollectHand collectHand)
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
}