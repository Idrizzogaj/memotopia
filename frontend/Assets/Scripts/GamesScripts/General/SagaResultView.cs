using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// All Saga games share one result contract. Training keeps its original results.
public sealed class SagaResultView : MonoBehaviour
{
    public int Stop {get;private set;}
    public bool Won {get;private set;}
    public bool Consumed {get;private set;}

    public static GameObject Show(Transform parent,int stop,bool won,int stars,string score,UnityAction replay,UnityAction map){
        var panel=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/"+(won?"WinPanel":"LosePanel")),parent,false);
        panel.name="Saga result";
        var view=panel.AddComponent<SagaResultView>();view.Stop=stop;view.Won=won;
        var content=panel.transform.GetChild(0);
        string title="SAGA LEVEL "+stop;
        if(won){
            content.GetChild(0).GetChild(0).GetComponent<Text>().text=title;
            content.GetChild(10).GetChild(0).GetComponent<Text>().text=score;
            foreach(var fill in panel.GetComponentsInChildren<StarFillScript>(true)){
                fill.EarnedStarsOverride=stars;
                if(!Application.isPlaying){
                    var images=new[]{fill.leftStar,fill.midStar,fill.rightStar};var visible=new[]{fill.leftStarG,fill.midStarG,fill.rightStarG};
                    for(int i=0;i<3;i++){visible[i].SetActive(i<stars);images[i].GetComponent<Image>().fillAmount=i<stars?1:0;}
                }
            }
            var replayButton=content.GetChild(11).GetComponent<Button>();var mapButton=content.GetChild(12).GetComponent<Button>();
            foreach(var button in new[]{replayButton,mapButton}){var rect=button.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(rect.anchoredPosition.x,-215);}
            Bind(view,replayButton,"REPLAY",replay);Bind(view,mapButton,"MAP",map);
        }else{
            var heading=content.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text.Trim().Equals("Game Over",StringComparison.OrdinalIgnoreCase));
            if(heading!=null)heading.text=title;
            var replayButton=content.GetChild(2).GetComponent<Button>();var mapButton=content.GetChild(3).GetComponent<Button>();
            replayButton.onClick=new Button.ButtonClickedEvent();mapButton.onClick=new Button.ButtonClickedEvent();
            replayButton.onClick.AddListener(()=>view.Use(replay));mapButton.onClick.AddListener(()=>view.Use(map));
            replayButton.GetComponentInChildren<Text>().text="REPLAY";mapButton.GetComponentInChildren<Text>().text="MAP";
        }
        // A manual route out must remain available even if automatic navigation fails.
        foreach(var button in panel.GetComponentsInChildren<Button>(true))button.interactable=true;
        return panel;
    }
    static void Bind(SagaResultView view,Button button,string caption,UnityAction action){button.gameObject.SetActive(true);button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(()=>view.Use(action));var text=button.GetComponentInChildren<Text>(true);if(text!=null)text.text=caption;}
    void Use(UnityAction action){if(Consumed)return;Consumed=true;action();}
}
