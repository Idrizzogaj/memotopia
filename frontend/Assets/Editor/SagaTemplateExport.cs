using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SagaTemplateExport
{
    public static void Export(){
        foreach(string variant in new[]{"BoxesScene","BoxesSceneIpad"}){
            EditorSceneManager.OpenScene("Assets/_Scenes/"+variant+".unity");
            var manager=Object.FindObjectOfType<BoxesManager>();var canvas=manager.countDownText.GetComponentInParent<Canvas>().rootCanvas;
            var source=canvas.GetComponentsInChildren<Transform>(true);
            var root=Object.Instantiate(canvas.gameObject);var copies=root.GetComponentsInChildren<Transform>(true);
            System.Func<GameObject,GameObject> copy=go=>go==null?null:copies[System.Array.IndexOf(source,go.transform)].gameObject;
            var layout=root.AddComponent<SagaBoxesLayout>();
            foreach(var button in canvas.GetComponentsInChildren<Button>(true))for(int n=0;n<button.onClick.GetPersistentEventCount();n++)if(button.onClick.GetPersistentMethodName(n).Contains("Quit"))copy(button.gameObject).name="Saga Quit";
            layout.Boards=manager.levelPanels.Select(copy).ToArray();layout.Tray=copy(manager.scrollViewContent.gameObject).transform;
            layout.TrayItem=System.Array.IndexOf(source,manager.imagePrefab.transform)>=0?copy(manager.imagePrefab):Object.Instantiate(manager.imagePrefab,root.transform,false);
            layout.TrayItem.SetActive(false);layout.RecallButton=copy(manager.recallBtn);layout.Bottom=copy(manager.bottomIconsContainer);
            layout.BottomTarget=copy(manager.nextPosBottom);layout.Header=copy(manager.topPanel);layout.Clock=copy(manager.countDownText.gameObject).GetComponent<Text>();
            layout.Hearts=manager.hearts.Select(h=>copy(h.gameObject).GetComponent<Image>()).ToArray();
            foreach(var button in root.GetComponentsInChildren<Button>(true))button.onClick=new Button.ButtonClickedEvent();
            foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))if(!(component is UnityEngine.EventSystems.UIBehaviour) && component!=layout)Object.DestroyImmediate(component);
            foreach(var component in root.GetComponentsInChildren<Animator>(true))Object.DestroyImmediate(component);
            foreach(var component in root.GetComponentsInChildren<GraphicRaycaster>(true))Object.DestroyImmediate(component);
            foreach(var component in root.GetComponentsInChildren<CanvasScaler>(true))Object.DestroyImmediate(component);
            foreach(var component in root.GetComponentsInChildren<Canvas>(true))Object.DestroyImmediate(component);
            foreach(var board in layout.Boards)board.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/Saga/"+variant+".prefab");Object.DestroyImmediate(root);
        }
        Debug.Log("SAGA_BOXES_TEMPLATES_EXPORTED");
    }
}
