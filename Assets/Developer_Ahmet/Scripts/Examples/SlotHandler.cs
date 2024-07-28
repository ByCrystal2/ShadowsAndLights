using MalbersAnimations;
using MalbersAnimations.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class SlotHandler : MonoBehaviour, ICollectable
{
    public int LevelID;
    [SerializeField] List<Transform> MySlots = new List<Transform>();
    [SerializeField] int SlotCapasity = 1;
    [SerializeField] List<Transform> targets = new List<Transform>();
    InventoryHandler inventoryHandler;
    List<ICollectInventory> _contentItems = new List<ICollectInventory>();
    [SerializeField] SlotInventoryUI inventoryUI;
    [SerializeField] InteractableBarHandler bar;
    private bool control = false;

    private void OnValidate()
    {
        int length1 = targets.Count;
        for (int i = 0; i < length1; i++)
        {
            if (targets[i] == null) continue;
            if (targets[i].TryGetComponent(out IEnterAnySlotable x)) ;
            else if (targets[i] != null) targets[i] = null;
        }

        if (MySlots.Count == 0) return;
        List<Transform> slots = new List<Transform>();
        List<Transform> _targets = new List<Transform>();
        int length = SlotCapasity;
        for (int i = 0; i < length; i++)
        {
            if (i < MySlots.Count)
                slots.Add(MySlots[i]);
            if (i < targets.Count)
                _targets.Add(targets[i]);
        }
        if (MySlots.Count > SlotCapasity)
        {
            MySlots.Clear();
            int length2 = slots.Count;
            for (int i = 0; i < length2; i++)
            {
                MySlots.Add(slots[i]);
            }
        }
        if (targets.Count > SlotCapasity)
        {
            targets.Clear();
            int length3 = _targets.Count;
            for (int i = 0; i < length3; i++)
            {
                targets.Add(_targets[i]);
            }
        }
    }

    public void SetLevelID(int _levelID)
    {
        LevelID = _levelID;
    }

    private void Awake()
    {
        inventoryHandler = GameObject.FindWithTag("Animal").GetComponent<InventoryHandler>();
    }
    float targetTouchTime = 1.5f, currentTouchTime = 0;
    private void Update()
    {
        HandleInteraction();
    }
    public CollectHandType CollectType { get; set; }
    public bool IsCollected {  get; set; }
    public InteractableBarHandler barHandler { get => bar; set { } }
    private enum TouchState { None, Began, Stationary, Ended }
    private TouchState touchState = TouchState.None;
    private void HandleInteraction()
    {
        float distance = Vector3.Distance(transform.position, inventoryHandler.transform.position);
        if (/*_contentItems.Count <= 0 && */distance > 3) return;

        UpdateTouchState();

        if (touchState != TouchState.Stationary)
        {
            currentTouchTime = 0;
            barHandler.gameObject.SetActive(false);
            barHandler.ResetBar();
        }

        if (Input.touchCount > 0 && touchState == TouchState.Stationary)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.GetInstanceID() == gameObject.GetInstanceID())
                {
                    HandleInventoryInteraction(hitObject);
                }
            }
        }
    }
    private void UpdateTouchState()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchState = TouchState.Began;
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
                if (touchState == TouchState.Began)
                {
                    touchState = TouchState.Stationary;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                touchState = TouchState.Ended;
            }
        }
        else
        {
            touchState = TouchState.None;
        }
    }
    private void HandleInventoryInteraction(GameObject hitObject)
    {        

        if (MySlots.Count == 1)
        {
            HandleSingleSlotInteraction();
        }
        else
        {
            if (MySlots.Count > 0)
            {
                inventoryUI.SetInventory(_contentItems);
                inventoryUI.gameObject.SetActive(true);
            }
        }
    }

    private void HandleSingleSlotInteraction()
    {
        if (targets[0].TryGetComponent(out IInteractable interact))
        {
            Debug.Log($"{name} adlý slot objesi, {targets[0].name} adlý envanter objesiyle etkileþime giriyor...");
            ICollectable collectable = interact as ICollectable;
            inventoryUI.gameObject.SetActive(false);
                currentTouchTime += Time.deltaTime;
            if (inventoryHandler.IsItemInInventory((ICollectInventory)collectable))
            {
                Debug.Log("Item is in Inventory");
                
                if(!control)
                {
                    if (SlotCapasity <= 0) return;
                    if (currentTouchTime <= targetTouchTime)
                    {
                        CollectControl(this);
                        return;
                    }
                    control = !control;
                    if (interact is IEnterAnySlotable slotable)
                    {
                        slotable.EnterSlot(this);                       
                    }
                    targets[0].gameObject.SetActive(true);
                    EndInteraction();
                }
            }
            else
            {
                if (control)
                {
                    if (currentTouchTime <= targetTouchTime / 2)
                    {
                        CollectControl(this);
                        return;
                    }
                    control = !control;
                    interact.Interact(InteractType.Pickable);
                    SlotCapasity++;
                    EndInteraction();
                    LightPuzzleHandler.instance.GetLevelBehaviour(LevelID).OnBatteryRemoved();
                }
            }
        }
    }

    private void EndInteraction()
    {
        barHandler.gameObject.SetActive(false);
        touchState = TouchState.Ended;
        currentTouchTime = 0f;
    }

    public void CollectControl(ICollectable collectable)
    {
        if (!collectable.barHandler.gameObject.activeSelf)
            collectable.barHandler.gameObject.SetActive(true);

        float percentage = 0;
        float speed;

        if (!control)
        {
            speed = 5;
            percentage = Mathf.Clamp01(currentTouchTime / targetTouchTime);
        }
        else
        {
            speed = 20;
            percentage = Mathf.Clamp01(currentTouchTime / (targetTouchTime / 2));
        }

        Color fillerColor = GetFillerColor();
        collectable.barHandler.SetBar(Color.white, fillerColor, percentage, speed);
    }

    private Color GetFillerColor()
    {
        if (!control)
        {
            if (currentTouchTime >= targetTouchTime / 2)
                return Color.green;
            else if (currentTouchTime >= targetTouchTime / 4)
                return Color.blue;
            else
                return Color.cyan;
        }
        else
        {
            if (currentTouchTime >= targetTouchTime / 3)
                return Color.red;
            else if (currentTouchTime >= targetTouchTime / 4)
                return Color.yellow;
            else
                return Color.grey;
        }
    }
    public void AddMyContent(GameObject _object)
    {
        if (SlotCapasity > 0)
        {
            int index = SlotCapasity - 1;
            _object.transform.SetPositionAndRotation(MySlots[index].transform.position, MySlots[index].rotation);
            SlotCapasity--;
            if (_object.TryGetComponent(out ICollectInventory _item))
            {
                inventoryHandler.RemoveInventory(_item);
            }
            else
                Debug.Log($"{_object} adli obje envanter itemi degildir.");
            Debug.Log($"{_object.name} adli obje slota yerlestirildi.");
            if (SlotCapasity <= 0)
            {
                SlotCapasity = 0;
                if(_object.TryGetComponent(out BatteryBehaviour BB))
                    LightPuzzleHandler.instance.GetLevelBehaviour(LevelID).OnBatteryPlaced(BB);
                Debug.Log("Tum slotlar doldu.");
                return;
            }
        }
        else
        {
            Debug.Log("Slot kapasitesi 0 veya 0 dan kucuk => " + SlotCapasity);
        }
    }

    public void Collect()
    {
    }
}
