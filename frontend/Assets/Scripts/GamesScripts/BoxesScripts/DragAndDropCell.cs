using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Every item's cell must contain this script
/// </summary>
[RequireComponent(typeof(Image))]
public class DragAndDropCell : MonoBehaviour, IDropHandler
{
    [HideInInspector]
    public BoxesManager boxesManager;
    [HideInInspector]
    public bool highlight = true;

    Vector3 localPos;


    // private void OnMouseOver() {
    //     this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("3DImages\\Artboard  (12)") as Sprite;
    // }

    private void OnMouseEnter() 
    {
        if(boxesManager != null)
        {
            if (boxesManager.inGame && highlight)
                this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Boxes\\boxes-ring-blue") as Sprite;
        }
    }

    private void OnMouseExit() {
        this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Boxes\\boxes-ring") as Sprite;
    }

    private void OnMouseUp() {
        this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Boxes\\boxes-ring") as Sprite;
    }

    private void Start()
    {
        boxesManager = GameObject.Find("[BoxesManager]").GetComponent<BoxesManager>();

        int a = LevelScript._selectedLevel;

        if(a >= 11 & a < 14)
        {
            localPos = new Vector3(0, 20, 0);
        }
        else if(a >= 14)
        {
            localPos = new Vector3(0, 15, 0);
        }
        else
        {
            localPos = new Vector3(0, 25, 0);
        }
    }

    private void Update() {
        
    }

    public enum CellType                                                    // Cell types
    {
        Swap,                                                               // Items will be swapped between any cells
        DropOnly,                                                           // Item will be dropped into cell
        DragOnly                                                            // Item will be dragged from this cell
    }

    public enum TriggerType                                                 // Types of drag and drop events
    {
        DropRequest,                                                        // Request for item drop from one cell to another
        DropEventEnd,                                                       // Drop event completed
        ItemAdded,                                                          // Item manualy added into cell
        ItemWillBeDestroyed                                                 // Called just before item will be destroyed
    }

    public class DropEventDescriptor                                        // Info about item's drop event
    {
        public TriggerType triggerType;                                     // Type of drag and drop trigger
        public DragAndDropCell sourceCell;                                  // From this cell item was dragged
        public DragAndDropCell destinationCell;                             // Into this cell item was dropped
        public DragAndDropItem item;                                        // Dropped item
        public bool permission;                                             // Decision need to be made on request
    }

    [Tooltip("Functional type of this cell")]
    public CellType cellType = CellType.Swap;                               // Special type of this cell
    [Tooltip("Sprite color for empty cell")]
    public Color empty = new Color();                                       // Sprite color for empty cell
    [Tooltip("Sprite color for filled cell")]
    public Color full = new Color();                                        // Sprite color for filled cell
    [Tooltip("This cell has unlimited amount of items")]
    public bool unlimitedSource = false;                                    // Item from this cell will be cloned on drag start

    private DragAndDropItem myDadItem;										// Item of this DaD cell

    void OnEnable()
    {
        DragAndDropItem.OnItemDragStartEvent += OnAnyItemDragStart;         // Handle any item drag start
        DragAndDropItem.OnItemDragEndEvent += OnAnyItemDragEnd;             // Handle any item drag end
        UpdateMyItem();
        //UpdateBackgroundState();
    }

    void OnDisable()
    {
        DragAndDropItem.OnItemDragStartEvent -= OnAnyItemDragStart;
        DragAndDropItem.OnItemDragEndEvent -= OnAnyItemDragEnd;
        StopAllCoroutines();                                                // Stop all coroutines if there is any
    }

    /// <summary>
    /// On any item drag start need to disable all items raycast for correct drop operation
    /// </summary>
    /// <param name="item"> dragged item </param>
    private void OnAnyItemDragStart(DragAndDropItem item)
    {
        UpdateMyItem();
        if (myDadItem != null)
        {
            myDadItem.MakeRaycast(false);                                   // Disable item's raycast for correct drop handling
            if (myDadItem == item)                                         	// If item dragged from this cell
            {
                // Check cell's type
                switch (cellType)
                {
                    case CellType.DropOnly:
                        DragAndDropItem.icon.SetActive(false);              // Item can not be dragged. Hide icon
                        break;
                }
            }
        }
    }

    /// <summary>
    /// On any item drag end enable all items raycast
    /// </summary>
    /// <param name="item"> dragged item </param>
    private void OnAnyItemDragEnd(DragAndDropItem item)
    {
        UpdateMyItem();
        if (myDadItem != null)
        {
            myDadItem.MakeRaycast(true);                                  	// Enable item's raycast
        }
        //UpdateBackgroundState();
    }
    /// <summary>
    /// Item is dropped in this cell
    /// </summary>
    /// <param name="data"></param>
    public void OnDrop(PointerEventData data)
    {
        this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Boxes\\boxes-ring") as Sprite;

        if (DragAndDropItem.icon != null)
        {
            DragAndDropItem item = DragAndDropItem.draggedItem;
            DragAndDropCell sourceCell = DragAndDropItem.sourceCell;

            if (!this.name.Contains(item.name) && !this.name.Contains("Prefabe"))
            {
                Handheld.Vibrate();

                boxesManager.countForLose++;
                boxesManager.hearts[boxesManager.maxMistakes - boxesManager.countForLose].gameObject.SetActive(false);

                if(boxesManager.countForLose >= boxesManager.maxMistakes) {
                    boxesManager.Finished();
                }
            }

            if (this.name.Contains(item.name) && !this.name.Contains("Prefabe"))
            {
                boxesManager.countForWin++;

                if(boxesManager.countForWin >= boxesManager.spawnNumber) {
                    boxesManager.YouWon();
                }
            }

            if (DragAndDropItem.icon.activeSelf == true)                    // If icon inactive do not need to drop item into cell
            {
                if ((item != null) && (sourceCell != this))
                {
                    DropEventDescriptor desc = new DropEventDescriptor();
                    switch (cellType)                                       // Check this cell's type
                    {
                        case CellType.Swap:                                 // Item in destination cell can be swapped
                            UpdateMyItem();
                            switch (sourceCell.cellType)
                            {
                                case CellType.Swap:                         // Item in source cell can be swapped
                                    // Fill event descriptor
                                    desc.item = item;
                                    desc.sourceCell = sourceCell;
                                    desc.destinationCell = this;
                                    SendRequest(desc);                      // Send drop request
                                    StartCoroutine(NotifyOnDragEnd(desc));  // Send notification after drop will be finished
                                    if (desc.permission == true)            // If drop permitted by application
                                    {
                                        if (myDadItem != null)            // If destination cell has item
                                        {

                                            // Fill event descriptor
                                            DropEventDescriptor descAutoswap = new DropEventDescriptor();
                                            descAutoswap.item = myDadItem;
                                            descAutoswap.sourceCell = this;
                                            descAutoswap.destinationCell = sourceCell;
                                            SendRequest(descAutoswap);                      // Send drop request
                                            StartCoroutine(NotifyOnDragEnd(descAutoswap));  // Send notification after drop will be finished
                                            if (descAutoswap.permission == true)            // If drop permitted by application
                                            {
                                                SwapItems(sourceCell, this);                // Swap items between cells
                                            }
                                            else
                                            {
                                                PlaceItem(item);            // Delete old item and place dropped item into this cell

                                            }
                                        }
                                        else
                                        {
                                            PlaceItem(item);                // Place dropped item into this empty cell
                                        }
                                    }
                                    break;
                                default:                                    // Item in source cell can not be swapped
                                    // Fill event descriptor
                                    desc.item = item;
                                    desc.sourceCell = sourceCell;
                                    desc.destinationCell = this;
                                    SendRequest(desc);                      // Send drop request
                                    StartCoroutine(NotifyOnDragEnd(desc));  // Send notification after drop will be finished
                                    if (desc.permission == true)            // If drop permitted by application
                                    {
                                        PlaceItem(item);                    // Place dropped item into this cell
                                    }
                                    break;
                            }
                            break;
                        case CellType.DropOnly:                             // Item only can be dropped into destination cell
                            // Fill event descriptor
                            desc.item = item;
                            desc.sourceCell = sourceCell;
                            desc.destinationCell = this;
                            SendRequest(desc);                              // Send drop request
                            StartCoroutine(NotifyOnDragEnd(desc));          // Send notification after drop will be finished
                            if (desc.permission == true)                    // If drop permitted by application
                            {
                                PlaceItem(item);                            // Place dropped item in this cell
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (item != null)
            {
                if (item.GetComponentInParent<DragAndDropCell>() == null)   // If item have no cell after drop
                {
                    Destroy(item.gameObject);                               // Destroy it
                }
            }
            UpdateMyItem();
            //UpdateBackgroundState();
            sourceCell.UpdateMyItem();
            //sourceCell.UpdateBackgroundState();
        }
    }

    /// <summary>
    /// Put item into this cell.
    /// </summary>
    /// <param name="item">Item.</param>
    private void PlaceItem(DragAndDropItem item)
    {
        if (item != null)
        {
            DestroyItem();                                              // Remove current item from this cell
            myDadItem = null;
            DragAndDropCell cell = item.GetComponentInParent<DragAndDropCell>();
            if (cell != null)
            {
                if (cell.unlimitedSource == true)
                {
                    string itemName = item.name;
                    item = Instantiate(item);                               // Clone item from source cell
                    item.name = itemName;
                }
            }
            item.transform.SetParent(transform, false);
            item.transform.localPosition = Vector3.zero;
            item.MakeRaycast(true);
            myDadItem = item;
        }
        //UpdateBackgroundState();
    }

    /// <summary>
    /// Destroy item in this cell
    /// </summary>
    private void DestroyItem()
    {
        UpdateMyItem();
        if (myDadItem != null)
        {
            DropEventDescriptor desc = new DropEventDescriptor();
            // Fill event descriptor
            desc.triggerType = TriggerType.ItemWillBeDestroyed;
            desc.item = myDadItem;
            desc.sourceCell = this;
            desc.destinationCell = this;
            SendNotification(desc);                                         // Notify application about item destruction
            if (myDadItem != null)
            {
                Destroy(myDadItem.gameObject);
            }
        }
        myDadItem = null;
        //UpdateBackgroundState();
    }

    /// <summary>
    /// Send drag and drop information to application
    /// </summary>
    /// <param name="desc"> drag and drop event descriptor </param>
    private void SendNotification(DropEventDescriptor desc)
    {
        if (desc != null)
        {
            // Send message with DragAndDrop info to parents GameObjects
            gameObject.SendMessageUpwards("OnSimpleDragAndDropEvent", desc, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Send drag and drop request to application
    /// </summary>
    /// <param name="desc"> drag and drop event descriptor </param>
    /// <returns> result from desc.permission </returns>
    private bool SendRequest(DropEventDescriptor desc)
    {
        bool result = false;
        if (desc != null)
        {
            desc.triggerType = TriggerType.DropRequest;
            desc.permission = true;
            SendNotification(desc);
            result = desc.permission;
        }
        return result;
    }

    /// <summary>
    /// Wait for event end and send notification to application
    /// </summary>
    /// <param name="desc"> drag and drop event descriptor </param>
    /// <returns></returns>
    private IEnumerator NotifyOnDragEnd(DropEventDescriptor desc)
    {
        // Wait end of drag operation
        while (DragAndDropItem.draggedItem != null)
        {
            yield return new WaitForEndOfFrame();
        }
        desc.triggerType = TriggerType.DropEventEnd;
        SendNotification(desc);
    }

    /// <summary>
    /// Change cell's sprite color on item put/remove.
    /// </summary>
    /// <param name="condition"> true - filled, false - empty </param>
    //public void UpdateBackgroundState()
    //{
    //    Image bg = GetComponent<Image>();
    //    if (bg != null)
    //    {
    //        bg.color = myDadItem != null ? full : empty;
    //    }
    //}

    /// <summary>
    /// Updates my item
    /// </summary>
    public void UpdateMyItem()
    {
        myDadItem = GetComponentInChildren<DragAndDropItem>();
    }

    /// <summary>
    /// Get item from this cell
    /// </summary>
    /// <returns> Item </returns>
    public DragAndDropItem GetItem()
    {
        return myDadItem;
    }

    /// <summary>
    /// Manualy add item into this cell
    /// </summary>
    /// <param name="newItem"> New item </param>
    public void AddItem(DragAndDropItem newItem)
    {
        if (newItem != null)
        {
            PlaceItem(newItem);
            DropEventDescriptor desc = new DropEventDescriptor();
            // Fill event descriptor
            desc.triggerType = TriggerType.ItemAdded;
            desc.item = newItem;
            desc.sourceCell = this;
            desc.destinationCell = this;
            SendNotification(desc);
        }
    }

    /// <summary>
    /// Manualy delete item from this cell
    /// </summary>
    public void RemoveItem()
    {
        DestroyItem();
    }

    /// <summary>
    /// Swap items between two cells
    /// </summary>
    /// <param name="firstCell"> Cell </param>
    /// <param name="secondCell"> Cell </param>
    public void SwapItems(DragAndDropCell firstCell, DragAndDropCell secondCell)
    {
        if ((firstCell != null) && (secondCell != null))
        {
            DragAndDropItem firstItem = firstCell.GetItem();                // Get item from first cell
            DragAndDropItem secondItem = secondCell.GetItem();              // Get item from second cell

            if (firstCell.name.Contains("Prefabe"))
            {
                // Swap items
                if (firstItem != null)
                {
                    firstItem.transform.SetParent(secondCell.transform, false);
                    firstItem.transform.localPosition = localPos; // ne nivele mbi 15 (check it) duhet 15 me kon ...
                    firstItem.transform.localScale = new Vector3(1, 1, 1);
                    firstItem.MakeRaycast(true);
                }
                if (secondItem != null)
                {
                    secondItem.transform.SetParent(firstCell.transform, false);
                    secondItem.transform.localPosition = Vector3.zero;
                    secondItem.transform.localScale = new Vector3(1, 1, 1);
                    secondItem.MakeRaycast(true);
                }
                
                if(secondItem.GetName() == "ImgPref") {
                    firstCell.DisableCell();
                    firstItem.ChangeImageTypeToSimple();
                }
                firstItem.ChangeImageTypeToSimple();

            }
            else if (secondCell.name.Contains("Prefabe"))
            {
                // Swap items
                if (firstItem != null)
                {
                    firstItem.transform.SetParent(firstCell.transform, false);
                    firstItem.transform.localPosition = localPos;
                    firstItem.transform.localScale = new Vector3(1, 1, 1);
                    firstItem.MakeRaycast(true);

                }
                if (secondItem != null)
                {
                    secondItem.transform.SetParent(secondCell.transform, false);
                    secondItem.transform.localPosition = Vector3.zero;
                    secondItem.transform.localScale = new Vector3(1, 1, 1);
                    secondItem.MakeRaycast(true);
                }
            }
            else
            {
                // Swap items
                if (firstItem != null)
                {
                    firstItem.transform.SetParent(secondCell.transform, false);
                    firstItem.transform.localPosition = localPos;
                    firstItem.transform.localScale = new Vector3(1, 1, 1);
                    firstItem.MakeRaycast(true);
                }
                if (secondItem != null)
                {
                    secondItem.transform.SetParent(firstCell.transform, false);
                    secondItem.transform.localPosition = localPos;
                    secondItem.transform.localScale = new Vector3(1, 1, 1);
                    secondItem.MakeRaycast(true);
                }

                firstItem.ChangeImageTypeToSimple();
            }

            if (firstCell.name.Contains("Prefabe") && secondCell.name.Contains("Prefabe"))
            {
                // Swap items
                if (firstItem != null)
                {
                    firstItem.transform.SetParent(secondCell.transform, false);
                    firstItem.transform.localPosition = Vector3.zero;
                    firstItem.transform.localScale = new Vector3(1, 1, 1);
                    firstItem.MakeRaycast(true);
                }
                if (secondItem != null)
                {
                    secondItem.transform.SetParent(firstCell.transform, false);
                    secondItem.transform.localPosition = Vector3.zero;
                    secondItem.transform.localScale = new Vector3(1, 1, 1);
                    secondItem.MakeRaycast(true);
                }
            }

            // Disable drag and drop when matches
            if(secondCell.name.Contains(firstItem.GetName())) {
                secondCell.GetComponent<DragAndDropCell>().enabled = false;
                firstItem.GetComponent<DragAndDropItem>().enabled = false;
                highlight = false;
            }

            if(firstCell.name.Contains(secondItem.GetName())) {
                firstCell.GetComponent<DragAndDropCell>().enabled = false;
                secondItem.GetComponent<DragAndDropItem>().enabled = false;
                firstCell.highlight = false;

                if (firstCell.name.Contains(secondItem.name) && !firstCell.name.Contains("Prefabe"))
                {
                    boxesManager.countForWin++;

                    if(boxesManager.countForWin >= boxesManager.spawnNumber) {
                        boxesManager.YouWon();
                    }
                }
            }


            //Kur e kthen figuren posht
            //if (!firstCell.name.Contains("Prefabe") && secondCell.name.Contains("Prefabe"))
            //{

            //    Debug.Log("5");

            //    // Swap items
            //    if (firstItem != null)
            //    {
            //        firstItem.transform.SetParent(secondCell.transform, false);
            //        firstItem.transform.localPosition = new Vector3(0, 35, 0); // UNE E KOM NDRU o kon vector3.zero
            //        firstItem.transform.rotation = Quaternion.Euler(0, 0, -45);
            //        // Ja ndron madhesin e Idem me scale te Cell
            //        firstItem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            //        firstItem.MakeRaycast(true);
            //    }
            //    if (secondItem != null)
            //    {
            //        secondItem.transform.SetParent(firstCell.transform, false);
            //        secondItem.transform.localPosition = Vector3.zero; // UNE E KOM NDRU o kon Vector3.zero; new Vector3(7, 94, 0);
            //        secondItem.transform.rotation = Quaternion.Euler(0, 0, 0);
            //        secondItem.transform.localScale = new Vector3(1, 1, 1);
            //        secondItem.MakeRaycast(true);
            //    }
            //}

            // Update states
            firstCell.UpdateMyItem();
            secondCell.UpdateMyItem();
            //firstCell.UpdateBackgroundState();
            //secondCell.UpdateBackgroundState();
        }

    }

    public void DisableCell(){
        this.gameObject.SetActive(false);
    }
}
