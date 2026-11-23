using UnityEngine;
using UnityEngine.UI;

// References exported from the original phone/tablet scenes, without their live managers.
public sealed class SagaBoxesLayout : MonoBehaviour
{
    public GameObject[] Boards;
    public Transform Tray;
    public GameObject TrayItem,RecallButton,Bottom,BottomTarget,Header;
    public Text Clock;
    public Image[] Hearts;
}
